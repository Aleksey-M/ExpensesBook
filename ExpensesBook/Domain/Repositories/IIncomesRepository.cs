using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IIncomesRepository
{
    ValueTask AddIncome(Income income);

    ValueTask<List<Income>> GetIncomes();

    ValueTask UpdateIncome(Income income);

    ValueTask DeleteIncome(Guid incomeId);
}
