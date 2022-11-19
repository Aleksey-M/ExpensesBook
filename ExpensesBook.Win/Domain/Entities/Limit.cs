using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities;

internal sealed class Limit : IEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required, StringLength(200)]
    public string Description { get; set; } = "";

    [Required]
    public DateTimeOffset StartDate { get; set; }

    [Required]
    public DateTimeOffset EndDate { get; set; }

    [Required, Range(1, 100000)]
    public double LimitAmounth { get; set; }
}

internal sealed class LimitDto
{
    public Guid? Id { get; set; }

    [Required, StringLength(200)]
    public string? Description { get; set; }

    [Required]
    public DateTime? StartDate { get; set; } = DateTime.Now;

    [Required]
    public DateTime? EndDate { get; set; } = DateTime.Now.AddMonths(1);

    [Required, Range(1, 100000)]
    public double LimitAmounth { get; set; } = 40000.00;
}
