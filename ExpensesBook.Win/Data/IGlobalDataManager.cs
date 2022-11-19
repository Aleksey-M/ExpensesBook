using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

internal sealed record ParsedExpense(Expense Expense, string CategoryName, string? GroupName);

internal interface IGlobalDataManager
{
    Task<GlobalDataSerializable> GetAllData(CancellationToken token);

    Task SetAllData(GlobalDataSerializable data, CancellationToken token);

    Task ClearData();

    Task<int> ImportExpensesFromFlatList(List<ParsedExpense> parsedExpenses, CancellationToken token);
}