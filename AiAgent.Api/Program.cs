using AiAgent.Core.Abstractions;
using AiAgent.Core.DependencyInjection;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, loggerConfiguration) =>
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

builder.Services.AddOpenApi();
builder.Services.AddAiAgentCore();
builder.Services.AddAiAgentInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddRazorPages();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AiAgent.Infrastructure.Persistence.AgentExecutionDbContext>();
    dbContext.Database.EnsureCreated();
    try
    {
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE AgentExecutions ADD COLUMN ConversationId TEXT");
    }
    catch
    {
    }
    dbContext.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS Conversations (
            Id TEXT NOT NULL PRIMARY KEY,
            Title TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );
    """);
    try
    {
        dbContext.Database.ExecuteSqlRaw("ALTER TABLE Conversations ADD COLUMN RepoUrl TEXT NOT NULL DEFAULT ''");
    }
    catch
    {
    }
    dbContext.Database.ExecuteSqlRaw("""
        CREATE TABLE IF NOT EXISTS Messages (
            Id TEXT NOT NULL PRIMARY KEY,
            ConversationId TEXT NOT NULL,
            Role TEXT NOT NULL,
            Content TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );
    """);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.UseStaticFiles();
app.MapRazorPages();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();
