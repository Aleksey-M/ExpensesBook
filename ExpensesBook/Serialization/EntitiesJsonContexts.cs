using System.Collections.Generic;
using System.Text.Json.Serialization;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Serialization;

[JsonSerializable(typeof(List<Category>))]
internal sealed partial class CategoryJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Group>))]
internal sealed partial class GroupJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Expense>))]
internal sealed partial class ExpenseJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<GroupDefaultCategory>))]
internal sealed partial class GroupDefaultCategoryJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Income>))]
internal sealed partial class IncomeJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<Limit>))]
internal sealed partial class LimitJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<string>))]
internal sealed partial class StringListJsonContext : JsonSerializerContext
{

}