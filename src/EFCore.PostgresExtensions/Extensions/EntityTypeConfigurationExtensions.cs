using EFCore.PostgresExtensions.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.PostgresExtensions.Extensions;

/// <summary>
///    Provides property configuration helpers for PostgreSQL-generated values and computed columns.
/// </summary>
public static class EntityTypeConfigurationExtensions
{
   /// <summary>
   ///    Configures a property to use the table-specific random-ID generator function as its default value.
   /// </summary>
   /// <typeparam name="TProperty">The property CLR type.</typeparam>
   /// <param name="propertyBuilder">The property builder to configure.</param>
   /// <returns>The same property builder instance for fluent configuration.</returns>
   public static PropertyBuilder<TProperty> HasRandomIdSequence<TProperty>(
      this PropertyBuilder<TProperty> propertyBuilder)
   {
      var tableName = propertyBuilder.Metadata.DeclaringType.GetTableName();
      var pgFunctionName = PgFunctionHelpers.GetRandomIdFunctionName(tableName!);
      propertyBuilder.HasDefaultValueSql(pgFunctionName);


      return propertyBuilder;
   }

   /// <summary>
   ///    Configures a persisted computed column that stores the natural sort key for another property.
   /// </summary>
   /// <typeparam name="TProperty">The property CLR type.</typeparam>
   /// <param name="propertyBuilder">The property builder to configure.</param>
   /// <param name="originalPropName">The database column name used as input for the natural sort key.</param>
   /// <returns>The same property builder instance for fluent configuration.</returns>
   public static PropertyBuilder<TProperty> HasNaturalSortKey<TProperty>(
      this PropertyBuilder<TProperty> propertyBuilder,
      string originalPropName)
   {
      propertyBuilder.HasComputedColumnSql($"get_natural_sort_key({originalPropName})::text", true);


      return propertyBuilder;
   }
}
