using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

internal interface IExpensesService
{
    ValueTask<Expense> AddExpense(DateTimeOffset date, double amounth, string description, Guid categoryId, Guid? groupId);

    ValueTask<List<Expense>> GetExpenses(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter);

    ValueTask UpdateExpense(Guid expenseId, DateTimeOffset? date, DateTimeOffset oldDate, double? amounth, string? description, Guid? categoryId, Guid? groupId);

    ValueTask DeleteExpense(Guid expenseId, DateTimeOffset date);

    ValueTask<List<(int year, int month, string monthName)>> GetExpensesMonths();

    ValueTask<List<(Expense item, Category category, Group? group)>> GetExpensesWithRelatedData(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter);
}

internal class ExpensesService : IExpensesService
{
    private readonly IExpensesRepository _expensesRepo;
    private readonly ICategoriesRepository _categoriesRepo;
    private readonly IGroupsRepository _groupsRepo;

    public ExpensesService(IExpensesRepository expensesRepo, ICategoriesRepository categoriesRepo, IGroupsRepository groupsRepo)
    {
        _expensesRepo = expensesRepo;
        _categoriesRepo = categoriesRepo;
        _groupsRepo = groupsRepo;
    }

    public async ValueTask<Expense> AddExpense(DateTimeOffset date,
        double amounth, string description, Guid categoryId, Guid? groupId)
    {
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Date = date,
            Amounth = amounth,
            Description = description,
            CategoryId = categoryId,
            GroupId = groupId
        };

        await _expensesRepo.AddExpense(expense);

        return expense;
    }

    public async ValueTask DeleteExpense(Guid expenseId, DateTimeOffset date) =>
        await _expensesRepo.DeleteExpense(expenseId, date);

    public async ValueTask<List<Expense>> GetExpenses(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter)
    {
        filter ??= "";

        Func<string, bool> descriptionFilter = filter == ""
            ? _ => true
            : desc => desc.Contains(filter, StringComparison.OrdinalIgnoreCase);

        var fullList = await _expensesRepo.GetExpenses(startDate, endDate);

        return fullList
            .Where(exp => descriptionFilter(exp.Description))
            .OrderBy(exp => exp.Date)
            .ToList();
    }

    public async ValueTask UpdateExpense(Guid expenseId, DateTimeOffset? date,
        DateTimeOffset oldDate, double? amounth, string? description, Guid? categoryId, Guid? groupId)
    {
        if (date is null && amounth is null && description is null && categoryId is null) return;

        var expense = await _expensesRepo.GetExpense(expenseId, oldDate);

        var updatedExpense = new Expense
        {
            Id = expense.Id,
            Amounth = amounth ?? expense.Amounth,
            Date = date ?? expense.Date,
            Description = string.IsNullOrWhiteSpace(description) ? expense.Description : description,
            CategoryId = categoryId ?? expense.CategoryId,
            GroupId = groupId
        };

        await _expensesRepo.UpdateExpense(updatedExpense, oldDate);
    }

    public async ValueTask<List<(int year, int month, string monthName)>> GetExpensesMonths() =>
        (await _expensesRepo.GetMonths())
            .Select(e => (
                e.year,
                e.month,
                new DateTime(e.year, e.month, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))))
            .OrderBy(ym => ym.year)
            .ThenBy(ym => ym.month)
            .ToList();

    public async ValueTask<List<(Expense item, Category category, Group? group)>> GetExpensesWithRelatedData(
        DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter)
    {
        var expenses = await GetExpenses(startDate: startDate, endDate: endDate, filter: filter);

        var allCategories = await _categoriesRepo.GetCategories();
        var allGroups = await _groupsRepo.GetGroups();

        var result = new List<(Expense item, Category category, Group? group)>();

        foreach (var exp in expenses)
        {
            var category = allCategories.SingleOrDefault(c => c.Id == exp.CategoryId)
                ?? throw new Exception($@"Category with Id='{exp.CategoryId}' does not exists");

            Group? group = null;

            if (exp is not null && exp.GroupId is not null)
            {
                group = allGroups.SingleOrDefault(g => g.Id == exp.GroupId)
                    ?? throw new Exception($@"Group with Id='{exp.GroupId}' does not exists");
            }

            if (exp is not null)
            {
                result.Add((exp, category, group));
            }
        }

        return result.OrderBy(exp => exp.item.Date).ToList();
    }
}
