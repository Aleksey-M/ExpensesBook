using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class LimitsRepository : ILimitsRepository, ILocalStorageGenericRepository<Limit>
{
    public LimitsRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async ValueTask AddLimit(Limit limit) =>
        await (this as ILocalStorageGenericRepository<Limit>).AddEntity(limit);

    public async ValueTask DeleteLimit(Guid limitId) =>
        await (this as ILocalStorageGenericRepository<Limit>).DeleteEntity(limitId);

    public async ValueTask<List<Limit>> GetLimits() =>
        await (this as ILocalStorageGenericRepository<Limit>).GetCollection();

    public async ValueTask UpdateLimit(Limit limit) =>
        await (this as ILocalStorageGenericRepository<Limit>).UpdateEntity(limit);

}
