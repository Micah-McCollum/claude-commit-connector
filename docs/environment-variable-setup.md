# Storing the Web API password in an environment variable

This guide stores the RangerMSP Web API **DbQry** password in the
`RANGERMSP_WEBAPI_PASSWORD` environment variable so the connector can read it.

> **Credential Manager is the more secure option.** A user environment variable
> is stored as plain text in the registry (`HKCU\Environment`). Any program you
> run, and any administrator, can read it. Windows Credential Manager encrypts
> the password for your account (see [server-configuration.md](server-configuration.md)).
> The connector checks Credential Manager first and only uses the environment
> variable if no credential named `RangerMSP-DbQry` exists.

---

## Before you start: which machine, which account, which password

| Question | Answer |
|---|---|
| **Which machine?** | The machine that runs **Claude Desktop and the connector** (`CommitConnector.McpServer.exe`). If that's the RangerMSP server itself, do it there. If Claude runs on a workstation, do it on the workstation. The RangerMSP server doesn't read this variable; its password lives in `CmtWPDbQry.ini`. |
| **Which Windows account?** | The account you're signed in as when you use Claude. Claude Desktop starts the connector as that user, so it only sees that user's variables. |
| **User or System variable?** | **User.** Every account and service on the machine can read a System variable. |
| **Which password?** | The **DbQry** (query) password only, never the DbEng one. The connector is read-only. |

You'll need:
- The new DbQry password from your password manager. If you haven't replaced the old shared password yet, do the password fix in [server-configuration.md](server-configuration.md) first.
- To be signed in to Windows as the account from the table above. Administrator rights aren't needed for a User variable.

---

## Step 1: Check that no Credential Manager entry would take priority

Open **PowerShell** (Start → type `PowerShell` → *Windows PowerShell*) and run:

```powershell
cmdkey /list:RangerMSP-DbQry
```

- If it prints `* NONE *`: continue to Step 2.
- If it lists a credential: the connector will use that and **ignore** the
  environment variable. Either keep using Credential Manager and stop here, or
  remove the credential with `cmdkey /delete:RangerMSP-DbQry` and continue.

---

## Step 2: Create the variable

Use **either** option A (mouse) **or** option B (PowerShell). Both set the
same User variable.

### Option A: Windows dialog

1. Press **Win + R**, type the following, then press **Enter**:
   ```
   rundll32 sysdm.cpl,EditEnvironmentVariables
   ```
   (Or: Start → type `environment` → **Edit environment variables for your account**.
   Don't pick "Edit the *system* environment variables".)
2. In the top section, **User variables for *your-name***, click **New…**
3. Fill in:
   - **Variable name:** `RANGERMSP_WEBAPI_PASSWORD`
     (exact spelling, all capitals, underscores)
   - **Variable value:** paste the DbQry password. Check for leading or
     trailing spaces.
4. Click **OK** on the *New User Variable* box, then **OK** on the
   *Environment Variables* window. Clicking OK on the outer window is what
   saves it.

### Option B: PowerShell, without the password appearing on screen

Paste this into PowerShell. It asks for the password with hidden input, so the
password isn't shown on screen or saved in PowerShell's history.

```powershell
$secure = Read-Host "Paste the DbQry password" -AsSecureString
$bstr   = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try {
    $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($bstr)
    [Environment]::SetEnvironmentVariable("RANGERMSP_WEBAPI_PASSWORD", $plain, "User")
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($bstr)
    Remove-Variable plain, secure -ErrorAction SilentlyContinue
}
"Saved."
```

Don't use `setx RANGERMSP_WEBAPI_PASSWORD <password>`. Typing the password on
the command line saves it in PowerShell's history file.

---

## Step 3: Check the variable is set (without printing the password)

Open a **new** PowerShell window (existing windows don't pick up new
variables) and run:

```powershell
$v = [Environment]::GetEnvironmentVariable("RANGERMSP_WEBAPI_PASSWORD", "User")
if ($v) { "Set (length $($v.Length))" } else { "NOT set" }
Remove-Variable v
```

The length should match your password. With the 48-character hex passwords
from the server guide, it should print `Set (length 48)`.

If it says **NOT set**, repeat Step 2 and make sure you clicked **OK** on both
windows, or that option B printed `Saved.`

---

## Step 4: Test the connection without Claude

In the same **new** PowerShell window, run the connection test from the build
output. Use your server's Web API address and a city that exists in your data:

```powershell
cd C:\path\to\claude-commit-connector
.\samples\CommitConnector.ConnectionTest\bin\Release\net48\CommitConnector.ConnectionTest.exe http://YOUR-SERVER:4964/ "Your City"
```

| Result | Meaning / fix |
|---|---|
| `Connected to …` and a list of accounts | Works. Go to Step 5. |
| `No RangerMSP Web API password found…` | The variable isn't visible. Make sure you used a **new** window, spelled the name exactly, and set it under **User** for this account. |
| `RangerMSP API error: …` mentioning password or authentication | The value is wrong, or the server still has the old password. Re-check against your password manager and `CmtWPDbQry.ini`, and confirm the CRM Server service was restarted. |
| Timeout or connection refused | Network, port or URL problem, not the password. Check the URL includes `http://`, the port (default 4964) and the firewall. |

---

## Step 5: Restart Claude so the connector sees the variable

Programs only read environment variables when they start.

1. **Quit Claude Desktop completely.** Right-click its icon in the system tray
   (by the clock) and choose **Quit**. Closing the window isn't enough.
2. Start Claude Desktop again from the Start menu.
3. If it still can't find the password, **sign out of Windows and back in**,
   then start Claude Desktop. That reloads every variable for your session.

Then ask Claude something like: *"Use RangerMSP to find accounts named Contoso."*

---

## Things not to do

- **Don't put the password in `claude_desktop_config.json`** (its `"env"`
  block) or in `appsettings.json`. Those files are plain text and get copied
  and backed up.
- **Don't create it as a System variable.** Every account and service on the
  machine could read it.
- **Don't save it in a `.txt` file or a script** "for later". The password
  manager is the only place it should be stored besides this variable.

---

## Later: changing or removing the password

**When the DbQry password changes:**
1. Update it on the RangerMSP server (`CmtWPDbQry.ini`) and restart the CRM Server service.
2. Repeat **Step 2** with the new value. It overwrites the old one.
3. Repeat **Step 3** and **Step 5**.

**To remove the variable** (for example, when moving to Credential Manager):

```powershell
[Environment]::SetEnvironmentVariable("RANGERMSP_WEBAPI_PASSWORD", $null, "User")
```

Then restart Claude Desktop.

**If the connector later runs as a Windows service or scheduled task** under a
dedicated service account, sign in as that account and set the variable under
that account's **User** variables, or use Credential Manager as that account.
Your own account's variables aren't visible to it.

---

## Checklist

- [ ] Working on the machine that runs Claude Desktop and the connector
- [ ] Signed in as the account that uses Claude
- [ ] No `RangerMSP-DbQry` entry in Credential Manager (or chose to keep using it)
- [ ] `RANGERMSP_WEBAPI_PASSWORD` created under **User** variables, holding the **DbQry** password
- [ ] Step 3 shows `Set (length NN)`
- [ ] ConnectionTest lists accounts
- [ ] Claude Desktop fully quit and restarted
- [ ] Password not stored in any config file, script or text file
