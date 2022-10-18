using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Serialization;

internal static class EntitiesJsonSerializer
{
    public static string GetUtf8JsonString<T>(T entitiesList)
    {
        byte[] utf8Json = entitiesList switch
        {
            List<Category> list => JsonSerializer.SerializeToUtf8Bytes(list,
                CategoryJsonContext.Default.ListCategory),
            List<Group> list => JsonSerializer.SerializeToUtf8Bytes(list, GroupJsonContext.Default.ListGroup),
            List<Limit> list => JsonSerializer.SerializeToUtf8Bytes(list, LimitJsonContext.Default.ListLimit),
            List<Income> list => JsonSerializer.SerializeToUtf8Bytes(list, IncomeJsonContext.Default.ListIncome),
            List<Expense> list => JsonSerializer.SerializeToUtf8Bytes(list, ExpenseJsonContext.Default.ListExpense),
            List<GroupDefaultCategory> list => JsonSerializer.SerializeToUtf8Bytes(list,
                GroupDefaultCategoryJsonContext.Default.ListGroupDefaultCategory),
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
            Type t when t == typeof(List<Category>) => JsonSerializer.Deserialize(utf8Json, CategoryJsonContext.Default.ListCategory),
            Type t when t == typeof(List<Group>) => JsonSerializer.Deserialize(utf8Json, GroupJsonContext.Default.ListGroup),
            Type t when t == typeof(List<Limit>) => JsonSerializer.Deserialize(utf8Json, LimitJsonContext.Default.ListLimit),
            Type t when t == typeof(List<Income>) => JsonSerializer.Deserialize(utf8Json, IncomeJsonContext.Default.ListIncome),
            Type t when t == typeof(List<Expense>) => JsonSerializer.Deserialize(utf8Json, ExpenseJsonContext.Default.ListExpense),
            Type t when t == typeof(List<GroupDefaultCategory>) => JsonSerializer.Deserialize(utf8Json,
                GroupDefaultCategoryJsonContext.Default.ListGroupDefaultCategory),
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
