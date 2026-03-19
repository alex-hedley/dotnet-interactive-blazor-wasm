using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
    {
        // In development allow all origins so the Blazor dev server on a different port can reach us.
        // In production restrict this to the specific origin(s) hosting the BlazorApp.
        if (builder.Environment.IsDevelopment())
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        else
            policy.WithOrigins(
                    builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>())
                  .AllowAnyHeader()
                  .AllowAnyMethod();
    }));

builder.Services.AddSingleton<DotNetInteractiveKernel>();

var app = builder.Build();

// Initialise the kernel on startup
var kernel = app.Services.GetRequiredService<DotNetInteractiveKernel>();
await kernel.InitialiseAsync();

app.UseCors();

// POST /api/execute  { "code": "...", "language": "csharp" }  →  { "output": "...", "errors": ["..."] }
// Supported languages: "csharp" (default), "fsharp"
app.MapPost("/api/execute", async (ExecuteRequest req, DotNetInteractiveKernel k) =>
{
    if (string.IsNullOrWhiteSpace(req.Code))
        return Results.BadRequest(new ExecuteResponse("", new[] { "Code must not be empty." }));

    var language = req.Language?.ToLowerInvariant() switch
    {
        "fsharp" or "f#" => "fsharp",
        "csharp" or "c#" or null or "" => "csharp",
        _ => null
    };

    if (language is null)
        return Results.BadRequest(new ExecuteResponse("",
            new[] { $"Unsupported language '{req.Language}'. Supported: csharp, fsharp." }));

    var result = await k.ExecuteAsync(req.Code, language, TimeSpan.FromSeconds(30));
    return Results.Ok(result);
});

app.MapGet("/health", () => Results.Ok(new { status = "ok", kernelReady = kernel.IsReady }));

app.Run();

// ── Records ─────────────────────────────────────────────────────────────────
record ExecuteRequest(string Code, string? Language = null);
record ExecuteResponse(string Output, IReadOnlyList<string> Errors);

// ── .NET Interactive kernel wrapper ─────────────────────────────────────────

/// <summary>
/// Manages a single <c>dotnet-interactive stdio</c> child process.
/// Commands and events travel over the stdio JSON protocol that
/// .NET Interactive defines.
/// </summary>
sealed class DotNetInteractiveKernel : IAsyncDisposable
{
    private Process? _process;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    // token → completion source for that submission
    private readonly ConcurrentDictionary<string, PendingSubmission> _pending = new();
    private int _tokenCounter;

    public bool IsReady { get; private set; }

