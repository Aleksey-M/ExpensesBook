using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

public interface IGroupsRepository
{
    Task AddGroup(Group group);

    Task UpdateGroup(Group group);

    Task DeleteGroup(Guid groupId);

    Task<List<Group>> GetGroups(CancellationToken token);

    Task AddGroups(IEnumerable<Group> groups);

    Task Clear();
}
