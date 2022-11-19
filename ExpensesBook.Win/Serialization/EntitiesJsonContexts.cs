using System.Collections.Generic;
using System.Text.Json.Serialization;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Serialization;

[JsonSerializable(typeof(List<Category>))]
internal partial class CategoryJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Group>))]
internal partial class GroupJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Expense>))]
internal partial class ExpenseJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<GroupDefaultCategory>))]
internal partial class GroupDefaultCategoryJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Income>))]
internal partial class IncomeJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Limit>))]
internal partial class LimitJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<string>))]
internal partial class StringListJsonContext : JsonSerializerContext
{

}