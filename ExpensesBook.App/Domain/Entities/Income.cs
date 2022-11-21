using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities;

public sealed class Income : IEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Description { get; set; } = "";

    [Required]
    public DateTimeOffset Date { get; set; }

    [Required, Range(1, 1000000)]
    public double Amounth { get; set; }
}

public sealed class IncomeDto
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Название обязательно")]
    [StringLength(200, ErrorMessage = "Название не должно быть больше 200 символов")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Значение обязательно")]
    public DateTime? Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Значение обязательно")]
    [Range(1, 1000000, ErrorMessage = "Значение должно быть больше 0 и меньше 1,000,000.00")]
    public double Amounth { get; set; }
}
