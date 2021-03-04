using System;
using System.ComponentModel.DataAnnotations;

namespace ExpensesBook.Domain.Entities
{
    internal class Group : IEntity
    {
        [Key]
        public Guid Id { get; set; }
        [Required, StringLength(50)]
        public string Name { get; set; } = "";
        public int Sort { get; set; }
    }

    internal class ExpensesGroupDto
    {
        public Guid? Id { get; set; }
        [Required, StringLength(50)]
        public string? Name { get; set; }
        public int? Sort { get; set; }
    }

    internal record GroupDefaultCategory : IEntity
    {
        public Guid Id { get; set; }
        public Guid GroupId { get; set; }
        public Guid CategoryId { get; set; }
    }    
}
