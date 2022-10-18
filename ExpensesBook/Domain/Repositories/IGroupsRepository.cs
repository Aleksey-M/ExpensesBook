using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IGroupsListRepository
{
    Task<List<Group>> GetGroups();
}

internal interface IAsyncGroupsListRepository
{
    IAsyncEnumerable<Group> GetGroupsAsyncEnumerable();
}

internal interface IGroupsRepository
{
    Task AddGroup(Group group);

    Task UpdateGroup(Group group);

    Task DeleteGroup(Guid groupId);
}
