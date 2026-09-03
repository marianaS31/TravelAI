using Microsoft.EntityFrameworkCore;
using TravelAI.Data;
using TravelAI.Interfaces;
using TravelAI.Models;
using TravelAI.Services;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IViagemService, ViagemService>();
//builder.Services.AddScoped<IItinerario, ItinerarioService>();
//builder.Services.AddScoped<IDiaItinerario, DiaItinerarioService>();
//builder.Services.AddScoped<IAtividade, AtividadeService>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<TravelAIContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

app.Run();
