using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IExpensesRepository
{
    Task AddExpense(Expense expense);

    Task<List<Expense>> GetExpenses();

    Task UpdateExpense(Expense expense);

    Task DeleteExpense(Guid expenseId);

    Task<List<(int year, int month)>> GetMonths();

    Task AddExpenses(IEnumerable<Expense> expenses);

    Task Clear();
}