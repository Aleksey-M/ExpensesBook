using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities
{
    internal class Expense : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(200)]
        public string Description { get; set; } = "";
        [Required]
        public DateTimeOffset Date { get; set; }
        [Required, Range(0, 1000000)]
        public double Amounth { get; set; }
        public Guid? GroupId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }

        private bool SameId(Expense? other) => Id == (other?.Id ?? Guid.Empty);

        public override bool Equals(object? obj) => SameId(obj as Expense);

        public override int GetHashCode() => Id.GetHashCode();
    }

    internal class ExpenseDto
    {
        public string? Id { get; set; }
        [Required, StringLength(200)]
        public string? Description { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
        [Required, Range(1, 1000000)]
        public double Amounth { get; set; }
        public string? GroupId { get; set; }
        [Required]
        public string? CategoryId { get; set; }
    }
}
