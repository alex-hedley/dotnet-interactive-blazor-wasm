# InteractiveServer

An ASP.NET Core HTTP server that wraps the [.NET Interactive](https://github.com/dotnet/interactive) CLI (`dotnet-interactive`) and exposes a simple REST API for code execution.

The server is the backend for the **BlazorApp** Blazor WASM frontend.

## Prerequisites

Install the `dotnet-interactive` global tool:

```bash
dotnet tool install -g Microsoft.dotnet-interactive
```

## Running

```bash
dotnet run --project src/InteractiveServer
```

The server starts on **http://localhost:5000** by default.

## API

### `GET /health`

Returns kernel status.

```json
{ "status": "ok", "kernelReady": true }
```

### `POST /api/execute`

Execute C# code using the .NET Interactive C# kernel.

**Request:**
```json
{ "code": "Console.WriteLine(\"Hello!\");" }
```

**Response:**
```json
{ "output": "Hello!\n", "errors": [] }
```

Errors (compilation failures, runtime exceptions) are returned in the `errors` array.
