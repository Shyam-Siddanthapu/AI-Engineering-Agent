using AiAgent.Core.Abstractions;
using AiAgent.Core.DependencyInjection;
using AiAgent.Core.Models;
using AiAgent.Infrastructure.DependencyInjection;
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
