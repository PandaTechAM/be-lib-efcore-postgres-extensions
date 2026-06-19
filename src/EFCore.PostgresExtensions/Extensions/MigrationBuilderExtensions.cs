using EFCore.PostgresExtensions.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

namespace EFCore.PostgresExtensions.Extensions;

/// <summary>
///    Provides migration helpers for PostgreSQL functions, sequences, and indexes used by this package.
/// </summary>
public static class MigrationBuilderExtensions
{
   /// <summary>
   ///    Creates the PostgreSQL sequence and random-ID generator function for a table primary key.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   /// <param name="tableName">The table that owns the generated primary-key values.</param>
   /// <param name="pkName">The primary-key column name used to derive the sequence name.</param>
   /// <param name="startValue">The initial sequence value.</param>
   /// <param name="minRandIncrementValue">The minimum random increment applied after each generated value.</param>
   /// <param name="maxRandIncrementValue">The maximum random increment applied after each generated value.</param>
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

   /// <summary>
   ///    Creates or replaces the PostgreSQL function used to compute natural sort keys.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   public static void CreateNaturalSortKeyFunction(this MigrationBuilder migrationBuilder)
   {
      migrationBuilder.Sql(PgFunctionHelpers.GetNaturalSortKeyFunction());
   }

   /// <summary>
   ///    Creates a unique index on the deterministic prefix of an encrypted column.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   /// <param name="tableName">The table that contains the encrypted column.</param>
   /// <param name="columnName">The encrypted column to index.</param>
   /// <param name="condition">An optional SQL WHERE clause condition for a filtered index.</param>
   /// <returns>The same migration builder instance for fluent configuration.</returns>
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
   ///    Removes the random‑ID generator function *and* its backing sequence.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   /// <param name="tableName">The table that owns the generated primary-key values.</param>
   /// <param name="pkName">The primary-key column name used to derive the sequence name.</param>
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
   ///    Removes the natural‑sort‑key function.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   public static void DropNaturalSortKeyFunction(this MigrationBuilder migrationBuilder)
   {
      migrationBuilder.Sql("DROP FUNCTION IF EXISTS get_natural_sort_key(TEXT);");
   }

   /// <summary>
   ///    Creates a unique index on the deterministic prefix of an encrypted column.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   /// <param name="tableName">The table that contains the encrypted column.</param>
   /// <param name="columnName">The encrypted column to index.</param>
   /// <returns>The same migration builder instance for fluent configuration.</returns>
   public static MigrationBuilder CreateUniqueIndexOnEncryptedColumn(this MigrationBuilder migrationBuilder,
      string tableName,
      string columnName)
   {
      return migrationBuilder.CreateUniqueIndexOnEncryptedColumn(tableName, columnName, null);
   }

   /// <summary>
   ///    Drops the unique index created for the deterministic prefix of an encrypted column.
   /// </summary>
   /// <param name="migrationBuilder">The migration builder used to append SQL operations.</param>
   /// <param name="tableName">The table that contains the encrypted column.</param>
   /// <param name="columnName">The encrypted column whose index should be dropped.</param>
   /// <returns>The same migration builder instance for fluent configuration.</returns>
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
