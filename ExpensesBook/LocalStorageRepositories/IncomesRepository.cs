using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal class IncomesRepository : IIncomesRepository, ILocalStorageGenericRepository<Income>
{
    public IncomesRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async ValueTask AddIncome(Income income) =>
        await (this as ILocalStorageGenericRepository<Income>).AddEntity(income);

    public async ValueTask DeleteIncome(Guid incomeId) =>
        await (this as ILocalStorageGenericRepository<Income>).DeleteEntity(incomeId);

    public async ValueTask<List<Income>> GetIncomes() =>
        await (this as ILocalStorageGenericRepository<Income>).GetCollection();

    public async ValueTask UpdateIncome(Income income) =>
        await (this as ILocalStorageGenericRepository<Income>).UpdateEntity(income);

}
