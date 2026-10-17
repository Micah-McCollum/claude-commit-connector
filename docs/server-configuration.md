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
out of shell history. For dev/test only, the connector also accepts the
`RANGERMSP_WEBAPI_PASSWORD` environment variable.
