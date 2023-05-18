# Setup

`dotnet workload install wasi-experimental`

> Inadequate permissions. Run the command with elevated privileges.

`sudo dotnet workload install wasi-experimental`

```bash
Welcome to .NET 7.0!
---------------------
SDK Version: 7.0.302

Telemetry
---------
The .NET tools collect usage data in order to help us improve your experience. It is collected by Microsoft and shared with the community. You can opt-out of telemetry by setting the DOTNET_CLI_TELEMETRY_OPTOUT environment variable to '1' or 'true' using your favorite shell.

Read more about .NET CLI Tools telemetry: https://aka.ms/dotnet-cli-telemetry

----------------
Installed an ASP.NET Core HTTPS development certificate.
To trust the certificate run 'dotnet dev-certs https --trust' (Windows and macOS only).
Learn about HTTPS: https://aka.ms/dotnet-https
----------------
Write your first app: https://aka.ms/dotnet-hello-world
Find out what's new: https://aka.ms/dotnet-whats-new
Explore documentation: https://aka.ms/dotnet-docs
Report issues and find source on GitHub: https://github.com/dotnet/core
Use 'dotnet --help' to see available commands or visit: https://aka.ms/dotnet-cli
--------------------------------------------------------------------------------------
Workload ID wasi-experimental is not recognized.
```

.NET 8.0
- https://dotnet.microsoft.com/en-us/download/dotnet/8.0

```bash
Welcome to .NET 8.0!
---------------------
SDK Version: 8.0.100-preview.4.23260.5

Telemetry
---------
The .NET tools collect usage data in order to help us improve your experience. It is collected by Microsoft and shared with the community. You can opt-out of telemetry by setting the DOTNET_CLI_TELEMETRY_OPTOUT environment variable to '1' or 'true' using your favorite shell.

Read more about .NET CLI Tools telemetry: https://aka.ms/dotnet-cli-telemetry

----------------
Installed an ASP.NET Core HTTPS development certificate.
To trust the certificate run 'dotnet dev-certs https --trust' (Windows and macOS only).
Learn about HTTPS: https://aka.ms/dotnet-https
----------------
Write your first app: https://aka.ms/dotnet-hello-world
Find out what's new: https://aka.ms/dotnet-whats-new
Explore documentation: https://aka.ms/dotnet-docs
Report issues and find source on GitHub: https://github.com/dotnet/core
Use 'dotnet --help' to see available commands or visit: https://aka.ms/dotnet-cli
--------------------------------------------------------------------------------------

Skipping NuGet package signature verification.
Installing workload manifest microsoft.net.sdk.android version 34.0.0-preview.4.273…
Installing workload manifest microsoft.net.sdk.ios version 16.4.8377-net8-p4…
Installing workload manifest microsoft.net.sdk.maccatalyst version 16.4.8377-net8-p4…
Installing workload manifest microsoft.net.sdk.macos version 13.3.8377-net8-p4…
Installing workload manifest microsoft.net.sdk.maui version 8.0.0-preview.4.8333…
Installing workload manifest microsoft.net.sdk.tvos version 16.4.8377-net8-p4…
Installing pack Microsoft.NET.Runtime.WebAssembly.Wasi.Sdk version 8.0.0-preview.4.23259.5...
Writing workload pack installation record for Microsoft.NET.Runtime.WebAssembly.Wasi.Sdk version 8.0.0-preview.4.23259.5...
Installing pack Microsoft.NETCore.App.Runtime.Mono.wasi-wasm version 8.0.0-preview.4.23259.5...
Writing workload pack installation record for Microsoft.NETCore.App.Runtime.Mono.wasi-wasm version 8.0.0-preview.4.23259.5...
Installing pack Microsoft.NET.Runtime.WebAssembly.Templates version 8.0.0-preview.4.23259.5...
Writing workload pack installation record for Microsoft.NET.Runtime.WebAssembly.Templates version 8.0.0-preview.4.23259.5...
Installing pack Microsoft.NET.Runtime.MonoAOTCompiler.Task version 8.0.0-preview.4.23259.5...
Writing workload pack installation record for Microsoft.NET.Runtime.MonoAOTCompiler.Task version 8.0.0-preview.4.23259.5...
Installing pack Microsoft.NET.Runtime.MonoTargets.Sdk version 8.0.0-preview.4.23259.5...
Writing workload pack installation record for Microsoft.NET.Runtime.MonoTargets.Sdk version 8.0.0-preview.4.23259.5...
Garbage collecting for SDK feature band(s) 7.0.100 8.0.100-preview.4...

Successfully installed workload(s) wasi-experimental.
```

`dotnet new list wasi`

```bash
These templates matched your input: 'wasi'

Template Name     Short Name   Language  Tags            
----------------  -----------  --------  ----------------
Wasi Console App  wasiconsole  [C#]      Wasi/WasiConsole
```

`dotnet new wasiconsole -o MyApp`