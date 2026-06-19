using EFCore.PostgresExtensions.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace EFCore.PostgresExtensions.Extensions;

/// <summary>
///    Provides extension methods for configuring PostgreSQL-specific EF Core options.
/// </summary>
public static class OptionsBuilderExtensions
{
   /// <summary>
   ///    Registers the query-lock interceptor required by <see cref="QueryableExtensions.ForUpdate{T}"/>.
   /// </summary>
   /// <param name="builder">The EF Core context options builder to configure.</param>
   /// <returns>The same options builder instance for fluent configuration.</returns>
   public static DbContextOptionsBuilder UseQueryLocks(this DbContextOptionsBuilder builder)
   {
      builder.AddInterceptors(new TaggedQueryCommandInterceptor());

      return builder;
   }
}
