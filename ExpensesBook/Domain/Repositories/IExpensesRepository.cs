using ExpensesBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpensesBook.Domain.Repositories
{
    internal interface IExpensesRepository
    {
        ValueTask AddExpense(Expense expense);
        ValueTask<Expense> GetExpense(Guid expenseId, DateTimeOffset date);
        ValueTask<List<Expense>> GetExpenses(DateTimeOffset? fromDate, DateTimeOffset? toDate);
        ValueTask UpdateExpense(Expense expense, DateTimeOffset oldDate);
        ValueTask DeleteExpense(Guid expenseId, DateTimeOffset expenseDate);
        ValueTask<List<(int year, int month)>> GetMonths();
    }
}
