# .NET Interactive Blazor (WASM)

[![C#](https://img.shields.io/badge/c%23-%23239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Blazor](https://img.shields.io/badge/blazor-%235C2D91.svg?style=for-the-badge&logo=blazor&logoColor=white)](https://dotnet.microsoft.com/en-us/apps/aspnet/web-apps/blazor)
[![License: MIT](https://img.shields.io/badge/License-MIT-lightgrey.svg?style=for-the-badge)](LICENSE) <!-- https://opensource.org/licenses/MIT -->

A C# interactive coding environment built with [Blazor WebAssembly](https://dotnet.microsoft.com/apps/aspnet/web-apps/blazor) (frontend) and [.NET Interactive](https://github.com/dotnet/interactive) (execution backend).

## Architecture

```
┌───────────────────────────────────────────┐
│  Browser                                  │
│                                           │
│  BlazorApp (WASM)  ──HTTP──▶  POST /api/execute
│  src/BlazorApp/               │           │
└───────────────────────────────┼───────────┘
                                │
                    ┌───────────▼──────────────────┐
                    │  InteractiveServer (ASP.NET)  │
                    │  src/InteractiveServer/        │
                    │                               │
                    │  dotnet-interactive stdio ────▶ C# kernel
                    └───────────────────────────────┘
```

| Component | Description |
|---|---|
| **BlazorApp** | Blazor WASM frontend — code editor UI, calls the API |
| **InteractiveServer** | ASP.NET Core server — wraps `dotnet-interactive` to execute C# code |

## Getting Started

### 1. Prerequisites

```bash
dotnet tool install -g Microsoft.dotnet-interactive
```

### 2. Start the InteractiveServer

```bash
dotnet run --project src/InteractiveServer
# Listening on http://localhost:5000
```

### 3. Start the Blazor WASM app

```bash
dotnet run --project src/BlazorApp
# Open http://localhost:5136 in your browser
```

## Docs

- [Info](docs/README.md)
- [Setup notes](docs/SETUP.md)
- [Reference notes](docs/NOTES.md)

