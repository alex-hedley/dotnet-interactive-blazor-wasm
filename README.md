# .NET Interactive Blazor (WASM)

[![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Blazor](https://img.shields.io/badge/blazor-%235C2D91.svg?style=for-the-badge&logo=blazor&logoColor=white)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-lightgrey.svg?style=for-the-badge)](LICENSE) <!-- https://opensource.org/licenses/MIT -->

A C# interactive coding environment built with [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) (frontend) and [.NET Interactive](https://github.com/dotnet/interactive) (execution backend).

## Architecture

```
┌──────────────────────────────────────────────────────┐
│  Browser                                             │
│                                                      │
│  BlazorApp (WASM)  ──HTTP──▶  POST /api/execute      │
│  src/BlazorApp/                                      │
└──────────────────────────────────────────────────────┘
                              │
              ┌───────────────▼────────────────────────┐
              │  InteractiveServer (ASP.NET Core)       │
              │  src/InteractiveServer/                 │
              │                                        │
              │  dotnet-interactive stdio ─────────────▶ C# kernel
              └────────────────────────────────────────┘

── WASI alternative (in-browser) ──────────────────────
              src/InteractiveKernel/  (wasi-wasm)
              Microsoft.DotNet.Interactive.CSharpKernel
              communicates via stdin / stdout JSON
              run with: wasmtime  or  a browser WASI shim
```

| Component | Description |
|---|---|
| **BlazorApp** | Blazor WASM frontend — code editor UI, calls the API |
| **InteractiveServer** | ASP.NET Core server wrapping `dotnet-interactive` — for local / server deployment |
| **InteractiveKernel** | WASI console app using `CSharpKernel` directly — compile to `.wasm`, run with `wasmtime` or a browser WASI shim |

## Getting Started

### Option A — Local server (`InteractiveServer`)

#### 1. Prerequisites

```bash
dotnet tool install -g Microsoft.dotnet-interactive
```

#### 2. Start the server

```bash
dotnet run --project src/InteractiveServer
# Listening on http://localhost:5000
```

#### 3. Start the Blazor WASM app

```bash
dotnet run --project src/BlazorApp
# Open http://localhost:5136 in your browser
```

### Option B — WASI kernel (`InteractiveKernel`)

#### 1. Prerequisites

```bash
dotnet workload install wasi-experimental-net8

# For producing the .wasm binary you also need wasi-sdk:
# https://github.com/WebAssembly/wasi-sdk/releases
export WASI_SDK_PATH=/opt/wasi-sdk
```

#### 2. Build and publish to WASM

```bash
dotnet publish src/InteractiveKernel -c Release
# Output: src/InteractiveKernel/bin/Release/net8.0/wasi-wasm/publish/
```

#### 3. Run with wasmtime

```bash
echo '{"code":"Console.WriteLine(\"Hello from WASI!\");"}' \
  | wasmtime dotnet.wasm --dir=.
```

See [`src/InteractiveKernel/README.md`](src/InteractiveKernel/README.md) for details on browser hosting with a JavaScript WASI shim.

## Docs

- [Info](docs/README.md)
- [Setup notes](docs/SETUP.md)
- [Reference notes](docs/NOTES.md)

