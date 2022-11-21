using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

public interface IExpensesRepository
{
    Task AddExpense(Expense expense);

    Task<List<Expense>> GetExpenses(CancellationToken token);

    Task UpdateExpense(Expense expense);

    Task DeleteExpense(Guid expenseId);

    Task<List<(int year, int month)>> GetMonths(CancellationToken token);

    Task AddExpenses(IEnumerable<Expense> expenses);

    Task Clear();
}