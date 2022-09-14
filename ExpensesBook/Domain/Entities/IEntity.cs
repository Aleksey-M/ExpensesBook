using System;

namespace ExpensesBook.Domain.Entities;

internal interface IEntity
{
    public Guid Id { get; set; }
}
