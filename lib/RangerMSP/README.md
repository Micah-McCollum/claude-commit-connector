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

The RangerMSP wiki warns against copying the *native* RangerMSP DLLs out of
`RangerMSP\ThirdParty\UserDev`. Those DLLs are for local-database mode. This
connector uses the Web API (`UseWebAPI = true`), so it should not need them.
If `CRM.Application.Initialize` still asks for them, set `RangerMsp:DllFolder`
in `appsettings.json` to your `UserDev` folder.
