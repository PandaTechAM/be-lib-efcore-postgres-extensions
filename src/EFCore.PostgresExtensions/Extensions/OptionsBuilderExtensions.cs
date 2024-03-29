using EFCore.PostgresExtensions.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace EFCore.PostgresExtensions.Extensions
{
    public static class OptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseQueryLocks(this DbContextOptionsBuilder builder)
        {
            builder.AddInterceptors(new TaggedQueryCommandInterceptor());

            return builder;
        }
    }
}
