using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class GroupsRepository : BaseLocalStorageRepository<Group>, IGroupsRepository
{
    protected override string CollectionName => "groups";

    public GroupsRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task AddGroup(Group group) => await AddEntity(group);

    public async Task DeleteGroup(Guid groupId) => await DeleteEntity(groupId);

    public async Task<List<Group>> GetGroups(CancellationToken token) => await GetCollection(token) ?? new();

    public async Task UpdateGroup(Group group) => await UpdateEntity(group);

    public async Task AddGroups(IEnumerable<Group> groups) => await SetCollection(groups.ToList());

    public async Task Clear() => await Clear<List<Group>>();
}
