- [1. Pandatech.EFCore.PostgresExtensions](#1-pandatechefcorepostgresextensions)
    - [1.1. Features](#11-features)
    - [1.2. Installation](#12-installation)
    - [1.3. Usage](#13-usage)
        - [1.3.1. Row-Level Locking](#131-row-level-locking)
        - [1.3.2. Npgsql COPY Integration](#132-npgsql-copy-integration)
            - [1.3.2.1. Benchmarks](#1321-benchmarks)
                - [1.3.2.1.1. General Benchmark Results](#13211-general-benchmark-results)
                - [1.3.2.1.2. Detailed Benchmark Results](#13212-detailed-benchmark-results)
                - [1.3.2.1.3. Efficiency Comparison](#13213-efficiency-comparison)
                - [1.3.2.1.4. Additional Notes](#13214-additional-notes)
    - [1.4. License](#14-license)

# 1. Pandatech.EFCore.PostgresExtensions

Pandatech.EFCore.PostgresExtensions is an advanced NuGet package designed to enhance PostgreSQL functionalities within
Entity Framework Core, leveraging specific features not covered by the official Npgsql.EntityFrameworkCore.PostgreSQL
package. This package introduces optimized row-level locking mechanisms and an efficient, typed version of the
PostgreSQL COPY operation, adhering to EF Core syntax for seamless integration into your projects.

## 1.1. Features

1. **Row-Level Locking**: Implements the PostgreSQL `FOR UPDATE` feature, providing three lock
   behaviors - `Wait`, `Skip`, and
   `NoWait`, to facilitate advanced transaction control and concurrency management.
2. **Npgsql COPY Integration**: Offers a high-performance, typed interface for the PostgreSQL COPY command, allowing for
   bulk data operations within the EF Core framework. This feature significantly enhances data insertion speeds and
   efficiency.

## 1.2. Installation

To install Pandatech.EFCore.PostgresExtensions, use the following NuGet command:

```bash
Install-Package Pandatech.EFCore.PostgresExtensions
```

## 1.3. Usage

### 1.3.1. Row-Level Locking

Configure your DbContext to use Npgsql and enable query locks:

```csharp
services.AddDbContext<MyDbContext>(options =>
{
    options.UseNpgsql(Configuration.GetConnectionString("MyDatabaseConnection"))
           .UseQueryLocks();
});
```

Within a transaction scope, apply the desired lock behavior using the `ForUpdate` extension method:

```csharp
using var transaction = _dbContext.Database.BeginTransaction();
try
{
    var entityToUpdate = _dbContext.Entities
        .Where(e => e.Id == id)
        .ForUpdate(LockBehavior.NoWait) // Or use LockBehavior.Default (Wait)/ LockBehavior.SkipLocked
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
```

### 1.3.2. Npgsql COPY Integration

For bulk data operations, use the `BulkInsert` or `BulkInsertAsync` extension methods:

```csharp
public async Task BulkInsertExampleAsync()
{
    var users = new List<UserEntity>();
    for (int i = 0; i < 10000; i++)
    {
        users.Add(new UserEntity { /* Initialization */ });
    }

    await dbContext.Users.BulkInsertAsync(users); // Or use BulkInsert for synchronous operation
    // It also saves changes to the database
}
```

#### 1.3.2.1. Benchmarks

The integration of the Npgsql COPY command showcases significant performance improvements compared to traditional EF
Core and Dapper methods:

##### 1.3.2.1.1. General Benchmark Results

| Caption    | Big O Notation | 1M Rows     | Batch Size |
|------------|----------------|-------------|------------|
| BulkInsert | O(log n)       | 350.000 r/s | No batch   |
| Dapper     | O(n)           | 20.000 r/s  | 1500       |
| EFCore     | O(n)           | 10.600 r/s  | 1500       |

##### 1.3.2.1.2. Detailed Benchmark Results

| Operation   | BulkInsert | Dapper | EF Core |
|-------------|------------|--------|---------|
| Insert 10K  | 76ms       | 535ms  | 884ms   |
| Insert 100K | 405ms      | 5.47s  | 8.58s   |
| Insert 1M   | 2.87s      | 55.85s | 94.57s  |

##### 1.3.2.1.3. Efficiency Comparison

| RowsCount | BulkInsert Efficiency      | Dapper Efficiency         |
|-----------|----------------------------|---------------------------|
| 10K       | 11.63x faster than EF Core | 1.65x faster than EF Core |
| 100K      | 21.17x faster than EF Core | 1.57x faster than EF Core |
| 1M        | 32.95x faster than EF Core | 1.69x faster than EF Core |

##### 1.3.2.1.4. Additional Notes

- The `BulkInsert` feature currently does not support entity properties intended for `JSON` storage.

- The performance metrics provided above are based on benchmarks conducted under controlled conditions. Real-world
  performance may vary based on specific use cases and configurations.

## 1.4. License

Pandatech.EFCore.PostgresExtensions is licensed under the MIT License.
