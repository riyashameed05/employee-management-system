# Database configuration

The application expects these environment variables:

- `Database__EncryptedConnectionString`
- `Database__EncryptionKey`

The connection string is encrypted with AES-256-GCM before it is stored/configured. The encryption key must never be committed to GitHub.

For local development, configure both values through your shell, .NET user secrets, or your IDE environment settings.

Example PowerShell:

```powershell
$env:Database__EncryptedConnectionString="<encrypted-value>"
$env:Database__EncryptionKey="<secret-key>"
```

Example bash:

```bash
export Database__EncryptedConnectionString='<encrypted-value>'
export Database__EncryptionKey='<secret-key>'
```

Do not put a real connection string, encryption key, password, or other credentials in `appsettings.json` or source control.
