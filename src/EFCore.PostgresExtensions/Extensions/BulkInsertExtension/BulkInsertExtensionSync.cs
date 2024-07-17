using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;

namespace EFCore.PostgresExtensions.Extensions.BulkInsertExtension;

[Obsolete("Use EfCore.BulkExtensions instead.")]
public static class BulkInsertExtension
{
   public static ILogger? Logger { get; set; }

   [Obsolete("Use EfCore.BulkExtensions instead.")]
   public static async Task BulkInsertAsync<T>(this DbSet<T> dbSet,
      List<T> entities,
      bool pkGeneratedByDb = true) where T : class
   {
      var context = PrepareBulkInsertOperation(dbSet,
         entities,
         pkGeneratedByDb,
         out var sp,
         out var properties,
         out var columnCount,
         out var sql,
         out var propertyInfos,
         out var propertyTypes);

      var connection = new NpgsqlConnection(context.Database.GetConnectionString());
      await connection.OpenAsync();

      await using var writer = await connection.BeginBinaryImportAsync(sql);

      for (var entity = 0; entity < entities.Count; entity++)
      {
         var item = entities[entity];
         var values = propertyInfos.Select(property => property!.GetValue(item))
                                   .ToList();

         ConvertEnumValue<T>(columnCount, propertyTypes, properties, values);

         await writer.StartRowAsync();

         for (var i = 0; i < columnCount; i++)
         {
            await writer.WriteAsync(values[i]);
         }
      }

      await writer.CompleteAsync();
      await connection.CloseAsync();
      sp.Stop();

      Logger?.LogInformation("Binary copy completed successfully. Total time: {Milliseconds} ms",
         sp.ElapsedMilliseconds);
   }

   [Obsolete("Use EfCore.BulkExtensions instead.")]
   public static void BulkInsert<T>(this DbSet<T> dbSet,
      List<T> entities,
      bool pkGeneratedByDb = true) where T : class
   {
      var context = PrepareBulkInsertOperation(dbSet,
         entities,
         pkGeneratedByDb,
         out var sp,
         out var properties,
         out var columnCount,
         out var sql,
         out var propertyInfos,
         out var propertyTypes);

      var connection = new NpgsqlConnection(context.Database.GetConnectionString());
      connection.Open();

      using var writer = connection.BeginBinaryImport(sql);

      for (var entity = 0; entity < entities.Count; entity++)
      {
         var item = entities[entity];
         var values = propertyInfos.Select(property => property!.GetValue(item))
                                   .ToList();

         ConvertEnumValue<T>(columnCount, propertyTypes, properties, values);

         writer.StartRow();

         for (var i = 0; i < columnCount; i++)
         {
            writer.Write(values[i]);
         }
      }

      writer.Complete();
      connection.Close();
      sp.Stop();

      Logger?.LogInformation("Binary copy completed successfully. Total time: {Milliseconds} ms",
         sp.ElapsedMilliseconds);
   }

   private static void ConvertEnumValue<T>(int columnCount,
      IReadOnlyList<Type> propertyTypes,
      IReadOnlyList<IProperty> properties,
      IList<object?> values) where T : class
   {
      for (var i = 0; i < columnCount; i++)
      {
         if (propertyTypes[i].IsEnum)
         {
            values[i] = Convert.ChangeType(values[i], Enum.GetUnderlyingType(propertyTypes[i]));
            continue;
         }

         // Check for generic types, specifically lists, and ensure the generic type is an enum
         if (!propertyTypes[i].IsGenericType || propertyTypes[i]
                .GetGenericTypeDefinition() != typeof(List<>) ||
             !propertyTypes[i]
              .GetGenericArguments()[0].IsEnum) continue;

         var enumMapping = properties[i]
            .FindTypeMapping();

         // Only proceed if the mapping is for an array type, as expected for lists
         if (enumMapping is not NpgsqlArrayTypeMapping) continue;

         var list = (IList)values[i]!;
         var underlyingType = Enum.GetUnderlyingType(propertyTypes[i]
            .GetGenericArguments()[0]);

         var convertedList = (from object item in list
                              select Convert.ChangeType(item, underlyingType)).ToList();
         values[i] = convertedList;
      }
   }


   private static DbContext PrepareBulkInsertOperation<T>(DbSet<T> dbSet,
      List<T> entities,
      bool pkGeneratedByDb,
      out Stopwatch sp,
      out List<IProperty> properties,
      out int columnCount,
      out string sql,
      out List<PropertyInfo?> propertyInfos,
      out List<Type> propertyTypes) where T : class
   {
      sp = Stopwatch.StartNew();
      var context = dbSet.GetDbContext();


      if (entities == null || entities.Count == 0)
         throw new ArgumentException("The model list cannot be null or empty.");

      if (context == null) throw new ArgumentNullException(nameof(context), "The DbContext instance cannot be null.");


      var entityType = context.Model.FindEntityType(typeof(T))! ??
                       throw new InvalidOperationException("Entity type not found.");

      var tableName = entityType.GetTableName() ??
                      throw new InvalidOperationException("Table name is null or empty.");

      properties = entityType.GetProperties()
                             .ToList();

      if (pkGeneratedByDb)
         properties = properties.Where(x => !x.IsKey())
                                .ToList();

      var columnNames = properties.Select(x => $"\"{x.GetColumnName()}\"")
                                  .ToList();

      if (columnNames.Count == 0)
         throw new InvalidOperationException("Column names are null or empty.");


      columnCount = columnNames.Count;
      var rowCount = entities.Count;

      Logger?.LogDebug(
         "Column names found successfully. \n Total column count: {ColumnCount} \n Total row count: {RowCount}",
         columnCount,
         rowCount);

      sql = $"COPY \"{tableName}\" ({string.Join(", ", columnNames)}) FROM STDIN (FORMAT BINARY)";

      Logger?.LogInformation("SQL query created successfully. Sql query: {Sql}", sql);

      propertyInfos = properties.Select(x => x.PropertyInfo)
                                .ToList();
      propertyTypes = propertyInfos.Select(x => x!.PropertyType)
                                   .ToList();
      return context;
   }
}