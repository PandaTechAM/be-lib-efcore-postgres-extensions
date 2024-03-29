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
        public static IQueryable<T> ForUpdate<T>(this IQueryable<T> query, LockBehavior lockBehavior = LockBehavior.Default)
        {
            query = query.TagWith(ForUpdateKey + lockBehavior.GetSqlKeyword());

            return query.AsQueryable();
        }
    }
}
