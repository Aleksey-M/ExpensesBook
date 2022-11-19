using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Services;

namespace ExpensesBook.Domain.Calculators;

#pragma warning disable CA1822 // Mark members as static
internal sealed class PeriodExpenseCalculator
{
    private readonly IExpensesService _expensesService;

    public PeriodExpenseCalculator(IExpensesService expensesService)
    {
        _expensesService = expensesService;
    }

    private TableRow[] GetExpensesList(
        List<(Expense item, Category category, Group? group)> expenses) => expenses
            .Select(exp => new TableRow(
                 exp.item.Date.ToString("yyyy.MM.dd"),
                 exp.item.Description,
                 exp.item.Amounth.ToString("N2"),
                 (Guid?)exp.item.Id))
            .ToArray();

    private TableRow[] GetExpensesListByDate(
        List<(Expense item, Category category, Group? group)> expenses, double total) => expenses
            .GroupBy(e => e.item.Date.Date)
            .Select(g => (
                g.Key.ToString("yyyy.MM.dd"),
                g.Select(e => e.item.Amounth).DefaultIfEmpty().Sum()))
            .Where(i => i.Item2 > 0)
            .Select(i => new TableRow(
                i.Item1,
                i.Item2.ToString("N2"),
                (i.Item2 * 100 / total).ToString("N2") + " %",
                null))
            .ToArray();

    private TableRow[] GetExpensesListByCategory(
        List<(Expense item, Category category, Group? group)> expenses, double total) => expenses
            .GroupBy(exp => exp.category)
            .OrderBy(g => g.Key.Name)
            .Select(g => (
                g.Key.Name,
                g.Key.Id,
                g.Select(e => e.item.Amounth).DefaultIfEmpty().Sum()))
            .Where(i => i.Item3 > 0)
            .Select(i => new TableRow(
                i.Name,
                i.Item3.ToString("N2"),
                (i.Item3 * 100 / total).ToString("N2") + " %",
                (Guid?)i.Id))
            .ToArray();

    private TableRow[] GetExpensesListByGroup(List<(Expense item, Category category, Group? group)> expenses, double total)
    {
        var result = expenses
            .Where(e => e.group is not null)
            .GroupBy(e => e.group)
            .OrderBy(g => g.Key?.Name)
            .Select(g => (
                g.Key?.Name ?? "[empty]",
                g.Key?.Id,
                g.Select(e => e.item.Amounth).DefaultIfEmpty().Sum()))
            .Where(i => i.Item3 > 0)
            .Select(i => new TableRow(
                i.Item1,
                i.Item3.ToString("N2"),
                (i.Item3 * 100 / total).ToString("N2") + " %",
                i.Id))
            .ToArray();

        var withoutGroup = expenses
            .Where(e => e.group is null)
            .Select(e => e.item.Amounth)
            .DefaultIfEmpty()
            .Sum();

        if (withoutGroup > 0)
        {
            result = result.Append(new TableRow("Без группы", withoutGroup.ToString("N2"),
                (withoutGroup * 100 / total).ToString("N2") + " %", null)).ToArray();
        }

        return result;
    }

    public async Task<PeriodExpensesTableData> GetExpensesAsTable(ExpensesGroupingType groupingType,
        DateTimeOffset fromDate, DateTimeOffset toDate, string? filter, CancellationToken token)
    {
        var periodExpenses = await _expensesService.GetExpensesWithRelatedData(startDate: fromDate, endDate: toDate, filter, token);

        var total = periodExpenses
            .Select(e => e.item.Amounth)
            .DefaultIfEmpty()
            .Sum();

        var rows = groupingType switch
        {
            ExpensesGroupingType.None => GetExpensesList(periodExpenses),
            ExpensesGroupingType.ByDate => GetExpensesListByDate(periodExpenses, total),
            ExpensesGroupingType.ByCategory => GetExpensesListByCategory(periodExpenses, total),
            ExpensesGroupingType.ByGroup => GetExpensesListByGroup(periodExpenses, total),
            _ => throw new ArgumentException("Unknown grouping type")
        };

        return new PeriodExpensesTableData(groupingType, rows, total);
    }

    public async Task<List<Expense>> GetFilteredExpenses(ExpensesGroupingType groupingType,
        DateTimeOffset fromDate, DateTimeOffset toDate, Guid? filterBy, CancellationToken token)
    {
        if (groupingType == ExpensesGroupingType.ByCategory && filterBy is null)
        {
            throw new ArgumentException("Category Id should not be null");
        }

        if (groupingType == ExpensesGroupingType.ByDate && fromDate != toDate)
        {
            throw new ArgumentException("FromDate and ToDate should be the same");
        }

        var periodExpenses = await _expensesService.GetExpensesWithRelatedData(startDate: fromDate, endDate: toDate, null, token);

        if (groupingType == ExpensesGroupingType.None)
        {
            return periodExpenses.Select(e => e.item).ToList();
        }

        if (groupingType == ExpensesGroupingType.ByDate)
        {
            return periodExpenses.Where(e => e.item.Date.Date == fromDate.Date).Select(e => e.item).ToList();
        }

        if (groupingType == ExpensesGroupingType.ByCategory)
        {
            return periodExpenses.Where(e => e.category.Id == filterBy).Select(e => e.item).ToList();
        }

        if (groupingType == ExpensesGroupingType.ByGroup)
        {
            if (filterBy is null)
            {
                return periodExpenses.Where(e => e.group is null).Select(e => e.item).ToList();
            }

            return periodExpenses.Where(e => e.group is not null && e.group.Id == filterBy).Select(e => e.item).ToList();
        }

        throw new Exception("Unknown grouping type");
    }
}

internal sealed record TableRow(string Value1, string Value2, string Value3, Guid? RelatedId);

internal sealed class PeriodExpensesTableData
{
    private readonly ExpensesGroupingType _expensesGroupingType;
    private readonly TableRow[] _rows;
    private readonly double _total;

    public PeriodExpensesTableData(ExpensesGroupingType expensesGroupingType, TableRow[] rows, double total)
    {
        _expensesGroupingType = expensesGroupingType;
        _rows = rows;
        _total = total;
    }

    public ExpensesGroupingType ExpensesGroupingType => _expensesGroupingType;

    public ReadOnlyCollection<TableRow> Rows => new(_rows);

    public string Total => _total.ToString("N2");

    public string[] Headers => ExpensesGroupingType switch
    {
        ExpensesGroupingType.None => new string[] { "Дата", "Описание", "Потрачено" },
        ExpensesGroupingType.ByDate => new string[] { "Дата", "Потрачено", "%" },
        ExpensesGroupingType.ByCategory => new string[] { "Категория", "Потрачено", "%" },
        ExpensesGroupingType.ByGroup => new string[] { "Группа", "Потрачено", "%" },
        _ => throw new ArgumentException("Unknown grouping type")
    };

    public string[] ChartLabels =>
        ExpensesGroupingType == ExpensesGroupingType.ByGroup || ExpensesGroupingType == ExpensesGroupingType.ByCategory
            ? Rows.OrderBy(x => x.Value1).Select(x => x.Value1).ToArray()
            : Array.Empty<string>();

    public double[] ChartData =>
         ExpensesGroupingType == ExpensesGroupingType.ByGroup || ExpensesGroupingType == ExpensesGroupingType.ByCategory
            ? Rows.OrderBy(x => x.Value1).Select(x => double.Parse(x.Value2)).ToArray()
            : Array.Empty<double>();

    public bool IsEmpty => _rows.Length == 0;
}
