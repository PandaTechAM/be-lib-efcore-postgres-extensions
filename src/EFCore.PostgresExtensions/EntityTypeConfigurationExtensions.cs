using EFCore.PostgresExtensions.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EFCore.PostgresExtensions;

public static class EntityTypeConfigurationExtensions
{
   public static PropertyBuilder<TProperty> HasRandomIdSequence<TProperty>(
      this PropertyBuilder<TProperty> propertyBuilder)
   {
      var tableName = propertyBuilder.Metadata.DeclaringType.GetTableName();
      var pgFunctionName = PgFunctionHelpers.GetPgFunctionName(tableName!);
      propertyBuilder.HasDefaultValueSql(pgFunctionName);


      return propertyBuilder;
   }
}