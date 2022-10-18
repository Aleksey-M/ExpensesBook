using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IGroupsRepository
{
    Task AddGroup(Group group);

    Task UpdateGroup(Group group);

    Task DeleteGroup(Guid groupId);

    Task<List<Group>> GetGroups();

    Task AddGroups(IEnumerable<Group> groups);

    Task Clear();
}
