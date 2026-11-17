using PokeJuegos.Api.Extensions;
using PokeJuegos.Api.Middleware;
using PokeJuegos.Application;
using PokeJuegos.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddJwtBearerAuth(builder.Configuration);
builder.Services.AddGoogleAuth(builder.Configuration);
builder.Services.AddAuthorization();
builder.Services.AddDefaultCors();
builder.Services.AddDefaultRequestLocalization();
builder.Services.AddRateLimiter(options =>
{
    // The default rejection is 503; clients expect 429 to show "too many attempts".
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1)
            }));
    // Every accepted request can send an email.
    options.AddPolicy("recover-password", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(1)
            }));
});
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithAuth();

WebApplication app = builder.Build();

app.UseExceptionHandlingMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseRequestLocalization();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
