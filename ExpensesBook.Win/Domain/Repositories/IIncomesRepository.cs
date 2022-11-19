using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IIncomesRepository
{
    Task AddIncome(Income income);

    Task<List<Income>> GetIncomes(CancellationToken token);

    Task UpdateIncome(Income income);

    Task DeleteIncome(Guid incomeId);

    Task AddIncomes(IEnumerable<Income> incomes);

    Task Clear();
}
