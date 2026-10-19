# RangerMSP server configuration

Server-side setup the connector expects. Based on the RangerMSP wiki page
[Web API Server Configuration](https://www.rangermsp.com/wiki/Web_API_Server_Configuration).
Check names against your RangerMSP version.

## Web API

- The Web API runs inside the **CRM Server** service.
- It listens on port **4964** by default; the port is set in `CmtWSrv.ini`.
- If the connector runs on another machine, open that port in the firewall and
  put HTTPS in front of it.

## Modules and passwords

| Module | File | Purpose | Connector needs it? |
|---|---|---|---|
| DbQry | `CmtWPDbQry.ini` | Queries (reads) | Yes |
| DbEng | `CmtWPDbEng.ini` | Creates and edits | Not yet; turn it OFF until write tools exist |

- Give each module its **own** password. A strong one can be generated with:
  ```powershell
  $b = New-Object byte[] 24
  [Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($b)
  -join ($b | ForEach-Object { $_.ToString('x2') })
  ```
- Keep the passwords in a password manager or vault, never in a text file.
- Restart the **CRM Server** service after any `.ini` change.
- Restrict the `.ini` files (they contain the passwords) to SYSTEM,
  Administrators and the service account. Check current access with `icacls`.

## DLL folder (required, even in Web API mode)

CRMLib refuses to start without `DllFolder` ("DLL folder must be set"). Set
`RangerMsp:DllFolder` in `appsettings.json`:

| Connector runs on | `DllFolder` value |
|---|---|
| The RangerMSP server | The local path, e.g. `C:\RangerMSP\ThirdParty\UserDev` |
| Another machine | A UNC path to a read-only share, e.g. `\\RANGERSERVER\RangerUserDev` |

In `appsettings.json` every backslash must be doubled:
```json
"DllFolder": "\\\\RANGERSERVER\\RangerUserDev"
```

### Sharing only the UserDev folder, read-only (on the RangerMSP server)

The `Db` folder sits under the same `RangerMSP` folder. **Don't share
`RangerMSP` or `Db`.** Share only `UserDev`:

1. In Explorer, go to `...\RangerMSP\ThirdParty\`, right-click **UserDev** → **Properties** → **Sharing** → **Advanced Sharing…**
2. Tick **Share this folder** and set **Share name** to `RangerUserDev`.
3. Click **Permissions**: remove **Everyone**, then **Add…** the Windows account that runs Claude on the other machine (a domain account, or a matching local account) with **Read** only.
4. **OK** out of all dialogs.
5. On the other machine, signed in as that account, check it opens:
   ```powershell
   Test-Path "\\RANGERSERVER\RangerUserDev"
   ```
   It should print `True`.

Use the UNC path, not a mapped drive letter: drive letters belong to one login
session and programs started another way may not see them. Sharing the server's
folder, rather than copying the DLLs, also keeps them the same version as the
server after RangerMSP upgrades. Mismatched copies are a known cause of
failures.

`DbFolder` stays empty in Web API mode. Set it only if CRMLib asks for it next.

## Connector machine

Store **only the DbQry password** in Windows Credential Manager, logged in as
the Windows user that will run Claude:

Control Panel > Credential Manager > Windows Credentials > *Add a generic credential*

| Field | Value |
|---|---|
| Internet or network address | `RangerMSP-DbQry` (must match `RangerMsp:CredentialTarget`) |
| User name | anything, e.g. `claude` |
| Password | the DbQry password |

Use the Control Panel dialog rather than `cmdkey /pass:` so the password stays
out of shell history. The connector also accepts the
`RANGERMSP_WEBAPI_PASSWORD` environment variable; see
[environment-variable-setup.md](environment-variable-setup.md).
