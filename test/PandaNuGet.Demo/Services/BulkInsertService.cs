using System.Diagnostics;
using System.Text;
using Dapper;
using EFCore.PostgresExtensions.Extensions.BulkInsertExtension;
using Microsoft.EntityFrameworkCore;
using PandaNuGet.Demo.Context;
using PandaNuGet.Demo.Dtos;
using PandaNuGet.Demo.Entities;

namespace PandaNuGet.Demo.Services;

public class BulkInsertService(PostgresContext dbContext)
{
    private const int BatchSize = 1500;

    public async Task<BulkBenchmarkResponse> BulkInsertEfCoreAsync(int rowsCount, bool ignoreReset = false)
    {
        await ResetDbAsync(ignoreReset);
        List<UserEntity> users = new();

        for (int i = 0; i < rowsCount; i++)
        {
            users.Add(new UserEntity());
        }

        var stopwatch = Stopwatch.StartNew();
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

        for (int i = 0; i < users.Count; i += BatchSize)
        {
            var batch = users.Skip(i).Take(BatchSize).ToList();
            await dbContext.Users.AddRangeAsync(batch);
            await dbContext.SaveChangesAsync();
        }

        dbContext.ChangeTracker.Clear();
        stopwatch.Stop();

        return new BulkBenchmarkResponse(BenchmarkMethod.EFCore, rowsCount,
            stopwatch.ElapsedMilliseconds.ToString());
    }

    public BulkBenchmarkResponse BulkInsertEfCore(int rowsCount, bool ignoreReset = false)
    {
        ResetDb(ignoreReset);

        List<UserEntity> users = new();

        for (int i = 0; i < rowsCount; i++)
        {
            users.Add(new UserEntity());
        }

        var stopwatch = Stopwatch.StartNew();
        dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

        for (int i = 0; i < users.Count; i += BatchSize)
        {
            var batch = users.Skip(i).Take(BatchSize).ToList();
            dbContext.Users.AddRange(batch);
            dbContext.SaveChanges();
        }

        dbContext.ChangeTracker.Clear();
        stopwatch.Stop();

        return new BulkBenchmarkResponse(BenchmarkMethod.EFCore, rowsCount,
            stopwatch.ElapsedMilliseconds.ToString());
    }

    public async Task<BulkBenchmarkResponse> BulkInsertNpgsqlCopyAsync(int rowsCount, bool ignoreReset = false)
    {
        await ResetDbAsync(ignoreReset);
        var users = new List<UserEntity>();

        for (int i = 0; i < rowsCount; i++)
        {
            users.Add(new UserEntity());
        }

        var stopwatch = Stopwatch.StartNew();
        await dbContext.Users.BulkInsertAsync(users);
        stopwatch.Stop();

        return new BulkBenchmarkResponse(BenchmarkMethod.NpgsqlCopy, rowsCount,
            stopwatch.ElapsedMilliseconds.ToString());
    }

    public BulkBenchmarkResponse BulkInsertNpgsqlCopy(int rowsCount, bool ignoreReset = false)
    {
        ResetDb(ignoreReset);
        var users = new List<UserEntity>();

        for (int i = 0; i < rowsCount; i++)
        {
            users.Add(new UserEntity());
        }

        var stopwatch = Stopwatch.StartNew();
        dbContext.Users.BulkInsert(users);
        stopwatch.Stop();

        return new BulkBenchmarkResponse(BenchmarkMethod.NpgsqlCopy, rowsCount,
            stopwatch.ElapsedMilliseconds.ToString());
    }

