﻿using System;
using System.Collections.Generic;
using System.Text.Json;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

internal sealed class GlobalDataSerializable
{
    public List<Category> Categories { get; set; } = new();

    public List<Group> Groups { get; set; } = new();

    public List<GroupDefaultCategory> GroupsDefaultCategories { get; set; } = new();

    public List<Expense> Expenses { get; set; } = new();

    public List<Limit> Limits { get; set; } = new();

    public List<Income> Incomes { get; set; } = new();
}

internal static class JsonGlobalData
{
    public static string Export(GlobalDataSerializable data) =>
        JsonSerializer.Serialize(data);

    public static GlobalDataSerializable Import(string jsonString) =>
        string.IsNullOrWhiteSpace(jsonString)
        ? new()
        : JsonSerializer.Deserialize<GlobalDataSerializable>(jsonString)
                ?? throw new Exception("Parsing Error");
}