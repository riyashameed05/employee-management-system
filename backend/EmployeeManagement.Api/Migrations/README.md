# Database migrations

Create and apply EF Core migrations from this directory's parent project:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

The migration requires a valid decrypted PostgreSQL connection string supplied through the `Database__EncryptedConnectionString` and `Database__EncryptionKey` environment variables.
