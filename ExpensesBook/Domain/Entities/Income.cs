using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities
{
    internal class Income : IEntity
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

    internal class IncomeDto
    {
        public Guid Id { get; set; }
        [Required, StringLength(200)]
        public string Description { get; set; } = "";
        [Required]
        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
        [Required, Range(1, 1000000)]
        public double Amounth { get; set; }
    }
}
