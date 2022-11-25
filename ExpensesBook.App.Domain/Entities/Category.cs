using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities;

public sealed class Category : IEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; } = "";

    public int Sort { get; set; }

    public override string ToString() => Name;

    public override bool Equals(object? o)
    {
        var other = o as Category;
        return other?.Id == Id;
    }
    public override int GetHashCode() => Id.GetHashCode();
}

public sealed class CategoryDto
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(50, ErrorMessage = "Название не должно быть больше 50 символов")]
    public string? Name { get; set; }

    public int? Sort { get; set; }
}

public static class CategoryDtoExtensions
{
    public static string GetName(this CategoryDto? catDto) => catDto?.Name ?? "";

    public static int GetSort(this CategoryDto? catDto) => catDto?.Sort ?? 1;

    public static Guid GetId(this CategoryDto? catDto) => catDto?.Id ?? Guid.Empty;
}
