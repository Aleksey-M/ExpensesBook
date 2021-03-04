using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities
{
    internal class Category : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = "";
        public int Sort { get; set; }
    }

    internal class CategoryDto
    {
        public Guid? Id { get; set; }
        [Required, StringLength(50)]
        public string? Name { get; set; }
        public int? Sort { get; set; }
    }
}
