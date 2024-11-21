namespace EFCore.PostgresExtensions.Enums;

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

public static class LockBehaviorExtensions
{
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