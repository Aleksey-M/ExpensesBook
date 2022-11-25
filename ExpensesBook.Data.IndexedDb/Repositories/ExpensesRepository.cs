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

    public async Task<List<Expense>> GetExpenses(DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? filter, CancellationToken token)
    {
        var filters = new List<PropertyCriteria>();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            filters.Add(new PropertyCriteria
            {
                Type = PropertyCriteriaType.ContainsString,
                PropertyJsName = "description",
                Value = filter
            });
        }

        if (startDate.HasValue && startDate.Value != default)
        {
            filters.Add(new PropertyCriteria
            {
                Type = PropertyCriteriaType.GreaterThan,
                PropertyJsName = "date",
                Value = startDate.Value
            });
        }

        if (endDate.HasValue && endDate.Value != default)
        {
            filters.Add(new PropertyCriteria
            {
                Type = PropertyCriteriaType.LessThan,
                PropertyJsName = "date",
                Value = endDate.Value
            });
        }

        return await GetCollection(filters) ?? new();
    }


    public async Task UpdateExpense(Expense expense) => await UpdateEntity(expense);

    public async Task AddExpenses(IEnumerable<Expense> expenses) => await SetCollection(expenses.ToList());

    public async Task Clear() => await Clear<List<Expense>>();

    public async Task<List<(int year, int month)>> GetMonths(CancellationToken token)
    {
        var allExpenses = await GetExpenses(
            startDate: null,
            endDate: null,
            filter: null,
            token);

        return allExpenses
            .Select(x => (year: x.Date.Year, month: x.Date.Month))
            .Distinct()
            .OrderBy(x => x.year)
            .ThenBy(x => x.month)
            .ToList();
    }
}