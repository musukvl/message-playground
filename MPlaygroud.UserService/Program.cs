using MassTransit;
using MPlaygroud.UserService.Consumers;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var services = builder.Services;

// Add services to the container.

builder.Services.AddControllers();


// mass transit:
services.AddMassTransit(config => {

    config.AddConsumer<UpdateUserConsumer>();

    config.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(configuration.GetValue<string>("EventBusSettings:HostAddress"));

        cfg.ReceiveEndpoint(configuration.GetValue<string>("EventBusSettings:UserServiceQueue"), c =>
        {
            c.ConfigureConsumer<UpdateUserConsumer>(ctx);
        });
    });
});

services.AddMassTransitHostedService();


// app services:
services.AddScoped<UpdateUserConsumer>();



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




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