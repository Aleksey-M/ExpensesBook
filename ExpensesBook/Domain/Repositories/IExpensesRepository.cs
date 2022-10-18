using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IExpensesRepository
{
    Task AddExpense(Expense expense);

    Task<Expense> GetExpense(Guid expenseId, DateTimeOffset date);

    Task<List<Expense>> GetExpenses(DateTimeOffset? fromDate, DateTimeOffset? toDate);

    Task UpdateExpense(Expense expense, DateTimeOffset oldDate);

    Task DeleteExpense(Guid expenseId, DateTimeOffset expenseDate);

    Task<List<(int year, int month)>> GetMonths();

    Task AddExpenses(IEnumerable<Expense> expenses);

    Task Clear();
}
