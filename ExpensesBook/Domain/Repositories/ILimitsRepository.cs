using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface ILimitsRepository
{
    Task AddLimit(Limit limit);

    Task<List<Limit>> GetLimits();

    Task UpdateLimit(Limit limit);

    Task DeleteLimit(Guid limitId);

    Task AddLimits(IEnumerable<Limit> limits);

    Task Clear();
}
