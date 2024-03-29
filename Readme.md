# Pandatech.EFCore.PostgresExtensions
Pandatech.EFCore.PostgresExtensions is a NuGet package that enhances Entity Framework Core with support for PostgreSQL-specific syntax for update operations.

## Introduction
You can install the Pandatech.EFCore.PostgresExtensions NuGet package via the NuGet Package Manager UI or the Package Manager Console using the following command:
Install-Package Pandatech.EFCore.PostgresExtensions

## Features
Adds support for PostgreSQL-specific update syntax.
Simplifies handling of update operations when working with PostgreSQL databases.

## Installation
1. Install Pandatech.EFCore.PostgresExtensions Package
```Install-Package Pandatech.EFCore.PostgresExtensions```
 
2. Enable Query Locks

Inside the AddDbContext or AddDbContextPool method, after calling UseNpgsql(), call the UseQueryLocks() method on the DbContextOptionsBuilder to enable query locks.
```
services.AddDbContext<MyDbContext>(options =>
{
    options.UseNpgsql(Configuration.GetConnectionString("MyDatabaseConnection"))
           .UseQueryLocks();
});
```

## Usage
Use the provided ForUpdate extension method on IQueryable within your application to apply PostgreSQL-specific update syntax.
```
using Pandatech.EFCore.PostgresExtensions;
using Microsoft.EntityFrameworkCore;

// Inside your service or repository method
using (var transaction = _dbContext.Database.BeginTransaction())
{
    try
    {
        // Use the ForUpdate extension method on IQueryable inside the transaction scope
        var entityToUpdate = _dbContext.Entities
            .Where(e => e.Id == id)
            .ForUpdate()
            .FirstOrDefault();

        // Perform updates on entityToUpdate

        await _dbContext.SaveChangesAsync();

        transaction.Commit();
    }
    catch (Exception ex)
    {
        transaction.Rollback();
        // Handle exception
    }
}
```
## License

Pandatech.EFCore.PostgresExtensions is licensed under the MIT License.