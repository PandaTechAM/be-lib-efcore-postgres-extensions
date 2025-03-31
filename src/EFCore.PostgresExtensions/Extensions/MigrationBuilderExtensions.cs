using EFCore.PostgresExtensions.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.PostgresExtensions.Extensions;

public static class MigrationBuilderExtensions
{
   public static void CreateRandomIdSequence(this MigrationBuilder migrationBuilder,
      string tableName,
      string pkName,
      long startValue,
      int minRandIncrementValue,
      int maxRandIncrementValue)
   {
      migrationBuilder.Sql(PgFunctionHelpers.GetRandomIdFunctionSql(tableName,
         pkName,
         startValue,
         minRandIncrementValue,
         maxRandIncrementValue));
   }

   public static void CreateNaturalSortKeyFunction(this MigrationBuilder migrationBuilder)
   {
      migrationBuilder.Sql(PgFunctionHelpers.GetNaturalSortKeyFunction());
   }

   public static MigrationBuilder CreateUniqueIndexOnEncryptedColumn(this MigrationBuilder migrationBuilder,
      string tableName,
      string columnName,
      string? condition)
   {
      var indexName = $"ix_{tableName}_{columnName}";
      var whereClause = !string.IsNullOrWhiteSpace(condition) ? $" WHERE {condition}" : string.Empty;

      var sql = $@"
            CREATE UNIQUE INDEX {indexName}
            ON {tableName} (substr({columnName}, 1, 64)){whereClause};";

      migrationBuilder.Sql(sql);

      return migrationBuilder;
   }

   public static MigrationBuilder CreateUniqueIndexOnEncryptedColumn(this MigrationBuilder migrationBuilder,
      string tableName,
      string columnName)
   {
      return migrationBuilder.CreateUniqueIndexOnEncryptedColumn(tableName, columnName, null);
   }

   public static MigrationBuilder DropUniqueIndexOnEncryptedColumn(this MigrationBuilder migrationBuilder,
      string tableName,
      string columnName)
   {
      var indexName = $"ix_{tableName}_{columnName}";

      migrationBuilder.Sql($@"
            DROP INDEX {indexName};");

      return migrationBuilder;
   }
}