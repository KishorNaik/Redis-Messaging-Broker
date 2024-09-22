using Message.Request_Response;
using RedisBrokerBus.Extensions;
using ReplyAPI.Reply;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddRedisMessageBroker("localhost:6379", (config) =>
{
    config.AddResponse();

    config.AddReply<DemoRequest, DemoResponse, DemoReplay>("request-channel");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();