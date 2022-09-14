using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal class ExpensesRepository : IExpensesRepository, ILocalStorageGenericRepository<Expense>
{
    public ExpensesRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async ValueTask Clear()
    {
        var keys = await (this as ILocalStorageGenericRepository<Expense>).GetKeys();
        foreach (var k in keys)
        {
            if (k.StartsWith("ExpListPart", StringComparison.OrdinalIgnoreCase))
            {
                await LocalStorage.RemoveItemAsync(k);
            }
        }
    }

    public async ValueTask SetCollection(IEnumerable<Expense> collection)
    {
        var groupedCollection = collection.GroupBy(e => (year: e.Date.Year, month: e.Date.Month));
        foreach (var g in groupedCollection)
        {
            var key = $"ExpListPart.{g.Key.year}.{g.Key.month}";
            var value = g.ToList();
            await LocalStorage.SetItemAsync(key, value);
        }
    }

    public async ValueTask<List<Expense>> GetCollection()
    {
        var keys = await (this as ILocalStorageGenericRepository<Expense>).GetKeys();
        var expenses = new List<Expense>();

        foreach (var k in keys)
        {
            if (k.StartsWith("ExpListPart", StringComparison.OrdinalIgnoreCase))
            {
                var part = await LocalStorage.GetItemAsync<List<Expense>>(k);
                expenses.AddRange(part);
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

    private static string DateToPartKey(DateTime date) => $"ExpListPart.{date.Year}.{date.Month}";

    public async ValueTask<List<Expense>> GetCollection(DateTimeOffset? fromDate, DateTimeOffset? toDate)
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

        var allKeys = await (this as ILocalStorageGenericRepository<Expense>).GetKeys();
        var filteredKeys = new List<string>();

        var keys = allKeys
            .Where(k => k.StartsWith("ExpListPart", StringComparison.OrdinalIgnoreCase))
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
            var part = await LocalStorage.GetItemAsync<List<Expense>>(k);
            expenses.AddRange(part.Where(e => datesFilter(e.Date)).ToList());
        }

        return expenses;
    }

    public async ValueTask AddEntity(Expense entity)
    {
        var key = DateToPartKey(entity.Date.Date);
        var partExists = await LocalStorage.ContainKeyAsync(key);

        if (partExists)
        {
            var part = await LocalStorage.GetItemAsync<List<Expense>>(key);
            part.Add(entity);
            await LocalStorage.SetItemAsync(key, part);
        }
        else
        {
            await LocalStorage.SetItemAsync(key, new List<Expense> { entity });
        }
    }

    public ValueTask DeleteEntity(Guid entityId) => throw new Exception("Not Applicable For Expenses Repository");

    public async ValueTask AddExpense(Expense expense) => await AddEntity(expense);

    public async ValueTask DeleteExpense(Guid expenseId, DateTimeOffset expenseDate)
    {
        var partKey = DateToPartKey(expenseDate.Date);
        var partExists = await LocalStorage.ContainKeyAsync(partKey);

        if (!partExists) return;

        var part = await LocalStorage.GetItemAsync<List<Expense>>(partKey);
        var e = part.SingleOrDefault(e => e.Id == expenseId);

        if (e is null) return;

        part.Remove(e);
        if (part.Count == 0)
        {
            await LocalStorage.RemoveItemAsync(partKey);
        }
        else
        {
            await LocalStorage.SetItemAsync(partKey, part);
        }
    }

    public async ValueTask<List<Expense>> GetExpenses(DateTimeOffset? fromDate, DateTimeOffset? toDate) =>
        await GetCollection(fromDate, toDate);

    public async ValueTask UpdateExpense(Expense expense, DateTimeOffset oldDate)
    {
        await DeleteExpense(expense.Id, oldDate);
        await AddExpense(expense);
    }

    public async ValueTask<Expense> GetExpense(Guid expenseId, DateTimeOffset date)
    {
        var partKey = DateToPartKey(date.Date);
        var partExists = await LocalStorage.ContainKeyAsync(partKey);

        if (!partExists) throw new Exception($"Expense with Id='{expenseId}' does not exists");

        var part = await LocalStorage.GetItemAsync<List<Expense>>(partKey);
        var exp = part.SingleOrDefault(e => e.Id == expenseId);

        return exp ?? throw new Exception($"Expense with Id='{expenseId}' does not exists");
    }

    public async ValueTask<List<(int year, int month)>> GetMonths()
    {
        var keys = await (this as ILocalStorageGenericRepository<Expense>).GetKeys();
        var result = new List<(int year, int month)>();

        foreach (var k in keys.Where(str => str.StartsWith("ExpListPart", StringComparison.OrdinalIgnoreCase)))
        {
            var arr = k.Split('.');
            int y = int.Parse(arr[1]);
            int m = int.Parse(arr[2]);
            result.Add((y, m));
        }

        return result;
    }
}
