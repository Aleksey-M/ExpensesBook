using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

public interface IIncomesService
{
    Task<Income> AddIncome(DateTimeOffset date, double amounth, string description);

    Task<List<Income>> GetIncomes(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter, CancellationToken token);

    Task UpdateIncome(Guid incomeId, DateTimeOffset? date, double? amounth, string? description);

    Task DeleteIncome(Guid incomeId);
}

public sealed class IncomesService : IIncomesService
{
    private readonly IIncomesRepository _incomesRepo;

    public IncomesService(IIncomesRepository incomesRepo)
    {
        _incomesRepo = incomesRepo;
    }

    public async Task<Income> AddIncome(DateTimeOffset date, double amounth, string description)
    {
        if (amounth <= 0) throw new ArgumentException("'Amount' should be positive and greater than 0");

        var income = new Income
        {
            Id = Guid.NewGuid(),
            Date = date,
            Description = description,
            Amounth = amounth
        };

        await _incomesRepo.AddIncome(income);

        return income;
    }

    public async Task DeleteIncome(Guid incomeId) => await _incomesRepo.DeleteIncome(incomeId);

    public async Task<List<Income>> GetIncomes(DateTimeOffset? startDate, DateTimeOffset? endDate,
        string? filter, CancellationToken token)
    {
        var incomes = await _incomesRepo.GetIncomes(token);

        Func<Income, bool> datePredicate = (startDate, endDate) switch
        {
            (null, null) => _ => true,
            (null, not null) => i => i.Date <= endDate,
            (not null, null) => i => i.Date >= startDate,
            (not null, not null) => i => i.Date <= endDate && i.Date >= startDate
        };

        Func<Income, bool> descriptionPredicate = string.IsNullOrWhiteSpace(filter)
            ? (_ => true)
            : (i => i.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));

        return incomes
            .Where(i => datePredicate(i) && descriptionPredicate(i))
            .OrderBy(i => i.Date)
            .ToList();
    }

    public async Task UpdateIncome(Guid incomeId, DateTimeOffset? date, double? amounth, string? description)
    {
        if (date is null && amounth is null && description is null) return;

        var incomes = await _incomesRepo.GetIncomes(token: default);
        var income = incomes.SingleOrDefault(i => i.Id == incomeId);

        if (income is null) throw new Exception($"Income with Id='{incomeId}' does not exists");

        var newIncome = new Income
        {
            Id = income.Id,
            Amounth = amounth ?? income.Amounth,
            Date = date ?? income.Date,
            Description = description ?? income.Description
        };

        await _incomesRepo.UpdateIncome(newIncome);
    }
}
