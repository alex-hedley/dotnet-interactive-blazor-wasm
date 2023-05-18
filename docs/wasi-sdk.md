# wasi-sdk

Use package

`dotnet add package Wasi.Sdk --prerelease`

[MyApp.csproj](../src/MyApp/MyApp.csproj): `<PackageReference Include="Wasi.Sdk" Version="0.1.4-preview.10020" />`

`dotnet build`

```bash
/usr/local/share/dotnet/packs/Microsoft.NET.Runtime.WebAssembly.Wasi.Sdk/8.0.0-preview.4.23259.5/Sdk/WasiApp.Native.targets(369,5): error : Cannot find ClangExecutable=/Users/*/.wasi-sdk/wasi-sdk-16.0/bin/clang [/*/src/MyApp/MyApp.csproj]
```

Download and add to path?

`mkdir /Users/*/.wasi-sdk/`

copy download

`/Users/*/.wasi-sdk/wasi-sdk-20.0`

Update path

`/Users/*/.zshrc`

```bash
export WASI_SDK_PATH="$HOME/.wasi-sdk/wasi-sdk-20.0"

export PATH="$WASI_SDK_PATH/bin:$PATH"
```

Refresh your terminal

> “clang-16” cannot be opened because the developer cannot be verified.
> macOS cannot verify that this app is free from malware.

> “lld” cannot be opened because the developer cannot be verified.

⛔️ Not sure what to do here.

## Links

- [wasi-sdk](https://github.com/WebAssembly/wasi-sdk)
- https://github.com/WebAssembly/wasi-sdk/releases/tag/wasi-sdk-20
  - https://github.com/WebAssembly/wasi-sdk/releases/download/wasi-sdk-20/wasi-sdk-20.0-macos.tar.gz

- [nuget](https://www.nuget.org/packages/Wasi.Sdk/0.1.4-preview.10020)
- [source](https://github.com/SteveSandersonMS/dotnet-wasi-sdk)
