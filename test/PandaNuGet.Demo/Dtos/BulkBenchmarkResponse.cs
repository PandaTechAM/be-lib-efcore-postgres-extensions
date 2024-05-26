using System.Text.Json.Serialization;

namespace PandaNuGet.Demo.Dtos;

public record BulkBenchmarkResponse(BenchmarkMethod Method, int RowsCount, string ElapsedMs);

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BenchmarkMethod
{
    EFCore,
    Dapper,
    NpgsqlCopy,
    ExternalBulkInsert
}