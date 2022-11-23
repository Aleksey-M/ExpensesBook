using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public sealed class GroupsRepository : BaseIndexedDbRepository<Group>, IGroupsRepository
{
    protected override string CollectionName => "groups";

    public GroupsRepository(IndexedDbManager manager) : base(manager)
    {
    }

    public async Task AddGroup(Group group) => await AddEntity(group);

    public async Task DeleteGroup(Guid groupId) => await DeleteEntity(groupId);

    public async Task<List<Group>> GetGroups(CancellationToken token) => await GetCollection() ?? new();

    public async Task UpdateGroup(Group group) => await UpdateEntity(group);

    public async Task AddGroups(IEnumerable<Group> groups) => await SetCollection(groups.ToList());

    public async Task Clear() => await Clear<List<Group>>();
}
