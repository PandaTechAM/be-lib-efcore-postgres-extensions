using Microsoft.EntityFrameworkCore;

namespace PandaNuGet.Demo.Context;

public static class DatabaseExtensions
{
    public static WebApplicationBuilder AddPostgresContext(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        var connectionString = configuration.GetConnectionString("Postgres");
        builder.Services.AddDbContextPool<PostgresContext>(options =>
            options.UseNpgsql(connectionString));
        return builder;
    }
    
    public static WebApplication ResetDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgresContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        return app;
    }

    public static WebApplication MigrateDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PostgresContext>();
        dbContext.Database.Migrate();
        return app;
    }
}