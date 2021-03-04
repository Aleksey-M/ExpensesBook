using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities
{
    internal class Limit : IEntity
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

    internal class LimitDto
    {
        public string? Id { get; set; }
        [Required, StringLength(200)]
        public string? Description { get; set; }
        [Required]
        public DateTimeOffset StartDate { get; set; } = DateTimeOffset.Now;
        [Required]
        public DateTimeOffset EndDate { get; set; } = DateTimeOffset.Now.AddMonths(1);
        [Required, Range(1, 100000)]
        public double LimitAmounth { get; set; } = 40000.00;
    }
}
