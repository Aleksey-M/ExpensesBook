using System.Collections.Generic;
using System.Text.Json;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

internal class ExpensesDataSerializable
{
    public List<Category> Categories { get; set; } = null!;

    public List<Group> Groups { get; set; } = null!;

    public List<GroupDefaultCategory> GroupsDefaultCategories { get; set; } = null!;

    public List<Expense> Expenses { get; set; } = null!;

    public List<Limit> Limits { get; set; } = null!;

    public List<Income> Incomes { get; set; } = null!;

    internal static string SerializeToJson(ExpensesDataSerializable data) => JsonSerializer.Serialize(data);

    internal static ExpensesDataSerializable DeserializeFromJson(string json)
    {
        var deserializedData = JsonSerializer.Deserialize<ExpensesDataSerializable>(json) ?? new();

        deserializedData.Categories ??= new();
        deserializedData.Expenses ??= new();
        deserializedData.GroupsDefaultCategories ??= new();
        deserializedData.Groups ??= new();
        deserializedData.Limits ??= new();
        deserializedData.Incomes ??= new();

        return deserializedData;
    }
}
