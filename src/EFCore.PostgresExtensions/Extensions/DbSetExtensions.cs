using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.PostgresExtensions.Extensions;

/// <summary>
///    Provides internal helpers for retrieving EF Core infrastructure from a DbSet.
/// </summary>
internal static class DbSetExtensions
{
   /// <summary>
   ///    Gets the DbContext instance that owns the specified DbSet.
   /// </summary>
   /// <typeparam name="T">The entity type contained in the DbSet.</typeparam>
   /// <param name="dbSet">The DbSet whose owning DbContext should be returned.</param>
   /// <returns>The DbContext instance associated with the DbSet.</returns>
   public static DbContext GetDbContext<T>(this DbSet<T> dbSet) where T : class
   {
      IInfrastructure<IServiceProvider> infrastructure = dbSet;
      var serviceProvider = infrastructure.Instance;
      var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
      return currentDbContext!.Context;
   }
}
