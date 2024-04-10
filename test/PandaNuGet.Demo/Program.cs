using PandaNuGet.Demo.Context;
using PandaNuGet.Demo.Dtos;
using PandaNuGet.Demo.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddPostgresContext();
builder.Services.AddScoped<BulkInsertService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

app.ResetDatabase();
app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("ping", () => "pong");

app.MapGet("/benchmark-sync/{minimumRows:int}", (BulkInsertService service, int minimumRows) =>
{
    var results = new List<BulkBenchmarkResponse>
    {
        service.BulkInsertEfCore(minimumRows),
        service.BulkInsertNpgsqlCopy(minimumRows),
        service.BulkInsertDapper(minimumRows),
        service.BulkInsertEfCore(minimumRows * 10),
        service.BulkInsertDapper(minimumRows * 10),
        service.BulkInsertNpgsqlCopy(minimumRows * 10),
        service.BulkInsertEfCore(minimumRows * 100),
        service.BulkInsertDapper(minimumRows * 100),
        service.BulkInsertNpgsqlCopy(minimumRows * 100)
    };

    return results;
});

app.MapGet("/benchmark-async/{minimumRows:int}", async (BulkInsertService service, int minimumRows) =>
{
    var results = new List<BulkBenchmarkResponse>
    {
        await service.BulkInsertEfCoreAsync(minimumRows),
        await service.BulkInsertDapperAsync(minimumRows),
        await service.BulkInsertNpgsqlCopyAsync(minimumRows),
        await service.BulkInsertEfCoreAsync(minimumRows * 10),
        await service.BulkInsertDapperAsync(minimumRows * 10),
        await service.BulkInsertNpgsqlCopyAsync(minimumRows * 10),
        await service.BulkInsertEfCoreAsync(minimumRows * 100),
        await service.BulkInsertDapperAsync(minimumRows * 100),
        await service.BulkInsertNpgsqlCopyAsync(minimumRows * 100)
    };

    return results;
});


app.MapGet("/concurrency1", async (BulkInsertService service) =>
{
    await service.BulkInsertEfCoreAsync(100000);
});
app.MapGet("/concurrency4", async (BulkInsertService service) =>
{
    await service.BulkInsertEfCoreAsync(100000, true);
});
app.MapGet("/concurrency2", async (BulkInsertService service) =>
{
    await service.BulkInsertDapperAsync(200000, true);
});
app.MapGet("/concurrency3", async (BulkInsertService service) =>
{
    await service.BulkInsertDapperAsync(200000, true);
});
app.MapGet("/concurrency5", async (BulkInsertService service) =>
{
    await service.BulkInsertNpgsqlCopyAsync(5_000_000, true);
});
app.MapGet("/concurrency6", async (BulkInsertService service) =>
{
    await service.BulkInsertNpgsqlCopyAsync(5_000_000, true);
});


app.Run();