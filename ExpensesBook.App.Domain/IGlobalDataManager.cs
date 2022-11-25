using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Data;

public sealed record ParsedExpense(Expense Expense, string CategoryName, string? GroupName);

public interface IGlobalDataManager
{
    Task<GlobalDataSerializable> GetAllData(CancellationToken token);

    Task SetAllData(GlobalDataSerializable data, CancellationToken token);

    Task ClearData();

    Task<int> ImportExpensesFromFlatList(List<ParsedExpense> parsedExpenses, CancellationToken token);
}