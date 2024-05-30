using EFCoreQueryMagic.PostgresContext;
using Microsoft.EntityFrameworkCore;
using PandaNuGet.Demo.Entities;

namespace PandaNuGet.Demo.Context;

public class PostgresContext(DbContextOptions<PostgresContext> options) : PostgresDbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;
}

public abstract class PostgresDbContext(DbContextOptions options) : DbContext(options)
{
    [DbFunction("substr", IsBuiltIn = true)]
    public static byte[] substr(byte[] byteArray, int start, int length)
    {
        return byteArray.Skip(start).Take(length).ToArray();
    }
}