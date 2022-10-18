using System.Collections.Generic;
using System.Text.Json.Serialization;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

internal sealed class ExpensesDataSerializable
{
    public List<Category> Categories { get; set; } = null!;

    public List<Group> Groups { get; set; } = null!;

    public List<GroupDefaultCategory> GroupsDefaultCategories { get; set; } = null!;

    public List<Expense> Expenses { get; set; } = null!;

    public List<Limit> Limits { get; set; } = null!;

    public List<Income> Incomes { get; set; } = null!;
}

[JsonSerializable(typeof(ExpensesDataSerializable))]
internal sealed partial class JsonContext : JsonSerializerContext
{

}
