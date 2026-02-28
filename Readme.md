# Pandatech.EFCore.PostgresExtensions

PostgreSQL-specific extensions for Entity Framework Core that fill the gaps left by the official Npgsql provider.

| Feature                     | What it does                                                   |
|-----------------------------|----------------------------------------------------------------|
| **Row-level locking**       | `FOR UPDATE` with `Wait`, `SkipLocked`, and `NoWait` behaviors |
| **Random-increment IDs**    | Non-predictable sequential IDs backed by a PostgreSQL sequence |
| **Natural sort keys**       | Human-friendly ordering for strings containing numbers         |
| **Schema rollback helpers** | Clean `Down()` migration methods for all of the above          |

Targets **net8.0**, **net9.0**, and **net10.0**.

## Installation

```bash
dotnet add package Pandatech.EFCore.PostgresExtensions
```

## Row-level locking

PostgreSQL's `FOR UPDATE` clause lets you lock selected rows for the duration of a transaction. This package exposes it
as a LINQ extension method with three lock behaviors.

### Setup

Register the query interceptor on your `DbContext`:

```csharp
services.AddDbContextPool<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
           .UseQueryLocks());
```

### Usage

The `ForUpdate` method must be called inside a transaction:

```csharp
await using var transaction = await dbContext.Database.BeginTransactionAsync();

var order = await dbContext.Orders
    .Where(o => o.Id == orderId)
    .ForUpdate(LockBehavior.SkipLocked)
    .FirstOrDefaultAsync();

// Modify the locked row
order.Status = OrderStatus.Processing;
await dbContext.SaveChangesAsync();
await transaction.CommitAsync();
```

### Lock behaviors

| Behavior         | SQL generated            | When to use                                                     |
|------------------|--------------------------|-----------------------------------------------------------------|
| `Default` (Wait) | `FOR UPDATE`             | You need the row and can wait for it                            |
| `SkipLocked`     | `FOR UPDATE SKIP LOCKED` | Queue-style processing — skip rows another worker already holds |
| `NoWait`         | `FOR UPDATE NOWAIT`      | Fail immediately if the row is locked                           |

## Random-increment sequence IDs

Generates `bigint` IDs that increment by a random amount within a configurable range. The IDs are unique and always
increasing, but the gaps between them are unpredictable — preventing enumeration attacks while keeping an index-friendly
insert order.

### 1. Configure the entity

```csharp
public class Animal
{
    public long Id { get; set; }
    public string Name { get; set; }
}

public class AnimalConfiguration : IEntityTypeConfiguration<Animal>
{
    public void Configure(EntityTypeBuilder<Animal> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
               .HasRandomIdSequence();
    }
}
```

### 2. Create the function in a migration

The function must be created **before** the table that references it. Add it manually at the top of your `Up()` method:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Create the sequence + function first
    migrationBuilder.CreateRandomIdSequence(
        tableName: "animal",
        pkName: "id",
        startValue: 5,
        minRandIncrementValue: 5,
        maxRandIncrementValue: 10);

    // Then create the table (the default value references the function)
    migrationBuilder.CreateTable(...);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropRandomIdSequence("animal", "id");
    migrationBuilder.DropTable("animal");
}
```

Parameters: `startValue` is the first ID returned, `minRandIncrementValue` and `maxRandIncrementValue` define the random
gap range between consecutive IDs.

## Natural sort keys

Sorts strings that contain numbers the way a human would expect: `"Item 2"` before `"Item 10"`, not after it. The
package creates a PostgreSQL function that zero-pads numeric substrings, producing a sortable text key.

### 1. Create the function (once per database)

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateNaturalSortKeyFunction();
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropNaturalSortKeyFunction();
}
```

### 2. Add a computed column

```csharp
public class Building
{
    public long Id { get; set; }
    public string Address { get; set; }
    public string AddressNaturalSortKey { get; set; }
}

public class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.Property(x => x.AddressNaturalSortKey)
               .HasNaturalSortKey("address");
    }
}
```

Then order by the computed column:

```csharp
var sorted = await dbContext.Buildings
    .OrderBy(b => b.AddressNaturalSortKey)
    .ToListAsync();
```

## Encrypted-column unique indexes

For columns that store encrypted data where only the first 64 characters are deterministic (e.g., hash prefix), you can
create a unique index on that prefix:

```csharp
// In migration Up()
migrationBuilder.CreateUniqueIndexOnEncryptedColumn("users", "email_encrypted");

// With an optional WHERE condition
migrationBuilder.CreateUniqueIndexOnEncryptedColumn("users", "email_encrypted", "is_active = true");

// In migration Down()
migrationBuilder.DropUniqueIndexOnEncryptedColumn("users", "email_encrypted");
```

## API reference

### Extension methods

| Method                                    | Target                    | Description                                                                         |
|-------------------------------------------|---------------------------|-------------------------------------------------------------------------------------|
| `UseQueryLocks()`                         | `DbContextOptionsBuilder` | Registers the interceptor that rewrites tagged queries into `FOR UPDATE` SQL        |
| `ForUpdate()`                             | `IQueryable<T>`           | Tags a query for row-level locking (must be inside a transaction)                   |
| `HasRandomIdSequence()`                   | `PropertyBuilder`         | Configures the property to use a random-increment sequence as its default value     |
| `HasNaturalSortKey(column)`               | `PropertyBuilder`         | Configures the property as a stored computed column using the natural sort function |
| `CreateRandomIdSequence(...)`             | `MigrationBuilder`        | Creates the PostgreSQL sequence and generator function                              |
| `DropRandomIdSequence(...)`               | `MigrationBuilder`        | Drops the sequence and generator function                                           |
| `CreateNaturalSortKeyFunction()`          | `MigrationBuilder`        | Creates the natural sort key function                                               |
| `DropNaturalSortKeyFunction()`            | `MigrationBuilder`        | Drops the natural sort key function                                                 |
| `CreateUniqueIndexOnEncryptedColumn(...)` | `MigrationBuilder`        | Creates a unique index on the first 64 chars of a column                            |
| `DropUniqueIndexOnEncryptedColumn(...)`   | `MigrationBuilder`        | Drops the encrypted-column unique index                                             |

### Enums

`LockBehavior`: `Default` (wait), `SkipLocked`, `NoWait`.

## License

MIT