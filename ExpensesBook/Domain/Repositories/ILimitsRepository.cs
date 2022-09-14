using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface ILimitsRepository
{
    ValueTask AddLimit(Limit limit);

    ValueTask<List<Limit>> GetLimits();

    ValueTask UpdateLimit(Limit limit);

    ValueTask DeleteLimit(Guid limitId);
}
