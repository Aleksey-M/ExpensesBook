using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities;

internal sealed class Expense : IEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Значение обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно быть больше 200 символов")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Значение обязательно")]
    public DateTimeOffset Date { get; set; }

    [Required(ErrorMessage = "Значение обязательно")]
    [Range(1, 1000000, ErrorMessage = "Значение должно быть больше 0 и меньше 1,000,000.00")]
    public double Amounth { get; set; }

    public Guid? GroupId { get; set; }

    [Required(ErrorMessage = "Значение обязательно")]
    public Guid CategoryId { get; set; }

    private bool SameId(Expense? other) => Id == (other?.Id ?? Guid.Empty);

    public override bool Equals(object? obj) => SameId(obj as Expense);

    public override int GetHashCode() => Id.GetHashCode();
}

internal sealed class ExpenseDto
{
    public Guid Id { get; set; } = Guid.Empty;

    [Required(ErrorMessage = "Значение обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно быть больше 200 символов")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Значение обязательно")]
    public DateTime? Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Значение обязательно")]
    [Range(1, 1000000, ErrorMessage = "Значение должно быть больше 0 и меньше 1,000,000.00")]
    public double Amounth { get; set; }

    public Guid? GroupId { get; set; }

    [Required(ErrorMessage = "Значение обязательно")]
    public Guid? CategoryId { get; set; }
}
