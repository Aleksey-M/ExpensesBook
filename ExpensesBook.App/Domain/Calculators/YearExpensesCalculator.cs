using System.Collections.ObjectModel;
using System.Globalization;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Services;
using MudBlazor;

namespace ExpensesBook.Domain.Calculators;

public sealed class YearExpensesCalculator
{
    private readonly IExpensesService _expensesSvc;
    private readonly ICategoriesService _categoriesSvc;
    private readonly IGroupsService _groupsSvc;

    public YearExpensesCalculator(IExpensesService expensesSvc, ICategoriesService categoriesSvc, IGroupsService groupsSvc)
    {
        _expensesSvc = expensesSvc;
        _categoriesSvc = categoriesSvc;
        _groupsSvc = groupsSvc;
    }

    public async Task<List<int>> GetYears(CancellationToken token)
    {
        var months = await _expensesSvc.GetExpensesMonths(token);
        return months.Select(m => m.year).Distinct().OrderBy(y => y).ToList();
    }

    public async Task<YearPivotTable?> GetYearExpenses(int year, ExpensesGroupingType groupingType, CancellationToken token)
    {
        if (year == 0) return null;

        var firstDay = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var lastDay = new DateTimeOffset(year, 12, 31, 0, 0, 0, TimeSpan.Zero);
        var expenses = await _expensesSvc.GetExpenses(startDate: firstDay, endDate: lastDay, filter: null, token);

        if (expenses.Count == 0) return null;

        switch (groupingType)
        {
            case ExpensesGroupingType.ByCategory:

                var categories = await _categoriesSvc.GetCategories(token);
                if (categories.Count == 0) return null;

                Func<IEnumerable<Expense>, IEnumerable<(string, double)>> groupingLogicC = items => categories
                    .OrderBy(c => c.Sort)
                    .GroupJoin(
                        items,
                        c => c.Id,
                        i => i.CategoryId,
                        (category, items) => (category.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()));

                return new YearPivotTable(year: year, groupingParamName: "Категория", expenses, groupingLogicC);

            case ExpensesGroupingType.ByGroup:

                var groups = await _groupsSvc.GetGroups(token);
                if (groups.Count == 0) return null;

                Func<IEnumerable<Expense>, IEnumerable<(string, double)>> groupingLogicG = items => groups
                    .Concat(new List<Group> { new Group { Id = default, Name = "Без группы", Sort = 1000 } })
                    .OrderBy(g => g.Sort)
                    .GroupJoin(
                        items,
                        g => g.Id,
                        i => i.GroupId ?? default,
                        (group, items) => (group.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()));

                return new YearPivotTable(year: year, groupingParamName: "Группа", expenses, groupingLogicG);

            default: throw new Exception("Not applicable grouping for year pivot table");
        }
    }
}


public sealed record ValuePercent(string Value, string Percent, double NumValue);

public sealed record YearPivotTableRow(string RowName, ReadOnlyCollection<ValuePercent> CellsValues);

public sealed record MonthColumnTotal(int Month, string MonthName, string Total, string YearPercent, double NumTotal);


public sealed class YearPivotTable
{
    private readonly double _totalNum;

    public ReadOnlyCollection<YearPivotTableRow> Rows { get; }

    public ReadOnlyCollection<MonthColumnTotal> MonthsTotals { get; }

    public string GroupingParameterName { get; }

    public string Total => _totalNum.ToString("N2");

    public YearPivotTable(int year, string groupingParamName, IEnumerable<Expense> allExpenses,
        Func<IEnumerable<Expense>, IEnumerable<(string, double)>> groupingLogic)
    {
        GroupingParameterName = groupingParamName;

        var yearExpenses = allExpenses.Where(e => e.Date.Year == year).ToList();
        _totalNum = yearExpenses.Select(e => e.Amounth).DefaultIfEmpty().Sum();

        var yearMonths = yearExpenses
            .Select(e => e.Date.Month)
            .DefaultIfEmpty()
            .Distinct()
            .OrderBy(m => m)
            .ToList();

        var monthsTotals = yearExpenses
            .GroupBy(e => e.Date.Month)
            .Select(g => (month: g.Key, total: g.Select(e => e.Amounth).Sum()))
            .OrderBy(x => x.month)
            .ToList();

        var monthsTotalsList = monthsTotals
            .Zip(yearMonths
                .OrderBy(x => x)
                .Select(m => new DateTimeOffset(year, m, 1, 0, 0, 0, TimeSpan.FromSeconds(0))
                    .ToString("MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))))
            .Select(pair => new MonthColumnTotal(
                Month: pair.First.month,
                MonthName: pair.Second,
                Total: pair.First.total.ToString("N2"),
                YearPercent: (pair.First.total / _totalNum).ToString("P2"),
                NumTotal: pair.First.total))
            .ToList();

        MonthsTotals = new ReadOnlyCollection<MonthColumnTotal>(monthsTotalsList);


        var monthsCalculated = new Dictionary<int, List<(string name, ValuePercent value)>>();

        foreach (int m in yearMonths)
        {
            var total = monthsTotals.Single(t => t.month == m).total;

            monthsCalculated[m] = groupingLogic(yearExpenses.Where(e => e.Date.Month == m))
            .Select(x => x.Item2 > 0
                    ? (x.Item1, new ValuePercent(x.Item2.ToString("N2"), (x.Item2 / total).ToString("P2"), x.Item2))
                    : (x.Item1, new ValuePercent("0.00", "0%", 0)))
                .ToList();
        }

        var allRowsNames = monthsCalculated
            .SelectMany(kv => kv.Value)
            .Select(v => v.name)
            .Distinct()
            .ToList();


        var rows = new List<YearPivotTableRow>();

        foreach (var n in allRowsNames)
        {
            var cellsValues = new List<ValuePercent>();

            foreach (int m in yearMonths)
            {
                bool valueExists = monthsCalculated.ContainsKey(m)
                    && monthsCalculated[m].SingleOrDefault(v => v.name == n) != default;

                if (valueExists)
                {
                    cellsValues.Add(monthsCalculated[m].SingleOrDefault(v => v.name == n).value);
                }
                else
                {
                    cellsValues.Add(new ValuePercent("0.00", "0%", 0));
                }
            }

            if (cellsValues.Any(x => x.NumValue > 0))
            {
                rows.Add(new YearPivotTableRow(n, new ReadOnlyCollection<ValuePercent>(cellsValues)));
            }
        }

        rows.Add(new YearPivotTableRow("За месяц", new ReadOnlyCollection<ValuePercent>(
            MonthsTotals.Select(x => new ValuePercent(x.Total, x.YearPercent, x.NumTotal)).ToList())));

        Rows = new ReadOnlyCollection<YearPivotTableRow>(rows);
    }

    public string[] XAxisLabels => MonthsTotals.Select(x => x.MonthName).ToArray();

    public static int GetYAxisTicks(HashSet<YearPivotTableRow> selectedRows)
    {
        var max = selectedRows
            .SelectMany(x => x.CellsValues)
            .Max(x => x.NumValue);

        return max switch
        {
            0 => 10,
            < 1000 => 200,
            < 2000 => 250,
            < 3000 => 300,
            < 5000 => 500,
            < 10_000 => 1000,
            _ => 20_000
        };
    }

    public static List<ChartSeries> GetChartSeries(HashSet<YearPivotTableRow> selectedRows) => selectedRows
        .Select(x => new ChartSeries
        {
            Name = x.RowName,
            Data = x.CellsValues.Select(y => y.NumValue).ToArray()
        })
        .ToList();
}
