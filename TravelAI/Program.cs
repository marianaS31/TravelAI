using Microsoft.EntityFrameworkCore;
using TravelAI.Data;
using TravelAI.Interfaces;
using TravelAI.McpIntegration;
using TravelAI.Models;
using TravelAI.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IViagemService, ViagemService>();
//builder.Services.AddScoped<IItinerarioService, ItinerarioService>();
builder.Services.AddScoped<IDiaItinerarioService, DiaItinerarioService>();
builder.Services.AddScoped<IAtividadeService, AtividadeService>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TravelAIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<List<McpServerConfig>>(
    builder.Configuration.GetSection("McpServers"));

builder.Services.AddSingleton<IMcpOrchestrator, McpOrchestrator>();
builder.Services.AddHttpClient<ILlmService, LmStudioLlmService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/api/test-mcp-tools", async (IMcpOrchestrator mcpOrchestrator) =>
{
    var tools = await mcpOrchestrator.DescobrirFerramentasAsync();
    return Results.Ok(tools);
});

app.Run();
