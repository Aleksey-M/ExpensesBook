using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Serialization;

internal static class EntitiesJsonSerializer
{
    public static string GetUtf8JsonString<T>(T entity)
    {
        byte[] utf8Json = entity switch
        {
            Category category => JsonSerializer.SerializeToUtf8Bytes(category, CategoryJsonContext.Default.Category),
            Group group => JsonSerializer.SerializeToUtf8Bytes(group, GroupJsonContext.Default.Group),
            Limit limit => JsonSerializer.SerializeToUtf8Bytes(limit, LimitJsonContext.Default.Limit),
            Income income => JsonSerializer.SerializeToUtf8Bytes(income, IncomeJsonContext.Default.Income),
            Expense expense => JsonSerializer.SerializeToUtf8Bytes(expense, ExpenseJsonContext.Default.Expense),
            GroupDefaultCategory groupDefaultCategory => JsonSerializer.SerializeToUtf8Bytes(groupDefaultCategory,
                GroupDefaultCategoryJsonContext.Default.GroupDefaultCategory),
            List<string> list => JsonSerializer.SerializeToUtf8Bytes(list, StringListJsonContext.Default.ListString),
            _ => throw new Exception("Unknown entity type")
        };

        return Encoding.UTF8.GetString(utf8Json);
    }

    public static T GetEntityFromUtf8Json<T>(string utf8String)
    {
        byte[] utf8Json = Encoding.UTF8.GetBytes(utf8String); 

        object? entity = typeof(T) switch
        {
            Type t when t == typeof(Category) => JsonSerializer.Deserialize(utf8Json, CategoryJsonContext.Default.Category),
            Type t when t == typeof(Group) => JsonSerializer.Deserialize(utf8Json, GroupJsonContext.Default.Group),
            Type t when t == typeof(Limit) => JsonSerializer.Deserialize(utf8Json, LimitJsonContext.Default.Limit),
            Type t when t == typeof(Income) => JsonSerializer.Deserialize(utf8Json, IncomeJsonContext.Default.Income),
            Type t when t == typeof(Expense) => JsonSerializer.Deserialize(utf8Json, ExpenseJsonContext.Default.Expense),
            Type t when t == typeof(GroupDefaultCategory) => JsonSerializer.Deserialize(utf8Json,
                GroupDefaultCategoryJsonContext.Default.GroupDefaultCategory),
            Type t when t == typeof(List<string>) => JsonSerializer.Deserialize(utf8Json, StringListJsonContext.Default.ListString),
            _ => throw new Exception("Unknown entity type")
        };

        if (entity == null)
        {
            throw new Exception("Parsing Error");
        }

        return (T)entity;
    }
}
