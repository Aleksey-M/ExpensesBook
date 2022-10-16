using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IIncomesRepository
{
    Task AddIncome(Income income);

    Task<List<Income>> GetIncomes();

    Task UpdateIncome(Income income);

    Task DeleteIncome(Guid incomeId);
}
