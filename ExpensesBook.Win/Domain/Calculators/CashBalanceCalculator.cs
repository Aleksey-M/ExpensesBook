using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Services;

namespace ExpensesBook.Domain.Calculators;

internal sealed class CashBalanceCalculator
{
    private readonly IIncomesService _incomesService;
    private readonly IExpensesService _expensesService;

    public CashBalanceCalculator(IIncomesService incomesService, IExpensesService expensesService)
    {
        _incomesService = incomesService;
        _expensesService = expensesService;
    }

    public async Task<CashBalance> GetCashBalance(DateTimeOffset fromDate, DateTimeOffset toDate, CancellationToken token)
    {
        if (fromDate.Date > toDate.Date) throw new ArgumentException(null, nameof(fromDate));

        var allExpenses = await _expensesService.GetExpenses(startDate: fromDate, endDate: toDate, filter: null, token);
        var allIncomes = await _incomesService.GetIncomes(startDate: fromDate, endDate: toDate, filter: null, token);

        double expenses = allExpenses.Select(e => e.Amounth).DefaultIfEmpty().Sum();
        double incomes = allIncomes.Select(s => s.Amounth).DefaultIfEmpty().Sum();

        return new CashBalance(fromDate: fromDate, toDate: toDate, expenses: expenses, incomes: incomes);
    }

    public async Task<(List<CashBalance> rows, CashBalance? total)> GetMonthlyCashBalance(CancellationToken token)
    {
        var allExpenses = await _expensesService.GetExpenses(startDate: null, endDate: null, filter: null, token);
        var allIncomes = await _incomesService.GetIncomes(startDate: null, endDate: null, filter: null, token);

        double totalExpenses = allExpenses.Select(e => e.Amounth).DefaultIfEmpty().Sum();
        double totalIncomes = allIncomes.Select(s => s.Amounth).DefaultIfEmpty().Sum();

        var monthlyExpenses = allExpenses
           .GroupBy(e => (e.Date.Year, e.Date.Month))
           .Select(g => (g.Key.Year, g.Key.Month, total: g.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
           .ToList();

        var monthlyIncomes = allIncomes
           .GroupBy(i => (i.Date.Year, i.Date.Month))
           .Select(g => (g.Key.Year, g.Key.Month, total: g.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
           .ToList();

        var allMonths = monthlyExpenses
            .Select(e => (e.Year, e.Month))
            .Union(monthlyIncomes.Select(i => (i.Year, i.Month)))
            .OrderBy(d => d.Year)
            .ThenBy(d => d.Month)
            .ToList();

        if (allMonths is { Count: 0 })
        {
            return (new List<CashBalance>(), null);
        }

        var start = allMonths.First();
        var startDate = new DateTimeOffset(start.Year, start.Month, 1, 0, 0, 0, TimeSpan.Zero);
        var end = allMonths.Last();
        var endDate = new DateTimeOffset(end.Year, end.Month, 1, 0, 0, 0, TimeSpan.Zero);

        var total = new CashBalance(
            fromDate: startDate,
            toDate: endDate,
            expenses: totalExpenses,
            incomes: totalIncomes,
            periodName: "Всего:");

        var rows = new List<CashBalance>();

        foreach (var (Year, Month) in allMonths)
        {
            var inc = monthlyIncomes.SingleOrDefault(i => i.Year == Year && i.Month == Month);
            var exp = monthlyExpenses.SingleOrDefault(e => e.Year == Year && e.Month == Month);
            var firstDay = new DateTimeOffset(Year, Month, 1, 0, 0, 0, TimeSpan.Zero);
            var lastDay = new DateTimeOffset(Year, Month, DateTime.DaysInMonth(Year, Month), 0, 0, 0, TimeSpan.Zero);
            var yearAndMonth = firstDay.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("ru-RU"));

            rows.Add(new CashBalance(firstDay, lastDay, exp.total, inc.total, yearAndMonth));
        }

        return (rows, total);
    }
}

internal sealed class CashBalance
{
    public CashBalance(DateTimeOffset fromDate, DateTimeOffset toDate, double expenses, double incomes, string periodName = "")
    {
        FromDate = fromDate;
        ToDate = toDate;
        ExpensesNum = expenses;
        IncomesNum = incomes;
        PeriodName = periodName;
    }

    public DateTimeOffset FromDate { get; }

    public DateTimeOffset ToDate { get; }

    public double ExpensesNum { get; }

    public double IncomesNum { get; }

    public string PeriodName { get; }

    public string Expenses => ExpensesNum.ToString("N2", CultureInfo.InvariantCulture);

    public string StartPeriodMonth => FromDate.ToString("yyyy-MM");

    public string Incomes => IncomesNum.ToString("N2", CultureInfo.InvariantCulture);

    public double SavingsNum => IncomesNum - ExpensesNum;

    public string Savings => SavingsNum.ToString("N2", CultureInfo.InvariantCulture);

    public double SavingsPercentNum => SavingsNum > 0 ? SavingsNum / IncomesNum : 0;

    public string SavingsPercent => SavingsPercentNum.ToString("P", CultureInfo.InvariantCulture);

    public string SavingsCSSStyle => SavingsNum >= 0 ? "color:forestgreen" : "color:red";
}

