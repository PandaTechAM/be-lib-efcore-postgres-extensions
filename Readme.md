# Pandatech.EFCore.PostgresExtensions

Pandatech.EFCore.PostgresExtensions is an advanced NuGet package designed to enhance PostgreSQL functionalities within
Entity Framework Core, leveraging specific features not covered by the official Npgsql.EntityFrameworkCore.PostgreSQL
package. This package introduces optimized row-level locking mechanisms and PostgreSQL sequence random incrementing
features.

## Features

1. **Row-Level Locking**: Implements the PostgreSQL `FOR UPDATE` feature, providing three lock
   behaviors - `Wait`, `Skip`, and
   `NoWait`, to facilitate advanced transaction control and concurrency management.
2. **Random Incrementing Sequence Generation:** Provides a secure way to generate sequential IDs with random increments
   to prevent predictability and potential data exposure. This ensures IDs are non-sequential and non-predictable,
   enhancing security and balancing database load.
3. **Natural Sorting**: Provides way to calculate natural sort compliant order for string, which can be used
   in `ORDER BY` clause. This is useful for sorting strings that contain numbers in a human-friendly way.
4. **Schema Rollback Helpers**: Extension methods `DropRandomIdSequence` and `DropNaturalSortKeyFunction` simplify
   cleanup in `Down` migrations.

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
        migrationBuilder.DropRandomIdSequence("animal", "id");
         
        migrationBuilder.DropTable(
            name: "animal");
    }
}
```

#### Additional notes

- The random incrementing sequence feature ensures the generated IDs are unique, non-sequential, and non-predictable,
  enhancing security.
- The feature supports only `long` data type (`bigint` in PostgreSQL).

### Natural Sort Key

This package can generate a natural sort key for your text columns—especially useful when sorting addresses or other
fields that contain embedded numbers. It avoids plain lexicographic ordering (e.g. `"10"` < `"2"`) by treating numeric
substrings numerically.

#### How to Use

1. Create the function in your migration (once per database). Call the helper method in `Up()`:
    ```csharp
       public partial class AddNaturalSortKeyToBuildings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        // Create the natural sort key function in PostgreSQL
        migrationBuilder.CreateNaturalSortKeyFunction();   
           
        protected override void Down(MigrationBuilder migrationBuilder)
           {
               migrationBuilder.DropNaturalSortKeyFunction();        
           }
        }
    }
    ```
2. Configure your entity to use the natural sort key. In your `IEntityTypeConfiguration` for the table:
    ```csharp
    public class BuildingConfiguration : IEntityTypeConfiguration<Building>
    {
        public void Configure(EntityTypeBuilder<Building> builder)
        {
            // Create a computed column in EF (like "address_natural_sort_key")
            builder
                .Property(x => x.AddressNaturalSortKey)
                .HasNaturalSortKey("address"); // Points to the column storing your original address
        }
    }    
    ```

When you query the entity, simply `ORDER BY AddressNaturalSortKey` to get true “natural” ordering in PostgreSQL.

## License

Pandatech.EFCore.PostgresExtensions is licensed under the MIT License.
