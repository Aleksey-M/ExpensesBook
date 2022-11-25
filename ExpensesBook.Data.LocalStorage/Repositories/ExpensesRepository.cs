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

    public async Task<List<Expense>> GetExpenses(DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? filter, CancellationToken token)
    {
        filter ??= "";

        Func<string, bool> descriptionFilter = filter == ""
            ? _ => true
            : desc => desc.Contains(filter, StringComparison.OrdinalIgnoreCase);

        var fullList = await GetCollection(token) ?? new();

        Func<DateTimeOffset, bool> datesFilter = (startDate, endDate) switch
        {
            (not null, not null) => d => d.Date >= startDate && d.Date <= endDate,
            (null, not null) => d => d <= endDate,
            (not null, null) => d => d >= startDate,
            (null, null) => d => true
        };

        return fullList
            .Where(x => datesFilter(x.Date))
            .Where(exp => descriptionFilter(exp.Description))
            .OrderBy(exp => exp.Date)
            .ToList();
    }

    public async Task UpdateExpense(Expense expense) => await UpdateEntity(expense);

    public async Task AddExpenses(IEnumerable<Expense> expenses) => await SetCollection(expenses.ToList());

    public async Task Clear() => await Clear<List<Expense>>();

    public async Task<List<(int year, int month)>> GetMonths(CancellationToken token)
    {
        var allExpenses = await GetExpenses(
            startDate: null,
            endDate: null,
            filter: null, token);

        return allExpenses
            .Select(x => (year: x.Date.Year, month: x.Date.Month))
            .Distinct()
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
            .ToList();
    }
}