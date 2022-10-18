using System.Collections.Generic;
using System.Text.Json.Serialization;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Serialization;

[JsonSerializable(typeof(Category))]
internal sealed partial class CategoryJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(Group))]
internal sealed partial class GroupJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(Expense))]
internal sealed partial class ExpenseJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(GroupDefaultCategory))]
internal sealed partial class GroupDefaultCategoryJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(Income))]
internal sealed partial class IncomeJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(Limit))]
internal sealed partial class LimitJsonContext : JsonSerializerContext
{

}

[JsonSerializable(typeof(List<string>))]
internal sealed partial class StringListJsonContext : JsonSerializerContext
{

}