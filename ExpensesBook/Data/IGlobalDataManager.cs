using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

internal sealed record ParsedExpense(Expense Expense, string CategoryName, string? GroupName);

internal interface IGlobalDataManager
{
    Task<GlobalDataSerializable> GetAllData();

    Task SetAllData(GlobalDataSerializable data);

    Task ClearData();

    Task<int> ImportExpensesFromFlatList(List<ParsedExpense> parsedExpenses);
}