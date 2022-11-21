using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;


public sealed class ExpensesRepository : BaseLocalStorageRepository<Expense>, IExpensesRepository
{
    protected override string CollectionName => "expenses";

    public ExpensesRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task AddExpense(Expense expense) => await AddEntity(expense);

    public async Task DeleteExpense(Guid expenseId) => await DeleteEntity(expenseId);

    public async Task<List<Expense>> GetExpenses(CancellationToken token) => await GetCollection(token) ?? new();

    public async Task UpdateExpense(Expense expense) => await UpdateEntity(expense);

    public async Task AddExpenses(IEnumerable<Expense> expenses) => await SetCollection(expenses.ToList());

    public async Task Clear() => await Clear<List<Expense>>();

    public async Task<List<(int year, int month)>> GetMonths(CancellationToken token)
    {
        var allExpenses = await GetExpenses(token);

        return allExpenses
            .Select(x => (year: x.Date.Year, month: x.Date.Month))
            .Distinct()
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
            .ToList();
    }
}