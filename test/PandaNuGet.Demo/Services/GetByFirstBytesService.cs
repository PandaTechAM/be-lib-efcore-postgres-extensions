using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PandaNuGet.Demo.Context;
using PandaNuGet.Demo.Entities;
using Pandatech.Crypto;
using PostgresDbContext = PandaNuGet.Demo.Context.PostgresDbContext;

namespace PandaNuGet.Demo.Services;

public class GetByFirstBytesService(PostgresContext context)
{
   private static byte[] DocumentNumber => Sha3.Hash("1234567890");

   public async Task<int> SeedUser()
   {
      var user = new UserEntity();
      var documentNumber = DocumentNumber;
      var randomBytes = new byte[10];
      documentNumber = documentNumber.Concat(randomBytes)
                                     .ToArray();
      user.Document = documentNumber;
      context.Users.Add(user);
      await context.SaveChangesAsync();
      return user.Id;
   }

   public async Task GetByFirstBytes()
   {
      var userId = await SeedUser();

      var userByDocument = await context
                                 .Users
                                 .WhereStartWithBytes(x => x.Document!, 1, 64, DocumentNumber)
                                 .FirstOrDefaultAsync();

      Console.WriteLine($"AAAAAA {userByDocument!.Id}");

      var user = await context.Users.FindAsync(userId);
      if (user != null)
      {
         context.Users.Remove(user);
         await context.SaveChangesAsync();
      }
   }

   public async Task GetByFirstBytesDavit()
   {
      var userId = await SeedUser();

      var userByDocument = await context
                                 .Users
                                 .Where(u => PostgresDbContext.substr(u.Document!, 1, 64)
                                                              .SequenceEqual(DocumentNumber))
                                 .FirstOrDefaultAsync();

      Console.WriteLine($"AAAAAA {userByDocument!.Id}");

      var user = await context.Users.FindAsync(userId);
      if (user != null)
      {
         context.Users.Remove(user);
         await context.SaveChangesAsync();
      }
   }
}

public static class QueryableExtensions
{
   public static IQueryable<T> WhereStartWithBytes<T>(this IQueryable<T> source,
      Expression<Func<T, byte[]>> byteArraySelector,
      int start,
      int length,
      byte[] prefix) where T : class
   {
      // Parameter expression for the source element
      var parameter = Expression.Parameter(typeof(T), "x");

      // Expression to get the byte array from the element
      var member = Expression.Invoke(byteArraySelector, parameter);

      // MethodInfo for the substr method
      var methodInfo = typeof(PostgresDbContext).GetMethod(nameof(PostgresDbContext.substr),
      [
         typeof(byte[]),
            typeof(int),
            typeof(int)
      ]);

      // Call to substr method
      var call = Expression.Call(
         methodInfo!,
         member,
         Expression.Constant(start),
         Expression.Constant(length));

      // MethodInfo for the SequenceEqual method
      var sequenceEqualMethod = typeof(Enumerable).GetMethods()
                                                  .First(m => m.Name == "SequenceEqual" && m.GetParameters()
                                                     .Length == 2)
                                                  .MakeGenericMethod(typeof(byte));

      // Call to SequenceEqual method
      var sequenceEqualCall = Expression.Call(
         sequenceEqualMethod,
         call,
         Expression.Constant(prefix));

      // Lambda expression for the final predicate
      var lambda = Expression.Lambda<Func<T, bool>>(sequenceEqualCall, parameter);

      // Apply the predicate to the source IQueryable with client-side evaluation
      return source.AsEnumerable()
                   .AsQueryable()
                   .Where(lambda);
   }
}