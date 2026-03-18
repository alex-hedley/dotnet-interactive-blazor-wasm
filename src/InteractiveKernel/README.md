# InteractiveKernel

A **WASI-targeted** console app that wraps the [.NET Interactive](https://github.com/dotnet/interactive) `CSharpKernel` for compiling and executing C# code.

Compiled to WebAssembly via the `wasi-wasm` runtime identifier, it can be run in any WASI-capable runtime — including **in the browser** using a JavaScript WASI shim.

## How it works

```
stdin  →  { "code": "<C# to run>" }  (one JSON object per line)
stdout ←  { "output": "...", "errors": [] }
```

Each line of stdin is treated as one execution request. The result is written back to stdout as a single line of JSON.

## Prerequisites

### Build requirements

- .NET 10 SDK
- `wasi-experimental` workload:
  ```bash
  dotnet workload install wasi-experimental
  ```

### Publish requirements

Publishing to a `.wasm` binary additionally requires the [wasi-sdk](https://github.com/WebAssembly/wasi-sdk/releases) C/C++ toolchain. Set `WASI_SDK_PATH` to its installation directory:

```bash
export WASI_SDK_PATH=/opt/wasi-sdk
```

## Building

```bash
# Compile to WASI-targeted DLL (no wasi-sdk needed)
dotnet build src/InteractiveKernel

# Publish to a self-contained .wasm binary (requires wasi-sdk + WASI_SDK_PATH)
dotnet publish src/InteractiveKernel -c Release
```

The published output is in `bin/Release/net10.0/wasi-wasm/publish/`.

## Running

### wasmtime (CLI)

```bash
echo '{"code":"Console.WriteLine(\"Hello from .NET Interactive WASI!\");"}' \
  | wasmtime bin/Release/net10.0/wasi-wasm/publish/dotnet.wasm --dir=.
# → {"output":"Hello from .NET Interactive WASI!\n","errors":[]}
```

### Browser (JavaScript WASI shim)

Use the [`@bjorn3/browser_wasi_shim`](https://github.com/bjorn3/browser_wasi_shim) npm package to load the `.wasm` in a browser:

```js
import { WASI, OpenFile, File, ConsoleStdout } from "@bjorn3/browser_wasi_shim";

const wasi = new WASI(
  [],            // args
  [],            // env
  [
    new OpenFile(new File([])),          // stdin  (fd 0)
    ConsoleStdout.lineBuffered(line => console.log(line)), // stdout (fd 1)
    ConsoleStdout.lineBuffered(line => console.error(line)), // stderr (fd 2)
  ]
);

const wasmBytes = await fetch("dotnet.wasm").then(r => r.arrayBuffer());
const { instance } = await WebAssembly.instantiate(wasmBytes, {
  wasi_snapshot_preview1: wasi.wasiImport
});

// Feed code via the shim's stdin, read results from stdout
```

See `src/BlazorApp` for the full Blazor WASM frontend that integrates with this kernel.
