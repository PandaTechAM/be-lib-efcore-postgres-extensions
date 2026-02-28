using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace EFCore.PostgresExtensions.Extensions;

internal static class DbSetExtensions
{
   public static DbContext GetDbContext<T>(this DbSet<T> dbSet) where T : class
   {
      IInfrastructure<IServiceProvider> infrastructure = dbSet;
      var serviceProvider = infrastructure.Instance;
      var currentDbContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
      return currentDbContext!.Context;
   }
}