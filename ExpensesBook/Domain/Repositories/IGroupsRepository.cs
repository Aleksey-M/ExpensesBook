using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface IGroupsRepository
{
    ValueTask AddGroup(Group group);

    ValueTask<List<Group>> GetGroups();

    ValueTask UpdateGroup(Group group);

    ValueTask DeleteGroup(Guid groupId);
}
