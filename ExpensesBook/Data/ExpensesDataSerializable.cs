using ExpensesBook.Domain.Entities;
using System.Collections.Generic;

namespace ExpensesBook.Data
{
    internal class ExpensesDataSerializable
    {
        public List<Category> Categories { get; set; } = null!;
        public List<Group> Groups { get; set; } = null!;
        public List<GroupDefaultCategory> GroupsDefaultCategories { get; set; } = null!;
        public List<Expense> Expenses { get; set; } = null!;
        public List<Limit> Limits { get; set; } = null!;
        public List<Income> Incomes { get; set; } = null!;

        internal static string SerializeToJson(ExpensesDataSerializable data) => System.Text.Json.JsonSerializer.Serialize(data);

        internal static ExpensesDataSerializable DeserializeFromJson(string json)
        {
            var deserializedData = System.Text.Json.JsonSerializer.Deserialize<ExpensesDataSerializable>(json) ?? new ExpensesDataSerializable();

            deserializedData.Categories ??= new List<Category>();
            deserializedData.Expenses ??= new List<Expense>();
            deserializedData.GroupsDefaultCategories ??= new List<GroupDefaultCategory>();
            deserializedData.Groups ??= new List<Group>();
            deserializedData.Limits ??= new List<Limit>();
            deserializedData.Incomes ??= new List<Income>();

            return deserializedData;
        }
    }
}
