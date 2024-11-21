using System.Data.Common;
using EFCore.PostgresExtensions.Extensions;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EFCore.PostgresExtensions.Interceptors;

public class TaggedQueryCommandInterceptor : DbCommandInterceptor
{
   public override InterceptionResult<DbDataReader> ReaderExecuting(DbCommand command,
      CommandEventData eventData,
      InterceptionResult<DbDataReader> result)
   {
      ManipulateCommand(command);

      return result;
   }

   public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
      CommandEventData eventData,
      InterceptionResult<DbDataReader> result,
      CancellationToken cancellationToken = default)
   {
      ManipulateCommand(command);

      return new ValueTask<InterceptionResult<DbDataReader>>(result);
   }

   private static void ManipulateCommand(DbCommand command)
   {
      if (command.CommandText.StartsWith($"-- {QueryableExtensions.ForUpdateKey}", StringComparison.Ordinal))
      {
         var tagEndIndex = command.CommandText.IndexOf('\n');

         command.CommandText += command.CommandText[2..tagEndIndex];
      }
   }
}