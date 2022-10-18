using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class LimitsRepository : BaseLocalStorageRepository, ILimitsRepository
{
    protected override string CollectionName => "limits";

    public LimitsRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task AddLimit(Limit limit) => await AddEntity(limit);

    public async Task DeleteLimit(Guid limitId) => await DeleteEntity<Limit>(limitId);

    public async Task<List<Limit>> GetLimits() => await GetCollection<List<Limit>>() ?? new();

    public async Task UpdateLimit(Limit limit) => await UpdateEntity(limit);

    public async Task AddLimits(IEnumerable<Limit> limits) => await SetCollection(limits.ToList());

    public async Task Clear() => await Clear<List<Limit>>();
}
