using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class GroupsRepository : IGroupsRepository, ILocalStorageGenericRepository<Group>
{
    public GroupsRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async ValueTask AddGroup(Group group) =>
        await (this as ILocalStorageGenericRepository<Group>).AddEntity(group);

    public async ValueTask DeleteGroup(Guid groupId) =>
        await (this as ILocalStorageGenericRepository<Group>).DeleteEntity(groupId);

    public async ValueTask<List<Group>> GetGroups() =>
        await (this as ILocalStorageGenericRepository<Group>).GetCollection();

    public async ValueTask UpdateGroup(Group group) =>
        await (this as ILocalStorageGenericRepository<Group>).UpdateEntity(group);
}
