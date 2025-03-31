using EFCore.PostgresExtensions.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.PostgresExtensions.Extensions;

public static class EntityTypeConfigurationExtensions
{
   public static PropertyBuilder<TProperty> HasRandomIdSequence<TProperty>(
      this PropertyBuilder<TProperty> propertyBuilder)
   {
      var tableName = propertyBuilder.Metadata.DeclaringType.GetTableName();
      var pgFunctionName = PgFunctionHelpers.GetRandomIdFunctionName(tableName!);
      propertyBuilder.HasDefaultValueSql(pgFunctionName);


      return propertyBuilder;
   }
   
   public static PropertyBuilder<TProperty> HasNaturalSortKey<TProperty>(this PropertyBuilder<TProperty> propertyBuilder,
      string originalPropName)
   {
      propertyBuilder.HasComputedColumnSql($"get_natural_sort_key({originalPropName})::text", true);


      return propertyBuilder;
   }
}