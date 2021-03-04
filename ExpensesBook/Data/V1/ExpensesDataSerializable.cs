using System.Collections.Generic;

namespace ExpensesBook.Data.V1
{
    internal class ExpensesDataSerializable
    {
        public List<ExpensesCategory> Categories { get; set; } = null!;
        public List<ExpensesGroup> Groups { get; set; } = null!;
        public List<ExpenseItem> Expenses { get; set; } = null!;
        public List<Limit> Limits { get; set; } = null!;
        public List<Savings> Savings { get; set; } = null!;

        internal static string SerializeToJson(ExpensesDataSerializable data) => System.Text.Json.JsonSerializer.Serialize(data);

        internal static ExpensesDataSerializable DeserializeFromJson(string json)
        {
            var deserializedData = System.Text.Json.JsonSerializer.Deserialize<ExpensesDataSerializable>(json) ?? new ExpensesDataSerializable();

            deserializedData.Categories ??= new List<ExpensesCategory>();
            deserializedData.Expenses ??= new List<ExpenseItem>();
            deserializedData.Groups ??= new List<ExpensesGroup>();
            deserializedData.Limits ??= new List<Limit>();
            deserializedData.Savings ??= new List<Savings>();

            return deserializedData;
        }
    }
}