    public BulkBenchmarkResponse BulkInsertDapper(int rowsCount, bool ignoreReset = false)
    {
        ResetDb(ignoreReset);

        var users = new List<UserEntity>();
        for (int i = 0; i < rowsCount; i++)
        {
            users.Add(new UserEntity());
        }

        var stopwatch = Stopwatch.StartNew();

        for (int batchStart = 0; batchStart < users.Count; batchStart += BatchSize)
        {
            var batchUsers = users.Skip(batchStart).Take(BatchSize);
            var queryBuilder = new StringBuilder(
                "INSERT INTO \"Users\" (\"AlternateId\", \"Name\", \"Address\", \"Height\", \"Weight\", \"BirthDate\", \"DeathDate\", \"Status\", \"IsMarried\", \"IsHappy\", \"Description\", \"Image\", \"Document\") VALUES ");
            var parameters = new DynamicParameters();

            int index = 0;
            foreach (var user in batchUsers)
            {
                queryBuilder.Append(
                    $"(@AlternateId{index}, @Name{index}, @Address{index}, @Height{index}, @Weight{index}, @BirthDate{index}, @DeathDate{index}, @Status{index}, @IsMarried{index}, @IsHappy{index}, @Description{index}, @Image{index}, @Document{index}),");

                parameters.Add($"@AlternateId{index}", user.AlternateId);
                parameters.Add($"@Name{index}", user.Name);
                parameters.Add($"@Address{index}", user.Address);
                parameters.Add($"@Height{index}", user.Height);
                parameters.Add($"@Weight{index}", user.Weight);
                parameters.Add($"@BirthDate{index}", user.BirthDate);
                parameters.Add($"@DeathDate{index}", user.DeathDate);
                parameters.Add($"@Status{index}", user.Status);
                parameters.Add($"@IsMarried{index}", user.IsMarried);
                parameters.Add($"@IsHappy{index}", user.IsHappy);
                parameters.Add($"@Description{index}", user.Description);
                parameters.Add($"@Image{index}", user.Image);
                parameters.Add($"@Document{index}", user.Document);
                index++;
            }

            queryBuilder.Length--; // Remove the last comma

            using var transaction = dbContext.Database.BeginTransaction();
            dbContext.Database.GetDbConnection().Execute(queryBuilder.ToString(), parameters);
            transaction.Commit();
        }

        stopwatch.Stop();
        return new BulkBenchmarkResponse(BenchmarkMethod.Dapper, rowsCount, stopwatch.ElapsedMilliseconds.ToString());
    }

    public async Task<BulkBenchmarkResponse> BulkInsertDapperAsync(int rowsCount, bool ignoreReset = false)
    {
        await ResetDbAsync(ignoreReset);

        var users = new List<UserEntity>();
        for (int i = 0; i < rowsCount; i++)
        {
            users.Add(new UserEntity());
        }

        var stopwatch = Stopwatch.StartNew();

        for (int batchStart = 0; batchStart < users.Count; batchStart += BatchSize)
        {
            var batchUsers = users.Skip(batchStart).Take(BatchSize);
            var queryBuilder = new StringBuilder(
                "INSERT INTO \"Users\" (\"AlternateId\", \"Name\", \"Address\", \"Height\", \"Weight\", \"BirthDate\", \"DeathDate\", \"Status\", \"IsMarried\", \"IsHappy\", \"Description\", \"Image\", \"Document\") VALUES ");
            var parameters = new DynamicParameters();

            int index = 0;
            foreach (var user in batchUsers)
            {
                queryBuilder.Append(
                    $"(@AlternateId{index}, @Name{index}, @Address{index}, @Height{index}, @Weight{index}, @BirthDate{index}, @DeathDate{index}, @Status{index}, @IsMarried{index}, @IsHappy{index}, @Description{index}, @Image{index}, @Document{index}),");

                parameters.Add($"@AlternateId{index}", user.AlternateId);
                parameters.Add($"@Name{index}", user.Name);
                parameters.Add($"@Address{index}", user.Address);
                parameters.Add($"@Height{index}", user.Height);
                parameters.Add($"@Weight{index}", user.Weight);
                parameters.Add($"@BirthDate{index}", user.BirthDate);
                parameters.Add($"@DeathDate{index}", user.DeathDate);
                parameters.Add($"@Status{index}", user.Status);
                parameters.Add($"@IsMarried{index}", user.IsMarried);
                parameters.Add($"@IsHappy{index}", user.IsHappy);
                parameters.Add($"@Description{index}", user.Description);
                parameters.Add($"@Image{index}", user.Image);
                parameters.Add($"@Document{index}", user.Document);
                index++;
            }

            queryBuilder.Length--; // Remove the last comma

            await using var transaction = await dbContext.Database.BeginTransactionAsync();
            await dbContext.Database.GetDbConnection().ExecuteAsync(queryBuilder.ToString(), parameters);
            await transaction.CommitAsync();
        }

        stopwatch.Stop();
        return new BulkBenchmarkResponse(BenchmarkMethod.Dapper, rowsCount, stopwatch.ElapsedMilliseconds.ToString());
    }

    private async Task ResetDbAsync(bool ignore)
    {
        if (ignore)
        {
            return;
        }

        await dbContext.Database.EnsureDeletedAsync();
        await dbContext.Database.EnsureCreatedAsync();
    }

    private void ResetDb(bool ignore)
    {
        if (ignore)
        {
            return;
        }

        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
}