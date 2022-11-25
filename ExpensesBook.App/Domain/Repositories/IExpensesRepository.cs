using ExpensesBook.Domain.Entities;
using IdbLib;

namespace ExpensesBook.Domain.Repositories;

public interface IExpensesRepository
{
    Task AddExpense(Expense expense);

    Task<List<Expense>> GetExpenses(List<PropertyCriteria>? filters, CancellationToken token);

    Task UpdateExpense(Expense expense);

    Task DeleteExpense(Guid expenseId);

    Task<List<(int year, int month)>> GetMonths(CancellationToken token);

    Task AddExpenses(IEnumerable<Expense> expenses);

    Task Clear();
}