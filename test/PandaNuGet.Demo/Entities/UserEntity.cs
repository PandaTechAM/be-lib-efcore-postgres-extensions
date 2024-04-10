using Microsoft.EntityFrameworkCore;

namespace PandaNuGet.Demo.Entities;

[PrimaryKey(nameof(Id))]
public class UserEntity
{
    public int Id { get; set; }
    public Guid AlternateId { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "John Wick";
    public string? Address { get; set; }
    public decimal Height { get; set; } = 1.85m;
    public decimal? Weight { get; set; }
    public DateTime BirthDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeathDate { get; set; }
    public Status Status { get; set; } = Status.Active;
    public bool IsMarried { get; set; } = true;
    public bool? IsHappy { get; set; }
    public string Description { get; set; } = "Some description to load the field with some data.";
    public byte[] Image { get; set; } = [1, 2, 3, 4, 5];
    public byte[]? Document { get; set; }
}

public enum Status
{
    Active,
    Inactive
}