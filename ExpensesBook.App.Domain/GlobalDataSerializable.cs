﻿using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

public sealed class GlobalDataSerializable
{
    public List<Category> Categories { get; set; } = new();

    public List<Group> Groups { get; set; } = new();

    public List<GroupDefaultCategory> GroupsDefaultCategories { get; set; } = new();

    public List<Expense> Expenses { get; set; } = new();

    public List<Limit> Limits { get; set; } = new();

    public List<Income> Incomes { get; set; } = new();
}
