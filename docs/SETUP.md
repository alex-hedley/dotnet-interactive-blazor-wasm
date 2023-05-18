# Setup

## PreReqs

- [.NET 8.0](dotNET8.md)
- [wasitime](wasitime.md)
- [wasi-sdk](wasi-sdk.md)

## Steps

`dotnet workload install wasi-experimental`

`dotnet new list wasi`

```bash
These templates matched your input: 'wasi'

Template Name     Short Name   Language  Tags            
----------------  -----------  --------  ----------------
Wasi Console App  wasiconsole  [C#]      Wasi/WasiConsole
```

`dotnet new wasiconsole -o MyApp`

Update [Program.cs](../src/MyApp/Program.cs).

`cd MyApp`

`dotnet build`

```bash
MSBuild version 17.7.0-preview-23251-02+59879b095 for .NET
  Determining projects to restore...
  Restored /*/src/MyApp/MyApp.csproj (in 1.52 sec).
/usr/local/share/dotnet/sdk/8.0.100-preview.4.23260.5/Sdks/Microsoft.NET.Sdk/targets/Microsoft.NET.RuntimeIdentifierInference.targets(287,5): message NETSDK1057: You are using a preview version of .NET. See: https://aka.ms/dotnet-support-policy [/*/src/MyApp/MyApp.csproj]
  MyApp -> /*/src/MyApp/bin/Debug/net8.0/wasi-wasm/MyApp.dll
```

`dotnet run`

- [wasitime](wasitime.md)

```bash
WasmAppHost --runtime-config /*/src/MyApp/bin/Debug/net8.0/wasi-wasm/AppBundle/MyApp.runtimeconfig.json
Running: wasmtime run --dir . -- dotnet.wasm MyApp
Using working directory: /*/src/MyApp/bin/Debug/net8.0/wasi-wasm/AppBundle
Hello, Wasi Console!
Hello, this is Wasm!
```

`wasmtime bin/Debug/net8.0/browser-wasm/AppBundle/MyApp.wasm`

```bash
Error: if you're trying to run a precompiled module, pass --allow-precompiled

Caused by:
    0: failed to read input file
    1: No such file or directory (os error 2)
```

⛔️ Not sure what to do here, with the above error.

Added `<WasmSingleFileBundle>true</WasmSingleFileBundle>` to [MyApp.csproj](../src/MyApp/MyApp.csproj) but got the following error:

```bash
/usr/local/share/dotnet/packs/Microsoft.NET.Runtime.WebAssembly.Wasi.Sdk/8.0.0-preview.4.23259.5/Sdk/WasiApp.Native.targets(54,5): error : Could not find wasi-sdk. Either set $(WASI_SDK_PATH), or use workloads to get the sdk. SDK is required for building native files. [/*/src/MyApp/MyApp.csproj]
```

From 📼

> C:\tools\wasi-sdk-20.0\bin\clang.exe

- [wasi-sdk](https://github.com/WebAssembly/wasi-sdk)
- https://github.com/WebAssembly/wasi-sdk/releases/tag/wasi-sdk-20
  - https://github.com/WebAssembly/wasi-sdk/releases/download/wasi-sdk-20/wasi-sdk-20.0-macos.tar.gz

- [wasi-sdk](wasi-sdk.md)

...
