using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public sealed class LimitsRepository : BaseIndexedDbRepository<Limit>, ILimitsRepository
{
    protected override string CollectionName => "limits";

    public LimitsRepository(IndexedDbManager manager) : base(manager)
    {
    }

    public async Task AddLimit(Limit limit) => await AddEntity(limit);

    public async Task DeleteLimit(Guid limitId) => await DeleteEntity(limitId);

    public async Task<List<Limit>> GetLimits(CancellationToken token) => await GetCollection() ?? new();

    public async Task UpdateLimit(Limit limit) => await UpdateEntity(limit);

    public async Task AddLimits(IEnumerable<Limit> limits) => await SetCollection(limits.ToList());

    public async Task Clear() => await Clear<List<Limit>>();
}
