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

// Keep Swagger available in production so the deployed API can be tested easily.
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.MapControllers();

app.Run();
