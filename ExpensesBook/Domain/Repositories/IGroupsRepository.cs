using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IGroupsRepository
{
    Task AddGroup(Group group);

    Task<List<Group>> GetGroups();

    Task UpdateGroup(Group group);

    Task DeleteGroup(Guid groupId);
}
