# BlazorApp — .NET Interactive REPL

A Blazor WebAssembly standalone app that provides an interactive C# REPL running **entirely client-side** in the browser.

No server is required — all code compilation and execution happens inside .NET 10 WebAssembly.

## How it works

1. On startup the app reads `_framework/dotnet.js` to discover all managed assemblies. In .NET 10 the boot config is embedded directly in `dotnet.js` as JSON between `/*json-start*/` and `/*json-end*/` markers; it has two assembly categories — `coreAssembly` (which includes `System.Private.CoreLib`) and `assembly`. Both categories must be loaded so Roslyn has the full set of type definitions. A fallback path for .NET 8 (`blazor.boot.json` with `resources.assembly` as an object) is retained for backward compatibility.
2. Each assembly is fetched as raw bytes and wrapped in a `MetadataReference.CreateFromImage(bytes)` — this is the key technique that works in WASM where `Assembly.Location` is always empty.
3. A `CSharpCompilation.CreateScriptCompilation` is built with those references, so Roslyn can compile arbitrary C# snippets.
4. The emitted IL is loaded with `Assembly.Load(bytes)` and the script entry point (`<Factory>`) is invoked via reflection.
5. `Console.SetOut` redirects standard output so `Console.WriteLine` calls are captured and shown in the output panel.

### Why not `CSharpScript.RunAsync`?

The high-level scripting API (`Microsoft.CodeAnalysis.CSharp.Scripting`) internally calls `MetadataReference.CreateFromAssemblyInternal(assembly)`, which reads `assembly.Location`.  In a WASM host all locations are empty, causing a `NotSupportedException`.  Using `CSharpCompilation` directly avoids this path entirely.

### Why `<WasmEnableWebcil>false</WasmEnableWebcil>`?

By default, .NET 8 packages managed assemblies in a WebCIL container (a WebAssembly module wrapping the PE bytes). `MetadataReference.CreateFromImage` requires raw PE/ECMA-335 bytes, so WebCIL files fail. Setting `WasmEnableWebcil` to `false` restores plain `.dll` output.

## Build

```bash
dotnet build -c Debug
dotnet build -c Release
```

## Run locally

```bash
dotnet run --project src/BlazorApp/BlazorApp.csproj
```

Then open `http://localhost:5215` (or the URL shown in the terminal).

## Publish

```bash
dotnet publish src/BlazorApp/BlazorApp.csproj -c Release -o release
```

The published output lands in `release/wwwroot/` and is ready to be hosted on GitHub Pages or any static file host.
