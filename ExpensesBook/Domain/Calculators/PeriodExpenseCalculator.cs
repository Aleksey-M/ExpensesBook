using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExpensesBook.Domain.Calculators
{
#pragma warning disable CA1822 // Mark members as static
    internal class PeriodExpenseCalculator
    {
        private readonly IExpensesService _expensesService;

        public PeriodExpenseCalculator(IExpensesService expensesService)
        {
            _expensesService = expensesService;
        }

        public (string, string, string) GetTableHeaders(ExpensesGroupingType groupingType) => groupingType switch
        {
            ExpensesGroupingType.None => ("Дата", "Описание", "Потрачено"),
            ExpensesGroupingType.ByDate => ("Дата", "Потрачено", "%"),
            ExpensesGroupingType.ByCategory => ("Категория", "Потрачено", "%"),
            ExpensesGroupingType.ByGroup => ("Группа", "Потрачено", "%"),
            _ => throw new ArgumentException("Unknown grouping type")
        };


        public List<(string date, string description, string amount, Guid? expenseId)> GetExpensesList(List<(Expense item, Category category, Group? group)> expenses) => expenses
            .Select(exp =>
                (exp.item.Date.ToString("yyyy.MM.dd"),
                 exp.item.Description,
                 exp.item.Amounth.ToString("N2"),
                 (Guid?)exp.item.Id)).ToList();

        public List<(string date, string amount, string percent, Guid? id)> GetExpensesListByDate(List<(Expense item, Category category, Group? group)> expenses, double total) => expenses
            .GroupBy(e => e.item.Date.Date)
            .Select(g => (g.Key.ToString("yyyy.MM.dd"), g.Select(e => e.item.Amounth).DefaultIfEmpty().Sum()))
            .Where(i => i.Item2 > 0)
            .Select(i => (i.Item1, i.Item2.ToString("N2"), (i.Item2 * 100 / total).ToString("N2") + " %", (Guid?)null))
            .ToList();

        public List<(string category, string amount, string percent, Guid? categoryId)> GetExpensesListByCategory(List<(Expense item, Category category, Group? group)> expenses, double total) => expenses
            .GroupBy(exp => exp.category)
            .OrderBy(g => g.Key.Name)
            .Select(g => (g.Key.Name, g.Key.Id, g.Select(e => e.item.Amounth).DefaultIfEmpty().Sum()))
            .Where(i => i.Item3 > 0)
            .Select(i => (i.Name, i.Item3.ToString("N2"), (i.Item3 * 100 / total).ToString("N2") + " %", (Guid?)i.Id))
            .ToList();

        public List<(string group, string amount, string percent, Guid? groupId)> GetExpensesListByGroup(List<(Expense item, Category category, Group? group)> expenses, double total) 
        { 
            var result = expenses
                .Where(e => e.group is not null)
                .GroupBy(e => e.group)
                .OrderBy(g => g.Key?.Name)
                .Select(g => (g.Key?.Name ?? "[empty]", g.Key?.Id, g.Select(e => e.item.Amounth).DefaultIfEmpty().Sum()))
                .Where(i => i.Item3 > 0)
                .Select(i => (i.Item1, i.Item3.ToString("N2"), (i.Item3 * 100 / total).ToString("N2") + " %", (Guid?)i.Id))
                .ToList();

            var withoutGroup = expenses.Where(e => e.group is null).Select(e => e.item.Amounth).DefaultIfEmpty().Sum();

            if(withoutGroup > 0)
            {
                result.Add(("Без группы", withoutGroup.ToString("N2"), (withoutGroup * 100 / total).ToString("N2") + " %", (Guid?)null));
            }

            return result;
        }

        public async ValueTask<(List<(string, string, string, Guid?)> rows, string total)> GetExpensesAsTable(ExpensesGroupingType groupingType, DateTimeOffset fromDate, DateTimeOffset toDate, string? filter)
        {
            var periodExpenses = await _expensesService.GetExpensesWithRelatedData(startDate: fromDate, endDate: toDate, filter);

            var total = periodExpenses.Select(e => e.item.Amounth).DefaultIfEmpty().Sum();

            var rows = groupingType switch
            {
                ExpensesGroupingType.None => GetExpensesList(periodExpenses),
                ExpensesGroupingType.ByDate => GetExpensesListByDate(periodExpenses, total),
                ExpensesGroupingType.ByCategory => GetExpensesListByCategory(periodExpenses, total),
                ExpensesGroupingType.ByGroup => GetExpensesListByGroup(periodExpenses, total),
                _ => throw new ArgumentException("Unknown grouping type")
            };

            return (rows, total.ToString("N2"));
        }

        public async ValueTask<List<Expense>> GetFilteredExpenses(ExpensesGroupingType groupingType, DateTimeOffset fromDate, DateTimeOffset toDate, Guid? filterBy)
        {
            if (groupingType == ExpensesGroupingType.ByCategory && filterBy is null) throw new ArgumentException("Category Id should not be null");
            if (groupingType == ExpensesGroupingType.ByDate && fromDate != toDate) throw new ArgumentException("FromDate and ToDate should be the same");

            var periodExpenses = await _expensesService.GetExpensesWithRelatedData(startDate: fromDate, endDate: toDate, null);

            if (groupingType == ExpensesGroupingType.None) 
            {
                return periodExpenses.Select(e => e.item).ToList();
            } 

            if(groupingType == ExpensesGroupingType.ByDate)
            {
                return periodExpenses.Where(e => e.item.Date.Date == fromDate.Date).Select(e => e.item).ToList();
            }

            if(groupingType == ExpensesGroupingType.ByCategory)
            {
                return periodExpenses.Where(e => e.category.Id == filterBy).Select(e => e.item).ToList();
            }

            if (groupingType == ExpensesGroupingType.ByGroup)
            {
                if(filterBy is null)
                {
                    return periodExpenses.Where(e => e.group is null).Select(e => e.item).ToList();
                }
                
                return periodExpenses.Where(e => e.group is not null && e.group.Id == filterBy).Select(e => e.item).ToList();
            }

            throw new Exception("Unknown grouping type");
        }
    }
}
