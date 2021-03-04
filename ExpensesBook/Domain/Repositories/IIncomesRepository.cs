using ExpensesBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpensesBook.Domain.Repositories
{
    internal interface IIncomesRepository
    {
        ValueTask AddIncome(Income income);
        ValueTask<List<Income>> GetIncomes();
        ValueTask UpdateIncome(Income income);
        ValueTask DeleteIncome(Guid incomeId);
    }
}
