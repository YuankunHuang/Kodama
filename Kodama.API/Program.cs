using Kodama.Application.Interfaces;
using Kodama.Application.Services;
using Kodama.Infrastructure.Hosting;
using Kodama.Infrastructure.Hubs;
using Kodama.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

// DI
builder.Services.AddHostedService<SimulationHostedServices>();
builder.Services.AddSingleton<ISimulationLoop, SimulationLoop>();
builder.Services.AddSingleton<IGameBroadcaster, SignalRBroadcaster>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapHub<GameHub>("/gamehub");

app.Run();