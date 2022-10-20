using System;
using System.Collections.Generic;
using System.Text.Json;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Serialization;

internal static class EntitiesJsonSerializer
{
    public static string GetJsonString<T>(T entitiesList)
    {
        string jsonString = entitiesList switch
        {
            List<Category> list => JsonSerializer.Serialize(list, CategoryJsonContext.Default.ListCategory),
            List<Group> list => JsonSerializer.Serialize(list, GroupJsonContext.Default.ListGroup),
            List<Limit> list => JsonSerializer.Serialize(list, LimitJsonContext.Default.ListLimit),
            List<Income> list => JsonSerializer.Serialize(list, IncomeJsonContext.Default.ListIncome),
            List<Expense> list => JsonSerializer.Serialize(list, ExpenseJsonContext.Default.ListExpense),
            List<GroupDefaultCategory> list => JsonSerializer.Serialize(list,
                GroupDefaultCategoryJsonContext.Default.ListGroupDefaultCategory),
            List<string> list => JsonSerializer.Serialize(list, StringListJsonContext.Default.ListString),
            _ => throw new Exception("Unknown entity type")
        };

        return jsonString;
    }

    public static T GetEntitiesFromJsonString<T>(string jsonString)
    {
        object? entity = typeof(T) switch
        {
            Type t when t == typeof(List<Category>) => JsonSerializer.Deserialize(jsonString, CategoryJsonContext.Default.ListCategory),
            Type t when t == typeof(List<Group>) => JsonSerializer.Deserialize(jsonString, GroupJsonContext.Default.ListGroup),
            Type t when t == typeof(List<Limit>) => JsonSerializer.Deserialize(jsonString, LimitJsonContext.Default.ListLimit),
            Type t when t == typeof(List<Income>) => JsonSerializer.Deserialize(jsonString, IncomeJsonContext.Default.ListIncome),
            Type t when t == typeof(List<Expense>) => JsonSerializer.Deserialize(jsonString, ExpenseJsonContext.Default.ListExpense),
            Type t when t == typeof(List<GroupDefaultCategory>) => JsonSerializer.Deserialize(jsonString,
                GroupDefaultCategoryJsonContext.Default.ListGroupDefaultCategory),
            Type t when t == typeof(List<string>) => JsonSerializer.Deserialize(jsonString, StringListJsonContext.Default.ListString),
            _ => throw new Exception("Unknown entity type")
        };

        if (entity == null)
        {
            throw new Exception("Parsing Error");
        }

        return (T)entity;
    }
}
