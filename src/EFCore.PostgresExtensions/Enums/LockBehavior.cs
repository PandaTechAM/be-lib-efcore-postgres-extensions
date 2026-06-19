namespace EFCore.PostgresExtensions.Enums;

/// <summary>
///    Describes how PostgreSQL should handle locked rows when a query uses FOR UPDATE.
/// </summary>
public enum LockBehavior
{
   /// <summary>
   ///    Using this behavior forces transaction to wait until row is unlocked.
   /// </summary>
   Default = 0,

   /// <summary>
   ///    Using this behavior will skip rows that are locked by another transaction.
   /// </summary>
   SkipLocked = 1,

   /// <summary>
   ///    Using this behavior will throw an exception if requested rows are locked by another transaction.
   /// </summary>
   NoWait = 2
}

internal static class LockBehaviorExtensions
{
   /// <summary>
   ///    Gets the PostgreSQL locking keyword for a lock behavior.
   /// </summary>
   /// <param name="lockBehavior">The lock behavior to convert.</param>
   /// <returns>The PostgreSQL locking keyword, or an empty string for the default behavior.</returns>
   public static string GetSqlKeyword(this LockBehavior lockBehavior)
   {
      return lockBehavior switch
      {
         LockBehavior.Default => string.Empty,
         LockBehavior.SkipLocked => "skip locked",
         LockBehavior.NoWait => "nowait",
         _ => string.Empty
      };
   }
}
