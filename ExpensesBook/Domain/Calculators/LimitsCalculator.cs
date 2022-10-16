using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Services;

namespace ExpensesBook.Domain.Calculators;

internal sealed class LimitsCalculator
{
    private readonly ILimitsService _limitsService;
    private readonly IExpensesService _expensesService;

    public LimitsCalculator(ILimitsService limitsService, IExpensesService expensesService)
    {
        _limitsService = limitsService;
        _expensesService = expensesService;
    }

    public async Task<List<CalculatedLimit>> GetCalculatedLimits(bool onlyActual, DateTimeOffset? currentDate = null)
    {
        currentDate ??= DateTimeOffset.Now.Date;

        var limits = await _limitsService.GetLimits();
        if (onlyActual)
        {
            limits = limits.Where(l => l.StartDate.Date <= currentDate && l.EndDate.Date >= currentDate).ToList();
        }

        if (limits.Count == 0) return new List<CalculatedLimit>();

        var minDate = limits.Select(l => l.StartDate.Date).Min();
        var maxDate = limits.Select(l => l.EndDate.Date).Max();

        var expenses = await _expensesService.GetExpenses(startDate: minDate, endDate: maxDate, filter: null);

        return limits
            .Select(l => new CalculatedLimit(l, expenses, currentDate.Value))
            .OrderBy(l => l.IsActual)
            .ThenBy(l => l.EndDate)
            .ToList();
    }

    public async Task<CalculatedLimit> GetCalculatedLimit(Limit limit, DateTimeOffset? currentDate = null)
    {
        currentDate ??= DateTimeOffset.Now.Date;

        var expenses = await _expensesService.GetExpenses(startDate: limit.StartDate, endDate: limit.EndDate, filter: null);

        return new CalculatedLimit(limit, expenses, currentDate.Value);
    }
}

internal sealed class CalculatedLimit
{
    public CalculatedLimit(Limit limit, IEnumerable<Expense> allExpenses, DateTimeOffset currentDate)
    {
        LimitId = limit.Id;
        Description = limit.Description;
        StartDate = limit.StartDate.Date;
        EndDate = limit.EndDate.Date;
        CurrentDate = currentDate.Date;
        LimitAmounthNum = limit.LimitAmounth;

        var expensesFromRange = allExpenses.Where(e => e.Date.Date >= StartDate && e.Date.Date <= EndDate).ToList();
        SpentNum = expensesFromRange.Select(e => e.Amounth).DefaultIfEmpty().Sum();

        LeftNum = LimitAmounthNum - SpentNum;
        if (LeftNum < 0)
        {
            DeficiteNum = -LeftNum;
            LeftNum = 0;
        }
        else
        {
            DeficiteNum = 0;
        }
    }

    public Guid LimitId { get; }

    public string Description { get; }

    public DateTimeOffset StartDate { get; }

    public DateTimeOffset EndDate { get; }

    public DateTimeOffset CurrentDate { get; }

    public double LimitAmounthNum { get; }

    public string LimitAmounth => LimitAmounthNum.ToString("N2");

    public bool IsActual => StartDate <= CurrentDate && EndDate >= CurrentDate;

    public string DatesRange => $@"{StartDate:yyyy.MM.dd} - {EndDate:yyyy.MM.dd}";

    public double SpentNum { get; }

    public double LeftNum { get; }

    public double DeficiteNum { get; }

    public string Spent => SpentNum.ToString("N2");

    public string Left => LeftNum.ToString("N2");

    public string Deficite => DeficiteNum.ToString("N2");

    public string RowColorCSSStyle => (SpentNum > LimitAmounthNum, IsActual) switch
    {
        (true, false) => "color: indianred",
        (true, true) => "color: red",
        (false, true) => "color: blue",
        (false, false) => ""
    };
}
