using PandaNuGet.Demo.Context;
using PandaNuGet.Demo.Dtos;
using PandaNuGet.Demo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddPostgresContext();

builder.Services.AddScoped<GetByFirstBytesService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.ResetDatabase();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("ping", () => "pong");

app.MapGet("/get-user-new", async (GetByFirstBytesService service) =>
{
    await service.GetByFirstBytes();
    return "OK";
});

app.MapGet("/get-user-old", async (GetByFirstBytesService service) =>
{
    await service.GetByFirstBytesDavit();
    return "OK";
});



app.Run();