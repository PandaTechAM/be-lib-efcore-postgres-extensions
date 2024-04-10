using Microsoft.EntityFrameworkCore;
using PandaNuGet.Demo.Entities;

namespace PandaNuGet.Demo.Context;

public class PostgresContext(DbContextOptions<PostgresContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users { get; set; } = null!;
}