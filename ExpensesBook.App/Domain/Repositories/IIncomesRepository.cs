using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

public interface IIncomesRepository
{
    Task AddIncome(Income income);

    Task<List<Income>> GetIncomes(CancellationToken token);

    Task UpdateIncome(Income income);

    Task DeleteIncome(Guid incomeId);

    Task AddIncomes(IEnumerable<Income> incomes);

    Task Clear();
}
