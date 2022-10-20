using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using ExpensesBook.Serialization;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class ExpensesRepository : BaseLocalStorageRepository<Expense>, IExpensesRepository
{
    protected override string CollectionName => "explistpart";

    public ExpensesRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task Clear()
    {
        var keys = await GetKeys();
        foreach (var k in keys)
        {
            if (k.StartsWith(CollectionName, StringComparison.OrdinalIgnoreCase))
            {
                await LocalStorage.RemoveItemAsync(k);
            }
        }
    }

    public async Task AddExpenses(IEnumerable<Expense> expenses)
    {
        var groupedCollection = expenses.GroupBy(e => (year: e.Date.Year, month: e.Date.Month));
        foreach (var g in groupedCollection)
        {
            var key = $"{CollectionName}.{g.Key.year}.{g.Key.month}";
            var value = g.ToList();

            var serialized = EntitiesJsonSerializer.GetUtf8JsonString(value);
            await LocalStorage.SetItemAsStringAsync(key, serialized);
        }
    }

    public new async Task<List<Expense>> GetCollection()
    {
        var keys = await GetKeys();
        var expenses = new List<Expense>();

        foreach (var k in keys)
        {
            if (k.StartsWith(CollectionName, StringComparison.OrdinalIgnoreCase))
            {
                var part = await LocalStorage.GetItemAsStringAsync(k);
                var items = EntitiesJsonSerializer.GetEntityFromUtf8Json<List<Expense>>(part);

                expenses.AddRange(items);
            }
        }

        return expenses;
    }

    private static int PartKeyToComparableInt(string s)
    {
        var arr = s.Split('.');
        int y = int.Parse(arr[1]);
        int m = int.Parse(arr[2]);
        return y * 100 + m;
    }

    private string DateToPartKey(DateTime date) => $"{CollectionName}.{date.Year}.{date.Month}";

    public async Task<List<Expense>> GetCollection(DateTimeOffset? fromDate, DateTimeOffset? toDate)
    {
        if (fromDate is null && toDate is null) return await GetCollection();

        int? from = null;
        if (fromDate is not null)
        {
            from = fromDate.Value.Year * 100 + fromDate.Value.Month;
        }

        int? to = null;
        if (toDate is not null)
        {
            to = toDate.Value.Year * 100 + toDate.Value.Month;
        }

        Func<int, bool> filter = (from, to) switch
        {
            (not null, not null) => i => i >= from && i <= to,
            (null, not null) => i => i <= to,
            (not null, null) => i => i >= from,
            (null, null) => i => true
        };

        var allKeys = await GetKeys();
        var filteredKeys = new List<string>();

        var keys = allKeys
            .Where(k => k.StartsWith(CollectionName, StringComparison.OrdinalIgnoreCase))
            .Select(s => (key: s, dateVal: PartKeyToComparableInt(s)))
            .Where(d => filter(d.dateVal))
            .Select(d => d.key)
            .ToList();

        var expenses = new List<Expense>();
        Func<DateTimeOffset, bool> datesFilter = (fromDate, toDate) switch
        {
            (not null, not null) => d => d.Date >= fromDate && d.Date <= toDate,
            (null, not null) => d => d <= toDate,
            (not null, null) => d => d >= fromDate,
            (null, null) => d => true
        };

        foreach (var k in keys)
        {
            var part = await LocalStorage.GetItemAsStringAsync(k);
            var items = EntitiesJsonSerializer.GetEntityFromUtf8Json<List<Expense>>(part);
            expenses.AddRange(items.Where(e => datesFilter(e.Date)).ToList());
        }

        return expenses;
    }

    public new async Task AddEntity(Expense entity)
    {
        var key = DateToPartKey(entity.Date.Date);
        var partExists = await LocalStorage.ContainKeyAsync(key);

        if (partExists)
        {
            var part = await LocalStorage.GetItemAsStringAsync(key);
            var items = EntitiesJsonSerializer.GetEntityFromUtf8Json<List<Expense>>(part);
            items.Add(entity);

            var serialized = EntitiesJsonSerializer.GetUtf8JsonString(items);
            await LocalStorage.SetItemAsStringAsync(key, serialized);
        }
        else
        {
            var serialized = EntitiesJsonSerializer.GetUtf8JsonString(new List<Expense> { entity });
            await LocalStorage.SetItemAsStringAsync(key, serialized);
        }
    }

    public async Task AddExpense(Expense expense) => await AddEntity(expense);

    public async Task DeleteExpense(Guid expenseId, DateTimeOffset expenseDate)
    {
        var partKey = DateToPartKey(expenseDate.Date);
        var partExists = await LocalStorage.ContainKeyAsync(partKey);

        if (!partExists) return;

        var part = await LocalStorage.GetItemAsStringAsync(partKey);
        var items = EntitiesJsonSerializer.GetEntityFromUtf8Json<List<Expense>>(part);
        var e = items.SingleOrDefault(e => e.Id == expenseId);

        if (e == null) return;

        items.Remove(e);

        if (items.Count == 0)
        {
            await LocalStorage.RemoveItemAsync(partKey);
        }
        else
        {
            var serialized = EntitiesJsonSerializer.GetUtf8JsonString(items);
            await LocalStorage.SetItemAsStringAsync(partKey, serialized);
        }
    }

    public async Task<List<Expense>> GetExpenses(DateTimeOffset? fromDate, DateTimeOffset? toDate) =>
        await GetCollection(fromDate, toDate);

    public async Task UpdateExpense(Expense expense, DateTimeOffset oldDate)
    {
        await DeleteExpense(expense.Id, oldDate);
        await AddExpense(expense);
    }

    public async Task<Expense> GetExpense(Guid expenseId, DateTimeOffset date)
    {
        var partKey = DateToPartKey(date.Date);
        var partExists = await LocalStorage.ContainKeyAsync(partKey);

        if (!partExists) throw new Exception($"Expense with Id='{expenseId}' does not exists");

        var part = await LocalStorage.GetItemAsStringAsync(partKey);
        var items = EntitiesJsonSerializer.GetEntityFromUtf8Json<List<Expense>>(part);
        var exp = items.SingleOrDefault(e => e.Id == expenseId);

        return exp ?? throw new Exception($"Expense with Id='{expenseId}' does not exists");
    }

    public async Task<List<(int year, int month)>> GetMonths()
    {
        var keys = await GetKeys();
        var result = new List<(int year, int month)>();

        foreach (var k in keys.Where(str => str.StartsWith(CollectionName, StringComparison.OrdinalIgnoreCase)))
        {
            var arr = k.Split('.');
            int y = int.Parse(arr[1]);
            int m = int.Parse(arr[2]);
            result.Add((y, m));
        }

        return result;
    }
}
