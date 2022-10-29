using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

internal interface IExpensesService
{
    Task<Expense> AddExpense(DateTimeOffset date, double amounth, string description, Guid categoryId, Guid? groupId);

    Task<List<Expense>> GetExpenses(DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter, CancellationToken token);

    Task UpdateExpense(Guid expenseId, DateTimeOffset? date, double? amounth, string? description, Guid? categoryId, Guid? groupId);

    Task DeleteExpense(Guid expenseId);

    Task<List<(int year, int month, string monthName)>> GetExpensesMonths(CancellationToken token);

    Task<List<(Expense item, Category category, Group? group)>> GetExpensesWithRelatedData(DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? filter, CancellationToken token);
}

internal sealed class ExpensesService : IExpensesService
{
    private readonly IExpensesRepository _expensesRepo;
    private readonly ICategoriesRepository _categoriesRepository;
    private readonly IGroupsRepository _groupsRepo;

    public ExpensesService(IExpensesRepository expensesRepo,
        ICategoriesRepository categoriesRepository,
        IGroupsRepository groupsRepo)
    {
        _expensesRepo = expensesRepo;
        _categoriesRepository = categoriesRepository;
        _groupsRepo = groupsRepo;
    }

    public async Task<Expense> AddExpense(DateTimeOffset date,
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

    public async Task DeleteExpense(Guid expenseId) =>
        await _expensesRepo.DeleteExpense(expenseId);

    public async Task<List<Expense>> GetExpenses(DateTimeOffset? startDate,
        DateTimeOffset? endDate, string? filter, CancellationToken token)
    {
        filter ??= "";

        Func<string, bool> descriptionFilter = filter == ""
            ? _ => true
            : desc => desc.Contains(filter, StringComparison.OrdinalIgnoreCase);

        var fullList = await _expensesRepo.GetExpenses(token);

        Func<DateTimeOffset, bool> datesFilter = (startDate, endDate) switch
        {
            (not null, not null) => d => d.Date >= startDate && d.Date <= endDate,
            (null, not null) => d => d <= endDate,
            (not null, null) => d => d >= startDate,
            (null, null) => d => true
        };

        return fullList
            .Where(x => datesFilter(x.Date))
            .Where(exp => descriptionFilter(exp.Description))
            .OrderBy(exp => exp.Date)
            .ToList();
    }

    public async Task UpdateExpense(Guid expenseId, DateTimeOffset? date,
         double? amounth, string? description, Guid? categoryId, Guid? groupId)
    {
        if (date is null && amounth is null && description is null && categoryId is null) return;

        var expenses = await _expensesRepo.GetExpenses(token: default);
        var expense = expenses.SingleOrDefault(x => x.Id == expenseId);

        if (expense == null) throw new ArgumentException($"Expense with Id='{expenseId}' does not exists");

        var updatedExpense = new Expense
        {
            Id = expense.Id,
            Amounth = amounth ?? expense.Amounth,
            Date = date ?? expense.Date,
            Description = string.IsNullOrWhiteSpace(description) ? expense.Description : description,
            CategoryId = categoryId ?? expense.CategoryId,
            GroupId = groupId
        };

        await _expensesRepo.UpdateExpense(updatedExpense);
    }

    public async Task<List<(int year, int month, string monthName)>> GetExpensesMonths(CancellationToken token) =>
        (await _expensesRepo.GetMonths(token))
            .Select(e => (
                e.year,
                e.month,
                new DateTime(e.year, e.month, 1).ToString("MMMM", CultureInfo.CreateSpecificCulture("ru-RU"))))
            .OrderBy(ym => ym.year)
            .ThenBy(ym => ym.month)
            .ToList();

    public async Task<List<(Expense item, Category category, Group? group)>> GetExpensesWithRelatedData(
        DateTimeOffset? startDate, DateTimeOffset? endDate, string? filter, CancellationToken token)
    {
        var expenses = await GetExpenses(startDate: startDate, endDate: endDate, filter: filter, token: token);

        var allCategories = await _categoriesRepository.GetCategories(token);
        var allGroups = await _groupsRepo.GetGroups(token);

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
