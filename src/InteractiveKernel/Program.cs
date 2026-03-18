// InteractiveKernel — WASI console app
//
// This program is compiled to WebAssembly (wasi-wasm) and wraps the
// Microsoft.DotNet.Interactive kernels directly — no child-process
// spawning, no HTTP server — so it can run inside any WASI runtime
// (wasmtime, Node.js with wasi support, or a browser via a JS WASI shim).
//
// Communication protocol (stdin / stdout):
//
//   stdin  ← one JSON object per line:
//             { "code": "<code to execute>", "language": "csharp" }
//
//             Supported languages: "csharp" (default), "fsharp"
//
//   stdout → one JSON object per line:
//             { "output": "<captured stdout>", "errors": ["..."] }
//
// Example (wasmtime):
//   echo '{"code":"Console.WriteLine(\"Hello!\");","language":"csharp"}' \
//     | wasmtime dotnet.wasm --dir=.

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.FSharp;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

// Create both kernels once at startup so state is preserved across submissions.
using var csharpKernel = new CSharpKernel();
using var fsharpKernel = new FSharpKernel();

const int TimeoutSeconds = 30;

var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

string? line;
while ((line = Console.ReadLine()) != null)
{
    line = line.Trim();
    if (string.IsNullOrEmpty(line)) continue;

    KernelRequest? req;
    try
    {
        req = JsonSerializer.Deserialize<KernelRequest>(line, jsonOptions);
    }
    catch
    {
        WriteResponse(null, new[] { "Invalid JSON request." });
        continue;
    }

    if (req is null || string.IsNullOrWhiteSpace(req.Code))
    {
        WriteResponse(null, new[] { "Request must contain a non-empty 'code' field." });
        continue;
    }

    // Select kernel based on the optional "language" field (default: csharp).
    Kernel kernel = (req.Language?.ToLowerInvariant()) switch
    {
        "fsharp" or "f#" => fsharpKernel,
        "csharp" or "c#" or null or "" => csharpKernel,
        _ => throw new InvalidOperationException($"Unsupported language '{req.Language}'. Supported values: csharp, fsharp.")
    };

    var output = new StringBuilder();
    var errors = new List<string>();
    var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

    using var sub = kernel.KernelEvents.Subscribe(e =>
    {
        switch (e)
        {
            case StandardOutputValueProduced so:
                foreach (var fv in so.FormattedValues)
                    output.Append(fv.Value);
                break;

            case ReturnValueProduced rv:
                foreach (var fv in rv.FormattedValues)
                    output.AppendLine(fv.Value);
                break;

            case DisplayedValueProduced dv:
                foreach (var fv in dv.FormattedValues)
                    output.AppendLine(fv.Value);
                break;

            case DiagnosticsProduced dp:
                foreach (var d in dp.FormattedDiagnostics)
                    if (!string.IsNullOrWhiteSpace(d.Value))
                        errors.Add(d.Value);
                break;

            case CommandFailed cf:
                errors.Add(cf.Message);
                tcs.TrySetResult(false);
                break;

            case CommandSucceeded:
                tcs.TrySetResult(true);
                break;
        }
    });

    try
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
        await kernel.SendAsync(new SubmitCode(req.Code), cts.Token);

        await tcs.Task.WaitAsync(TimeSpan.FromSeconds(TimeoutSeconds));
    }
    catch (InvalidOperationException ex)
    {
        errors.Add(ex.Message);
    }
    catch (TimeoutException)
    {
        errors.Add("Execution timed out after 30 seconds.");
    }

    WriteResponse(output.ToString(), errors.ToArray());
}

static void WriteResponse(string? output, string[]? errors)
{
    var response = new KernelResponse(output ?? string.Empty, errors ?? Array.Empty<string>());
    Console.WriteLine(JsonSerializer.Serialize(response));
}

// ── Request / response types ──────────────────────────────────────────────────

record KernelRequest(string Code, string? Language = null);
record KernelResponse(string Output, IReadOnlyList<string> Errors);
