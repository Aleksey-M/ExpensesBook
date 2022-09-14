using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

internal interface IIncomesService
{
    ValueTask<Income> AddIncome(DateTimeOffset date, double amounth, string description);

    ValueTask<List<Income>> GetIncomes(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter);

    ValueTask UpdateIncome(Guid incomeId, DateTimeOffset? date, double? amounth, string? description);

    ValueTask DeleteIncome(Guid incomeId);
}

internal class IncomesService : IIncomesService
{
    private readonly IIncomesRepository _incomesRepo;

    public IncomesService(IIncomesRepository incomesRepo)
    {
        _incomesRepo = incomesRepo;
    }

    public async ValueTask<Income> AddIncome(DateTimeOffset date, double amounth, string description)
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

    public async ValueTask DeleteIncome(Guid incomeId) => await _incomesRepo.DeleteIncome(incomeId);

    public async ValueTask<List<Income>> GetIncomes(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter)
    {
        var incomes = await _incomesRepo.GetIncomes();

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

    public async ValueTask UpdateIncome(Guid incomeId, DateTimeOffset? date, double? amounth, string? description)
    {
        if (date is null && amounth is null && description is null) return;

        var incomes = await _incomesRepo.GetIncomes();
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
