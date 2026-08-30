using EmployeeManagement.Api.Data;
using EmployeeManagement.Api.Security;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var encryptedConnectionString = builder.Configuration["Database:EncryptedConnectionString"];
var encryptionKey = builder.Configuration["Database:EncryptionKey"];

if (string.IsNullOrWhiteSpace(encryptedConnectionString) || string.IsNullOrWhiteSpace(encryptionKey))
{
    throw new InvalidOperationException(
        "Database:EncryptedConnectionString and Database:EncryptionKey must be configured.");
}

var connectionString = ConnectionStringProtector.Decrypt(encryptedConnectionString, encryptionKey);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "https://employee-management-system-ivory-beta-79.vercel.app",
                "http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Frontend");

app.MapGet("/health", async (ApplicationDbContext db, CancellationToken cancellationToken) =>
{
    var databaseHealthy = await db.Database.CanConnectAsync(cancellationToken);
    return databaseHealthy
        ? Results.Ok(new { status = "healthy", database = "connected" })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.MapControllers();

app.Run();
