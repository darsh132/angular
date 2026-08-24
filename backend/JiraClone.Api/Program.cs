using JiraClone.Api.Data;
using JiraClone.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<JiraDbContext>(o => o.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=jira.db"));
builder.Services.AddScoped<IssueApplicationService>();
builder.Services.AddCors(o => o.AddPolicy("frontend", p => p.WithOrigins("http://localhost:4200", "https://localhost:4200").AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<JiraDbContext>();
    db.Database.EnsureCreated();
    await SeedData.InitializeAsync(db);
}
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseCors("frontend");
app.MapControllers();
app.Run();
