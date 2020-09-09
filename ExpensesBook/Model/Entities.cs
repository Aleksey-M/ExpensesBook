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
        [Required, StringLength(200)]
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
        [Required, StringLength(200)]
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
        [Required, StringLength(200)]
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
        [Required, StringLength(200)]
        public string Description { get; set; }
        [Required]
        public DateTimeOffset StartIncluded { get; set; } = DateTimeOffset.Now;
        [Required]
        public DateTimeOffset EndExcluded { get; set; } = DateTimeOffset.Now.AddMonths(1);
        [Required]
        public double LimitAmounth { get; set; } = 10000.00;
    }

    internal class SavingsItem
    {
        [Key]
        public Guid Id { get; set; }
        [Required, Range(2000, 2100)]
        public int Year { get; set; }
        [Required, Range(1, 12)]
        public int Month { get; set; }
        [Required, StringLength(200)]
        public string  Description { get; set; }
        [Required]
        public double Income { get; set; }

        public string DisplayDate => new DateTimeOffset(Year, Month, 1, 0, 0, 0, TimeSpan.FromSeconds(0)).ToString("MMMM yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"));
    }

    internal class SavingsDto
    {
        public string Id { get; set; }
        [Required, Range(2000, 2100)]
        public int Year { get; set; }
        [Required, Range(1, 12)]
        public int Month { get; set; }
        [Required, StringLength(200)]
        public string Description { get; set; }
        [Required]
        public double Income { get; set; }
    }
}
