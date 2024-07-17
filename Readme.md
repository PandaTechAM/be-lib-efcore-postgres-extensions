# Pandatech.EFCore.PostgresExtensions

Pandatech.EFCore.PostgresExtensions is an advanced NuGet package designed to enhance PostgreSQL functionalities within
Entity Framework Core, leveraging specific features not covered by the official Npgsql.EntityFrameworkCore.PostgreSQL
package. This package introduces optimized row-level locking mechanisms and PostgreSQL sequence random incrementing
features.

## Features

1. **Row-Level Locking**: Implements the PostgreSQL `FOR UPDATE` feature, providing three lock
   behaviors - `Wait`, `Skip`, and
   `NoWait`, to facilitate advanced transaction control and concurrency management.
2. **Npgsql COPY Integration (Obsolete)**: Offers a high-performance, typed interface for the PostgreSQL COPY command,
   allowing for
   bulk data operations within the EF Core framework. This feature significantly enhances data insertion speeds and
   efficiency.
3. **Random Incrementing Sequence Generation:** Provides a secure way to generate sequential IDs with random increments
   to prevent predictability and potential data exposure. This ensures IDs are non-sequential and non-predictable,
   enhancing security and balancing database load.

## Installation

To install Pandatech.EFCore.PostgresExtensions, use the following NuGet command:

```bash
Install-Package Pandatech.EFCore.PostgresExtensions
```

## Usage

### Row-Level Locking

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

### Random Incrementing Sequence Generation

To configure a model to use the random ID sequence, use the `HasRandomIdSequence` extension method in your entity
configuration:

```csharp
public class Animal
{
    public long Id { get; set; }
    public string Name { get; set; }
}

public class AnimalEntityConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasRandomIdSequence();
    }
}
```

After creating a migration, add the custom function **above create table** script in your migration class:

```csharp
public partial class PgFunction : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateRandomIdSequence("animal", "id", 5, 5, 10); //Add this line manually
        
        migrationBuilder.CreateTable(
            name: "animal",
            columns: table => new
            {
                id = table.Column<long>(type: "bigint", nullable: false, defaultValueSql: "animal_random_id_generator()"),
                name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_animal", x => x.id);
            });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "animal");
    }
}
```
#### Additional notes
- The random incrementing sequence feature ensures the generated IDs are unique, non-sequential, and non-predictable, enhancing security.
- The feature supports only `long` data type (`bigint` in PostgreSQL).


### Npgsql COPY Integration (Obsolete: Use EFCore.BulkExtensions.PostgreSql instead)

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

#### Benchmarks

The integration of the Npgsql COPY command showcases significant performance improvements compared to traditional EF
Core and Dapper methods:

##### General Benchmark Results

| Caption    | Big O Notation | 1M Rows     | Batch Size |
|------------|----------------|-------------|------------|
| BulkInsert | O(log n)       | 350.000 r/s | No batch   |
| Dapper     | O(n)           | 20.000 r/s  | 1500       |
| EFCore     | O(n)           | 10.600 r/s  | 1500       |

##### Detailed Benchmark Results

| Operation   | BulkInsert | Dapper | EF Core |
|-------------|------------|--------|---------|
| Insert 10K  | 76ms       | 535ms  | 884ms   |
| Insert 100K | 405ms      | 5.47s  | 8.58s   |
| Insert 1M   | 2.87s      | 55.85s | 94.57s  |

##### Efficiency Comparison

| RowsCount | BulkInsert Efficiency      | Dapper Efficiency         |
|-----------|----------------------------|---------------------------|
| 10K       | 11.63x faster than EF Core | 1.65x faster than EF Core |
| 100K      | 21.17x faster than EF Core | 1.57x faster than EF Core |
| 1M        | 32.95x faster than EF Core | 1.69x faster than EF Core |

##### Additional Notes

- The `BulkInsert` feature currently does not support entity properties intended for `JSON` storage.

- The performance metrics provided above are based on benchmarks conducted under controlled conditions. Real-world
  performance may vary based on specific use cases and configurations.

## License

Pandatech.EFCore.PostgresExtensions is licensed under the MIT License.
