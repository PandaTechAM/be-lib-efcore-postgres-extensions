using EFCore.PostgresExtensions.Enums;
using Microsoft.EntityFrameworkCore;

namespace EFCore.PostgresExtensions.Extensions
{
    public static class QueryableExtensions
    {
        internal const string ForUpdateKey = "for update ";

        /// <summary>
        /// Use this method for selecting data with locking.
        /// <para>Attention! Be aware that this method works only inside the transaction scope(dbContext.BeginTransaction) and you need to register it in startup.</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">Query to lock.</param>
        /// <param name="lockBehavior">Behavior organizes the way data should be locked, for more information check enum values.</param>
        /// <returns>The same query with locking behavior added.</returns>
        public static IQueryable<T> ForUpdate<T>(this IQueryable<T> query,
            LockBehavior lockBehavior = LockBehavior.Default)
        {
            query = query.TagWith(ForUpdateKey + lockBehavior.GetSqlKeyword());

            return query.AsQueryable();
        }


        //     public static Task<T> FirstOrDefaultByBytesAsync<T>(
        //         this IQueryable<T> query,
        //         Expression<Func<T, byte[]>> byteArrayProperty,
        //         byte[] searchBytes,
        //         int numberOfBytes,
        //         CancellationToken cancellationToken = default) where T : class
        //     {
        //         if (searchBytes == null || searchBytes.Length < numberOfBytes)
        //         {
        //             throw new ArgumentException($"Input array must be at least {numberOfBytes} bytes long.");
        //         }
        //
        //         var firstBytes = searchBytes.Take(numberOfBytes).ToArray();
        //
        //         // Retrieve the DbContext from the IQueryable
        //         var context = query.GetDbContext();
        //
        //         // Get the table name from the DbContext model
        //         var entityType = context.Model.FindEntityType(typeof(T));
        //         var tableName = entityType.GetTableName();
        //
        //         // Construct the SQL query
        //         var queryText = $@"
        //         SELECT * FROM ""{tableName}""
        //         WHERE SUBSTRING(""Data"" FROM 1 FOR {numberOfBytes}) = @p0
        //         LIMIT 1";
        //
        //         // Apply the byte array comparison using FromSqlRaw
        //         return  query.FromSql(queryText, firstBytes).FirstOrDefaultAsync(cancellationToken);
        //     }
        //
        //     private static DbContext GetDbContext<T>(this IQueryable<T> query) where T : class
        //     {
        //         var infrastructure = (IInfrastructure<IServiceProvider>)query;
        //         var serviceProvider = infrastructure.Instance;
        //         var currentContext = serviceProvider.GetService(typeof(ICurrentDbContext)) as ICurrentDbContext;
        //         return currentContext!.Context;
        //     }
        // }
    }
}