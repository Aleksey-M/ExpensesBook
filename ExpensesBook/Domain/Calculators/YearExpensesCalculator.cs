using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesBook.Domain.Calculators
{
    internal class YearExpensesCalculator
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

        public async ValueTask<List<int>> GetYears()
        {
            var months = await _expensesSvc.GetExpensesMonths();
            return months.Select(m => m.year).Distinct().OrderBy(y => y).ToList();
        }

        public async ValueTask<YearPivotTable?> GetYearExpenses(int year, ExpensesGroupingType groupingType)
        {
            if (year == 0) return null;

            var firstDay = new DateTimeOffset(year, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var lastDay = new DateTimeOffset(year, 12, 31, 0, 0, 0, TimeSpan.Zero);
            var expenses = await _expensesSvc.GetExpenses(startDate: firstDay, endDate: lastDay, filter: null);

            if (expenses.Count == 0) return null;

            switch (groupingType)
            {
                case ExpensesGroupingType.ByCategory:

                    var categories = await _categoriesSvc.GetCategories();
                    if (categories.Count == 0) return null; // 

                    Func<IEnumerable<Expense>, IEnumerable<(string, double)>> groupingLogicC = items => categories
                        .OrderBy(c => c.Sort)
                        .GroupJoin(
                            items,
                            c => c.Id,
                            i => i.CategoryId,
                            (category, items) => (category.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()));

                    return new YearPivotTable(year: year, groupingParamName: "Категория", expenses, groupingLogicC);

                case ExpensesGroupingType.ByGroup:

                    var groups = await _groupsSvc.GetGroups();
                    if (groups.Count == 0) return null;

                    Func<IEnumerable<Expense>, IEnumerable<(string, double)>> groupingLogicG = items => groups.Concat(new List<Group> { new Group { Id = default, Name = "Без группы", Sort = 1000 } })
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

    internal class YearPivotTable
    {
        public List<(string rowName, List<(string value, string percent)>)> Rows { get; }
        public List<(int month, string total, string percent)> MonthsTotals { get; }
        public List<string> MonthsNames { get; }
        public double TotalNum { get; }
        public string Total => TotalNum.ToString("N2");
        public string GroupingParameterName { get; }

        public YearPivotTable(int year, string groupingParamName, IEnumerable<Expense> allExpenses, Func<IEnumerable<Expense>, IEnumerable<(string, double)>> groupingLogic)
        {
            GroupingParameterName = groupingParamName;

            var yearExpenses = allExpenses.Where(e => e.Date.Year == year).ToList();
            TotalNum = yearExpenses.Select(e => e.Amounth).DefaultIfEmpty().Sum();

            var yearMonths = yearExpenses
                .Select(e => e.Date.Month)
                .DefaultIfEmpty()
                .Distinct()
                .OrderBy(m => m).ToList();

            MonthsNames = yearMonths.Select(m => new DateTimeOffset(year, m, 1, 0, 0, 0, TimeSpan.FromSeconds(0)).ToString("MMMM", System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"))).ToList();

            var monthsTotals = yearExpenses.GroupBy(e => e.Date.Month).Select(g => (g.Key, g.Select(e => e.Amounth).Sum())).ToList();
            MonthsTotals = monthsTotals.Select(mt => (mt.Key, mt.Item2.ToString("N2"), (mt.Item2 / TotalNum).ToString("P2"))).ToList();

            var monthsCalculated = new Dictionary<int, List<(string name, string value, string percent)>>();

            foreach (int m in yearMonths)
            {
                var total = monthsTotals.Single(t => t.Key == m).Item2;

                monthsCalculated[m] = groupingLogic(yearExpenses.Where(e => e.Date.Month == m))
                    .Where(i => i.Item2 > 0)
                    .Select(i => (i.Item1, i.Item2.ToString("N2"), (i.Item2 / total).ToString("P2")))
                    .ToList();
            }

            var allRowsNames = monthsCalculated.SelectMany(kv => kv.Value).Select(v => v.name).Distinct().ToList();

            Rows = new List<(string rowName, List<(string value, string percent)>)>();

            foreach (var n in allRowsNames)
            {
                var row = new List<(string value, string percent)>();
                foreach (int m in yearMonths)
                {
                    if (monthsCalculated.ContainsKey(m))
                    {
                        var val = monthsCalculated[m].SingleOrDefault(v => v.name == n);
                        if (val == default)
                        {
                            row.Add(("0.00", "0%"));
                        }
                        else
                        {
                            row.Add((val.value, val.percent));
                        }
                    }
                    else
                    {
                        row.Add(("0.00", "0%"));
                    }
                }

                Rows.Add((rowName: n, row));
            }
        }
    }
}
