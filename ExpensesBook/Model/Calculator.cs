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
            if(withoutGroupAmount > 0)
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
                    datesRange: $@"{l.limit.StartIncluded.ToString("yyyy.MM.dd")} - {l.limit.EndExcluded.ToString("yyyy.MM.dd")}",
                    description: l.limit.Description,
                    limit: l.limit.LimitAmounth.ToString("N2"),
                    spent: l.currentAmount.ToString("N2"),
                    left: (l.Item3 > 0 ? l.Item3 : 0).ToString("N2"),
                    deficite: (l.Item3 < 0 ? -l.Item3 : 0).ToString("N2")
                    ))
                .ToList();

            return res;
        }
    }
}
