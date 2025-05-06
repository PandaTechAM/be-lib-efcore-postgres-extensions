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
   
   /// <summary>
   /// Removes the random‑ID generator function *and* its backing sequence.
   /// </summary>
   public static void DropRandomIdSequence(this MigrationBuilder migrationBuilder,
      string tableName,
      string pkName)
   {
      var sequenceName = $"{tableName}_{pkName}_seq";
      var functionName = $"{tableName}_random_id_generator";

      var sql = $"""
                 DO $$
                 BEGIN
                     -- drop function if it exists
                     IF EXISTS (
                         SELECT 1
                         FROM pg_proc
                         WHERE proname = '{functionName}'
                     ) THEN
                         DROP FUNCTION IF EXISTS {functionName}();
                     END IF;

                     -- drop sequence if it exists
                     IF EXISTS (
                         SELECT 1
                         FROM pg_class
                         WHERE relkind = 'S'
                           AND relname = '{sequenceName}'
                     ) THEN
                         DROP SEQUENCE IF EXISTS {sequenceName};
                     END IF;
                 END
                 $$;
                 """;

      migrationBuilder.Sql(sql);
   }

   /// <summary>
   /// Removes the natural‑sort‑key function.
   /// </summary>
   public static void DropNaturalSortKeyFunction(this MigrationBuilder migrationBuilder)
   {
      migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_natural_sort_key(TEXT);");
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