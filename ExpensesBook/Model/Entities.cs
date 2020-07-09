using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Model
{
    internal class ExpensesCategory
    {
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
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset Date { get; set; }
        [Required, Range(0, 1000000)]
        public double Amounth { get; set; }
        public Guid? GroupId { get; set; }
        public Guid? CategoryId { get; set; }
    }

    internal class Limit
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public DateTimeOffset StartIncluded { get; set; }
        public DateTimeOffset EndExcluded { get; set; }
        public double LimitAmounth { get; set; }
    }
}
