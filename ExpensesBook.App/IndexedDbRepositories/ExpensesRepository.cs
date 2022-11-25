using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;


public sealed class ExpensesRepository : BaseIndexedDbRepository<Expense>, IExpensesRepository
{
    protected override string CollectionName => "expenses";

    public ExpensesRepository(IndexedDbManager manager) : base(manager)
    {
    }

    public async Task AddExpense(Expense expense) => await AddEntity(expense);

    public async Task DeleteExpense(Guid expenseId) => await DeleteEntity(expenseId);

    public async Task<List<Expense>> GetExpenses(List<PropertyCriteria>? filters, CancellationToken token) =>
        await GetCollection(filters) ?? new();

    public async Task UpdateExpense(Expense expense) => await UpdateEntity(expense);

    public async Task AddExpenses(IEnumerable<Expense> expenses) => await SetCollection(expenses.ToList());

    public async Task Clear() => await Clear<List<Expense>>();

    public async Task<List<(int year, int month)>> GetMonths(CancellationToken token)
    {
        var allExpenses = await GetExpenses(filters: null, token);

        return allExpenses
            .Select(x => (year: x.Date.Year, month: x.Date.Month))
            .Distinct()
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
            .ToList();
    }
}