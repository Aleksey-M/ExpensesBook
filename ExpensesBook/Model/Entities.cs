using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Model
{
    internal class ExpensesCategory
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public int Sort { get; set; }
    }

    internal class ExpensesCategoryEdit
    {
        public Guid? Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    internal class ExpensesGroup
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
        public int Sort { get; set; }
    }

    internal class ExpensesGroupEdit
    {
        public Guid? Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; }
    }

    internal class ExpenseItem
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; }
        [Required, Range(0, 1000000)]
        public double Amounth { get; set; }
        public Guid? GroupId { get; set; }
        [Required]
        public Guid CategoryId { get; set; }
    }

    internal class ExpenseItemDto
    {
        public string Id { get; set; }
        [Required, StringLength(50)]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; } = DateTimeOffset.Now;
        [Required, Range(0, 1000000)]
        public double Amounth { get; set; }
        public string GroupId { get; set; }
        [Required]
        public string CategoryId { get; set; }
    }

    internal class Limit
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset StartIncluded { get; set; }
        [Required]
        public DateTimeOffset EndExcluded { get; set; }
        [Required, Range(0, 100000)]
        public double LimitAmounth { get; set; }
    }

    internal class LimitDto
    {
        public string Id { get; set; }
        [Required, StringLength(50)]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset StartIncluded { get; set; } = DateTimeOffset.Now;
        [Required]
        public DateTimeOffset EndExcluded { get; set; } = DateTimeOffset.Now.AddMonths(1);
        [Required]
        public double LimitAmounth { get; set; } = 10000.00;
    }
}
