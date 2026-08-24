using System.Text;
using JiraClone.Api.Data;
using JiraClone.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key must be configured.");
if (jwtKey.Length < 32) throw new InvalidOperationException("Jwt:Key must contain at least 32 characters.");

builder.Services.AddDbContext<JiraDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=jira.db"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IssueApplicationService>();
builder.Services.AddScoped<SprintApplicationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    ValidateIssuer = false,
    ValidateAudience = false,
    ValidateLifetime = true,
    ClockSkew = TimeSpan.FromMinutes(1)
});
builder.Services.AddAuthorization(o => o.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddProblemDetails(options => options.CustomizeProblemDetails = context => context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, title) = exception switch
    {
        KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource not found"),
        ArgumentException => (StatusCodes.Status400BadRequest, "Invalid request"),
        InvalidOperationException => (StatusCodes.Status409Conflict, "Operation not allowed"),
        DbUpdateException => (StatusCodes.Status409Conflict, "Database constraint violation"),
        _ => (StatusCodes.Status500InternalServerError, "Unexpected server error")
    };
    await Results.Problem(statusCode: status, title: title, detail: app.Environment.IsDevelopment() ? exception?.Message : null, extensions: new Dictionary<string, object?> { ["traceId"] = context.TraceIdentifier }).ExecuteAsync(context);
}));

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JiraDbContext>();
    await db.Database.MigrateAsync();
    await SeedData.InitializeAsync(db);
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
