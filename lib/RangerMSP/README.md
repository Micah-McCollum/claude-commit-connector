# RangerMSP C# API library (CRMLib.dll)

This folder holds `CRMLib.dll`, the RangerMSP C# API library. Git ignores
everything in this folder except this README, because the library is covered
by the RangerMSP developer agreement.

## Getting CRMLib.dll

1. Accept the developer agreement and download the API for RangerMSP 7.0+:
   https://www.rangermsp.com/developer-agreement-and-api-download/
2. Follow the RangerMSP wiki page **CSharp API Library Setup**:
   https://www.rangermsp.com/wiki/CSharp_API_Library_Setup
   The library ships as C# source. Open the CRMLib solution in Visual Studio
   and build it in **Release** mode to produce `CRMLib.dll`.
3. Copy the built `CRMLib.dll` into this folder:
   `lib/RangerMSP/CRMLib.dll`

To keep the DLL somewhere else, build with
`dotnet build /p:CRMLibPath=C:\path\to\CRMLib.dll`.

## CRMLib.dll is not the same as the DLL folder

`CRMLib.dll` (this folder) is the .NET library the connector is compiled
against. Separately, at runtime CRMLib needs `RangerMsp:DllFolder` to point at
the RangerMSP server's `ThirdParty\UserDev` folder, **even in Web API mode**.
Without it, it fails with "DLL folder must be set". The RangerMSP wiki warns
against copying those DLLs elsewhere, so from another machine point at a
read-only share of the server's folder instead. See "DLL folder" in
[docs/server-configuration.md](../../docs/server-configuration.md).