    public async Task InitialiseAsync(CancellationToken ct = default)
    {
        _process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet-interactive",
                Arguments = "stdio --default-kernel csharp",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                Environment =
                {
                    ["DOTNET_INTERACTIVE_CLI_TELEMETRY_OPTOUT"] = "1"
                }
            }
        };

        _process.Start();

        // Pump stderr to console so it can be monitored
        _ = Task.Run(() =>
        {
            string? line;
            while ((line = _process.StandardError.ReadLine()) != null)
                Console.Error.WriteLine($"[interactive-stderr] {line}");
        });

        // Pump stdout, dispatch events to pending submissions
        _ = Task.Run(ReadEventsLoopAsync);

        // Wait for KernelReady
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(60));
        while (!IsReady && !cts.Token.IsCancellationRequested)
            await Task.Delay(100, cts.Token).ConfigureAwait(false);

        if (!IsReady)
            throw new TimeoutException("dotnet-interactive did not become ready within 60 s.");
    }

    public async Task<ExecuteResponse> ExecuteAsync(string code, string language, TimeSpan timeout)
    {
        var token = $"tok-{Interlocked.Increment(ref _tokenCounter)}";
        var pending = new PendingSubmission();
        _pending[token] = pending;

        var envelope = new
        {
            commandType = "SubmitCode",
            command = new { code, targetKernelName = language },
            token,
            routingSlip = Array.Empty<string>()
        };

        await _sendLock.WaitAsync();
        try
        {
            var json = JsonSerializer.Serialize(envelope);
            await _process!.StandardInput.WriteLineAsync(json);
            await _process.StandardInput.FlushAsync();
        }
        finally
        {
            _sendLock.Release();
        }

        using var cts = new CancellationTokenSource(timeout);
        try
        {
            await pending.Completion.Task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException)
        {
            _pending.TryRemove(token, out _);
            return new ExecuteResponse("", new[] { $"Execution timed out after {timeout.TotalSeconds:0} s." });
        }
        finally
        {
            _pending.TryRemove(token, out _);
        }

        return new ExecuteResponse(pending.Output.ToString(), pending.Errors);
    }

    private async Task ReadEventsLoopAsync()
    {
        try
        {
            string? line;
            while ((line = await _process!.StandardOutput.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                DispatchEvent(line);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[interactive] stdout reader error: {ex.Message}");
        }
    }

    private void DispatchEvent(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var eventType = root.TryGetProperty("eventType", out var et) ? et.GetString() : null;

            // ── KernelReady ────────────────────────────────────────────────
            if (eventType == "KernelReady")
            {
                IsReady = true;
                return;
            }

            // Retrieve the submission token so we can route back to the caller
            var token = GetToken(root);
            if (token is null || !_pending.TryGetValue(token, out var pending)) return;

            switch (eventType)
            {
                case "StandardOutputValueProduced":
                    foreach (var fv in GetFormattedValues(root))
                        pending.Output.Append(fv);
                    break;

                case "ReturnValueProduced":
                case "DisplayedValueProduced":
                    foreach (var fv in GetFormattedValues(root))
                        pending.Output.AppendLine(fv);
                    break;

                case "ErrorProduced":
                    if (root.TryGetProperty("event", out var errEvt) &&
                        errEvt.TryGetProperty("message", out var msg))
                        pending.Errors.Add(msg.GetString() ?? "Unknown error");
                    break;

                case "DiagnosticsProduced":
                    if (root.TryGetProperty("event", out var diagEvt) &&
                        diagEvt.TryGetProperty("formattedDiagnostics", out var diags))
                        foreach (var d in diags.EnumerateArray())
                        {
                            var txt = d.GetString();
                            if (!string.IsNullOrWhiteSpace(txt))
                                pending.Errors.Add(txt);
                        }
                    break;

                case "CommandFailed":
                    if (root.TryGetProperty("event", out var failEvt) &&
                        failEvt.TryGetProperty("message", out var failMsg))
                        pending.Errors.Add(failMsg.GetString() ?? "Command failed");
                    pending.Completion.TrySetResult(true);
                    break;

                case "CommandSucceeded":
                    pending.Completion.TrySetResult(true);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[interactive] event dispatch error: {ex.Message}");
        }
    }

    private static string? GetToken(JsonElement root)
    {
        if (root.TryGetProperty("command", out var cmd) &&
            cmd.ValueKind == JsonValueKind.Object &&
            cmd.TryGetProperty("token", out var tok))
            return tok.GetString();
        return null;
    }

    private static IEnumerable<string> GetFormattedValues(JsonElement root)
    {
        if (!root.TryGetProperty("event", out var evt)) yield break;
        if (!evt.TryGetProperty("formattedValues", out var fvs)) yield break;
        foreach (var fv in fvs.EnumerateArray())
        {
            if (fv.TryGetProperty("value", out var val))
            {
                var v = val.GetString();
                if (v != null) yield return v;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is { HasExited: false })
        {
            try { _process.Kill(); }
            catch (InvalidOperationException) { /* process already exited */ }
            catch (Exception ex) { Console.Error.WriteLine($"[interactive] error killing process: {ex.Message}"); }
        }
        _process?.Dispose();
        _sendLock.Dispose();
        await ValueTask.CompletedTask;
    }
}

sealed class PendingSubmission
{
    public TaskCompletionSource<bool> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public StringBuilder Output { get; } = new();
    public List<string> Errors { get; } = new();
}
