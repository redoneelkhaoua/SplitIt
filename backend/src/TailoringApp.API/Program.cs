using TailoringApp.Application;
using TailoringApp.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TailoringApp.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tailoring API", Version = "v1" });
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

const string CorsPolicy = "CorsPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, p => p
        .WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        // Serve Swagger UI at the app root (/)
        c.RoutePrefix = string.Empty;
    // Explicitly set the Swagger JSON endpoint so it resolves correctly
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tailoring API v1");
    });
}

// Apply migrations at startup (skip in Testing environment)
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TailoringDbContext>();
    db.Database.Migrate();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (feature?.Error is ValidationException vex)
        {
            var problem = new ProblemDetails
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = "One or more validation errors occurred.",
                Type = "https://httpstatuses.com/400"
            };
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            problem.Extensions["errors"] = errors;
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(problem);
            return;
        }
        throw feature?.Error ?? new Exception("Unknown error");
    });
});

app.UseCors(CorsPolicy);
app.MapControllers();

// Basic health endpoint
app.MapGet("/health", async (TailoringDbContext db, CancellationToken ct) =>
{
    // simple DB query to ensure connectivity
    _ = await db.Database.CanConnectAsync(ct);
    return Results.Ok(new { status = "ok", db = "reachable" });
});

app.Run();

// Expose Program for WebApplicationFactory in integration tests
public partial class Program { }
