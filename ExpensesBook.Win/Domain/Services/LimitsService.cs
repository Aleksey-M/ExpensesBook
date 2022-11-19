using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

internal interface ILimitsService
{
    Task<Limit> AddLimit(DateTimeOffset startDate, DateTimeOffset endDate, string description, double amounth);

    Task<List<Limit>> GetLimits(CancellationToken token);

    Task UpdateLimit(Guid limitId, DateTimeOffset? startDate, DateTimeOffset? endDate, string? description, double? amounth);

    Task DeleteLimit(Guid limitId);
}

internal sealed class LimitsService : ILimitsService
{
    private readonly ILimitsRepository _limitsRepo;

    public LimitsService(ILimitsRepository limitsRepo)
    {
        _limitsRepo = limitsRepo;
    }

    public async Task<Limit> AddLimit(DateTimeOffset startDate, DateTimeOffset endDate, string description, double amounth)
    {
        if (amounth <= 0) throw new ArgumentException("'Amount' should be positive and greater than 0");
        if (startDate >= endDate) throw new ArgumentException("EndDate should be greater than StartDate");

        var limit = new Limit
        {
            Id = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate,
            LimitAmounth = amounth,
            Description = description
        };

        await _limitsRepo.AddLimit(limit);

        return limit;
    }

    public async Task DeleteLimit(Guid limitId) => await _limitsRepo.DeleteLimit(limitId);

    public async Task<List<Limit>> GetLimits(CancellationToken token)
    {
        var limits = await _limitsRepo.GetLimits(token);
        return limits.OrderBy(l => l.StartDate).ThenBy(l => l.EndDate).ToList();
    }

    public async Task UpdateLimit(Guid limitId, DateTimeOffset? startDate, DateTimeOffset? endDate,
        string? description, double? amounth)
    {
        if (startDate is null && endDate is null && amounth is null && description is null) return;

        var limits = await _limitsRepo.GetLimits(token: default);
        var limit = limits.SingleOrDefault(i => i.Id == limitId);

        if (limit is null) throw new Exception($"Limit with Id='{limitId}' does not exists");

        var newLimit = new Limit
        {
            Id = limit.Id,
            LimitAmounth = amounth ?? limit.LimitAmounth,
            StartDate = startDate ?? limit.StartDate,
            EndDate = endDate ?? limit.EndDate,
            Description = description ?? limit.Description
        };

        await _limitsRepo.UpdateLimit(newLimit);
    }
}
