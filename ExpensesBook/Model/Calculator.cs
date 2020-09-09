using ExpensesBook.Pages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpensesBook.Model
{
    internal enum ViewType { Default, ByDate, ByGroup, ByCategory, Limits }

    internal class Calculator
    {
        private readonly ExpensesData _data;
        public Calculator(ExpensesData data)
        {
            _data = data;
        }

        public List<ExpenseItem> GetRange(DateTimeOffset start, DateTimeOffset end) => _data.Expenses.Where(e => e.Date.Date >= start && e.Date.Date <= end).OrderBy(e => e.Date).ToList();
        public double GetTotal(DateTimeOffset start, DateTimeOffset end) => GetRange(start, end).Select(e => e.Amounth).DefaultIfEmpty().Sum();
        public List<(string date, string value, string percent)> GetGroupedByDate(DateTimeOffset start, DateTimeOffset end)
        {
            var total = GetTotal(start, end);

            return GetRange(start, end)
                .GroupBy(e => e.Date.Date)
                .Select(g => (g.Key.ToString("yyyy.MM.dd"), g.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
                .Where(i => i.Item2 > 0)
                .Select(i => (i.Item1, i.Item2.ToString("N2"), (i.Item2 * 100 / total).ToString("N2") + " %"))
                .ToList();
        }

        public List<(string group, string value, string percent)> GetGroupedByGroup(DateTimeOffset start, DateTimeOffset end)
        {
            var total = GetTotal(start, end);
            var items = GetRange(start, end);

            var withGroups = _data.Groups
                .OrderBy(g => g.Name)
                .GroupJoin(items, g => g.Id, i => i.GroupId, (group, items) => (group.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
                .Where(i => i.Item2 > 0)
                .Select(i => (i.Name, i.Item2.ToString("N2"), (i.Item2 * 100 / total).ToString("N2") + " %"))
                .ToList();

            var withoutGroupAmount = items.Where(i => i.GroupId == default).Select(e => e.Amounth).DefaultIfEmpty().Sum();
            if (withoutGroupAmount > 0)
            {
                withGroups.Add(("Без группы", withoutGroupAmount.ToString("N2"), (withoutGroupAmount * 100 / total).ToString("N2") + " %"));
            }

            return withGroups;
        }

        public List<(string category, string value, string percent)> GetGroupedByCategory(DateTimeOffset start, DateTimeOffset end)
        {
            var total = GetTotal(start, end);
            var items = GetRange(start, end);

            return _data.Categories
                .OrderBy(c => c.Name)
                .GroupJoin(items, c => c.Id, i => i.CategoryId, (category, items) => (category.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
                .Where(i => i.Item2 > 0)
                .Select(i => (i.Name, i.Item2.ToString("N2"), (i.Item2 * 100 / total).ToString("N2") + " %"))
                .ToList();
        }

        public List<(string datesRange, string description, string limit, string spent, string left, string deficit)> GetActualLimits(DateTimeOffset start, DateTimeOffset end)
        {
            var total = GetTotal(start, end);
            var items = GetRange(start, end)
                .GroupBy(e => e.Date.Date)
                .Select(g => (g.Key, g.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
                .Where(i => i.Item2 > 0)
                .ToList();

            var limits = _data.Limits.Where(l => items.Any(i => i.Key >= l.StartIncluded && i.Key < l.EndExcluded)).ToList();
            var res = limits.Select(l => (
                    limit: l,
                    currentAmount: items.Where(e => e.Key >= l.StartIncluded && e.Key < l.EndExcluded).Select(e => e.Item2).DefaultIfEmpty().Sum()))
                .Select(l => (l.limit, l.currentAmount, l.limit.LimitAmounth - l.currentAmount))
                .Where(l => l.currentAmount > 0)
                .OrderBy(l => l.limit.EndExcluded)
                .Select(l => (
                    datesRange: $@"{l.limit.StartIncluded:yyyy.MM.dd} - {l.limit.EndExcluded:yyyy.MM.dd}",
                    description: l.limit.Description,
                    limit: l.limit.LimitAmounth.ToString("N2"),
                    spent: l.currentAmount.ToString("N2"),
                    left: (l.Item3 > 0 ? l.Item3 : 0).ToString("N2"),
                    deficite: (l.Item3 < 0 ? -l.Item3 : 0).ToString("N2")
                    ))
                .ToList();

            return res;
        }

        public List<(ExpenseItem item, string category, string group)> GetMonthExpenses(int year, int month) => _data.Expenses
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .Join(_data.Categories,
               e => e.CategoryId,
               c => c.Id,
                (e, c) => (e, c.Name, _data.Groups.SingleOrDefault(g => g.Id == (e.GroupId ?? Guid.Empty))?.Name))
            .OrderBy(i => i.e.Date)
            .ToList();

        public List<(int year, int month, string monthName)> GetMonths() => _data.Expenses
            .Select(e => (e.Date.Year, e.Date.Month, e.Date.ToString("MMMM", System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"))))
            .Distinct()
            .OrderBy(ym => ym.Year).ThenBy(ym => ym.Month)
            .ToList();

        private List<CalculatedLimit> GetCalculatedLimits(IEnumerable<Limit> limits, DateTimeOffset currentDate) => limits
            .Select(l => new CalculatedLimit(l, _data.Expenses, currentDate))
            .OrderBy(l => l.IsActual)
            .ThenBy(l => l.EndExcluded)
            .ToList();

        public List<CalculatedLimit> GetCalculatedLimits(DateTimeOffset currentDate) => GetCalculatedLimits(_data.Limits, currentDate);

        public List<CalculatedLimit> GetYearLimits(int year, DateTimeOffset currentDate) =>
            GetCalculatedLimits(_data.Limits.Where(l => l.StartIncluded.Date.Year == year || l.EndExcluded.Date.Year == year), currentDate);

        public List<int> GetAllYears() => _data.Expenses.Select(e => e.Date.Date.Year).Distinct().ToList();

        public YearPivotTable GetYearExpensesByCategories(int year) => new YearPivotTable(year, _data.Expenses,
            items => _data.Categories
                .OrderBy(c => c.Sort)
                .GroupJoin(
                    items,
                    c => c.Id,
                    i => i.CategoryId,
                    (category, items) => (category.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
            );

        public YearPivotTable GetYearExpensesByGroup(int year) => new YearPivotTable(year, _data.Expenses,
            items => _data.Groups.Concat(new List<ExpensesGroup> { new ExpensesGroup { Id = default, Name = "Без группы", Sort = 1000 } })
                .OrderBy(g => g.Sort)
                .GroupJoin(
                    items, 
                    g => g.Id, 
                    i => i.GroupId ?? default, 
                    (group, items) => (group.Name, items.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
            );

        public SavingsCalculated GetSavingsForMonths() => new SavingsCalculated(_data.Savings, _data.Expenses);
    }

    internal class SavingsCalculated
    {
        public List<(SavingsItem saving, string expenses, string totalSaving, string color)> Savings { get; }
        public string TotalExpenses { get; }
        public string TotalIncome { get; }
        public string TotalSavings { get; }
        public string TotalSavingsStyle { get; }

        public SavingsCalculated(IEnumerable<SavingsItem> allSavings, IEnumerable<ExpenseItem> allExpenses)
        {
            var actualExpenses = allExpenses
                .GroupBy(e => (e.Date.Year, e.Date.Month))
                .Where(g => allSavings.Any(s => s.Year == g.Key.Year && s.Month == g.Key.Month))
                .Select(g => (g.Key.Year, g.Key.Month, total: g.Select(e => e.Amounth).DefaultIfEmpty().Sum()))
                .ToList();

            double expenses = actualExpenses.Select(e => e.total).DefaultIfEmpty().Sum();
            double income = allSavings.Select(s => s.Income).DefaultIfEmpty().Sum();
            double savings =  income - expenses;

            TotalExpenses = expenses.ToString("N2");
            TotalSavings = savings.ToString("N2");
            TotalIncome = income.ToString("N2");
            TotalSavingsStyle = savings >= 0 ? "color:forestgreen" : "color:red";

            Savings = new List<(SavingsItem, string, string, string)>();
            foreach(var s in allSavings)
            {
                var monthExpenses = actualExpenses.SingleOrDefault(e => e.Year == s.Year && e.Month == s.Month);

                double saving = s.Income - monthExpenses.total;
                string color = saving >= 0 ? "color:forestgreen" : "color:red";

                Savings.Add((s, monthExpenses.total.ToString("N2"), saving.ToString("N2"), color));
            }

            Savings = Savings.OrderBy(s => s.saving.Year).ThenBy(s => s.saving.Month).ToList();
        }
    }

    internal class CalculatedLimit
    {
        public Guid LimitId { get; }
        public string Description { get; }
        public DateTimeOffset StartIncluded { get; }
        public DateTimeOffset EndExcluded { get; }
        public double LimitAmounth { get; }
        public string LimitAmounthStr => LimitAmounth.ToString("N2");
        public bool IsActual { get; }
        private List<ExpenseItem> ExpensesFromRange { get; }
        public string DatesRange { get; }
        public double Spent { get; }
        public double Left { get; }
        public double Deficite { get; }
        public string SpentStr => Spent.ToString("N2");
        public string LeftStr => Left.ToString("N2");
        public string DeficiteStr => Deficite.ToString("N2");

        public string RowColorStyle => (Spent > LimitAmounth, IsActual) switch
        {
            (true, false) => "color: indianred",
            (true, true) => "color: red",
            (false, true) => "color: blue",
            (false, false) => ""
        };


        public CalculatedLimit(Limit limit, IEnumerable<ExpenseItem> allExpenses, DateTimeOffset currentDate)
        {
            LimitId = limit.Id;
            Description = limit.Description;
            StartIncluded = limit.StartIncluded.Date;
            EndExcluded = limit.EndExcluded.Date;
            LimitAmounth = limit.LimitAmounth;

            IsActual = StartIncluded <= currentDate.Date && EndExcluded > currentDate.Date;
            DatesRange = $@"{StartIncluded:yyyy.MM.dd} - {EndExcluded:yyyy.MM.dd}";
            ExpensesFromRange = allExpenses.Where(e => e.Date.Date >= StartIncluded && e.Date.Date < EndExcluded).ToList();

            Spent = ExpensesFromRange.Select(e => e.Amounth).DefaultIfEmpty().Sum();

            Left = LimitAmounth - Spent;
            if (Left < 0)
            {
                Deficite = -Left;
                Left = 0;
            }
            else
            {
                Deficite = 0;
            }
        }
    }

    internal class YearPivotTable
    {
        public List<(string rowName, List<(string value, string percent)>)> Rows { get; }
        public List<(int month, string total, string percent)> MonthsTotals { get; }
        public List<string> MonthsNames { get; }
        public double Total { get; }
        public string TotalStr => Total.ToString("N2");
        public ViewType ViewType { get; set; }
        public YearPivotTable(int year, IEnumerable<ExpenseItem> allExpenses, Func<IEnumerable<ExpenseItem>, IEnumerable<(string, double)>> groupingLogic)
        {
            var yearExpenses = allExpenses.Where(e => e.Date.Year == year).ToList();
            Total = yearExpenses.Select(e => e.Amounth).DefaultIfEmpty().Sum();

            var yearMonths = yearExpenses
                .Select(e => e.Date.Month)
                .DefaultIfEmpty()
                .Distinct()
                .OrderBy(m => m).ToList();

            MonthsNames = yearMonths.Select(m => new DateTimeOffset(year, m, 1, 0, 0, 0, TimeSpan.FromSeconds(0)).ToString("MMMM", System.Globalization.CultureInfo.CreateSpecificCulture("ru-RU"))).ToList();

            var monthsTotals = yearExpenses.GroupBy(e => e.Date.Month).Select(g => (g.Key, g.Select(e => e.Amounth).Sum())).ToList();
            MonthsTotals = monthsTotals.Select(mt => (mt.Key, mt.Item2.ToString("N2"), (mt.Item2 * 100 / Total).ToString("N2") + "%")).ToList();

            var monthsCalculated = new Dictionary<int, List<(string name, string value, string percent)>>();

            foreach(int m in yearMonths)
            {
                var total = monthsTotals.Single(t => t.Key == m).Item2;

                monthsCalculated[m] = groupingLogic(yearExpenses.Where(e => e.Date.Month == m))
                    .Where(i => i.Item2 > 0)
                    .Select(i => (i.Item1, i.Item2.ToString("N2"), (i.Item2 * 100 / total).ToString("N2") + "%"))
                    .ToList();
            }

            var allRowsNames = monthsCalculated.SelectMany(kv => kv.Value).Select(v => v.name).Distinct().ToList();

            Rows = new List<(string rowName, List<(string value, string percent)>)>();

            foreach(var n in allRowsNames)
            {
                var row = new List<(string value, string percent)>();
                foreach(int m in yearMonths)
                {
                    if (monthsCalculated.ContainsKey(m))
                    {
                        var val = monthsCalculated[m].SingleOrDefault(v => v.name == n);
                        if(val == default)
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
