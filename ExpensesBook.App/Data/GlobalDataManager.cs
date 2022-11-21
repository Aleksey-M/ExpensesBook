using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Data;

public sealed class GlobalDataManager : IGlobalDataManager
{
    private readonly IExpensesRepository _expensesRepo;
    private readonly ICategoriesRepository _categoriesRepo;
    private readonly IGroupDefaultCategoryRepository _groupDefaultCategoriesRepo;
    private readonly IGroupsRepository _groupsRepo;
    private readonly IIncomesRepository _incomesRepo;
    private readonly ILimitsRepository _limitsRepo;

    public GlobalDataManager(
        IExpensesRepository expensesRepo,
        ICategoriesRepository categoriesRepo,
        IGroupDefaultCategoryRepository groupDefaultCategoriesRepo,
        IGroupsRepository groupsRepo,
        IIncomesRepository incomesRepo,
        ILimitsRepository limitsRepo)
    {
        _expensesRepo = expensesRepo;
        _categoriesRepo = categoriesRepo;
        _groupDefaultCategoriesRepo = groupDefaultCategoriesRepo;
        _groupsRepo = groupsRepo;
        _incomesRepo = incomesRepo;
        _limitsRepo = limitsRepo;
    }

    public async Task<GlobalDataSerializable> GetAllData(CancellationToken token)
    {
        var categories = await _categoriesRepo.GetCategories(token);
        var expenses = await _expensesRepo.GetExpenses(token);
        var groups = await _groupsRepo.GetGroups(token);
        var groupsDefaultCategories = await _groupDefaultCategoriesRepo.GetGroupDefaultCategories(null, null, token);
        var incomes = await _incomesRepo.GetIncomes(token);
        var limits = await _limitsRepo.GetLimits(token);

        return new GlobalDataSerializable
        {
            Categories = categories,
            Expenses = expenses,
            Groups = groups,
            GroupsDefaultCategories = groupsDefaultCategories,
            Incomes = incomes,
            Limits = limits
        };
    }

    public async Task SetAllData(GlobalDataSerializable data, CancellationToken token)
    {
        var expenses = await _expensesRepo.GetExpenses(token);
        var categories = await _categoriesRepo.GetCategories(token);
        var groups = await _groupsRepo.GetGroups(token);
        var groupsDefaultCategories = await _groupDefaultCategoriesRepo.GetGroupDefaultCategories(null, null, token);
        var incomes = await _incomesRepo.GetIncomes(token);
        var limits = await _limitsRepo.GetLimits(token);

        foreach (var cat in data.Categories)
        {
            if (!categories.Any(c => c.Id == cat.Id))
            {
                categories.Add(cat);
            }
        }

        foreach (var gr in data.Groups)
        {
            if (!groups.Any(g => g.Id == gr.Id))
            {
                groups.Add(gr);
            }
        }

        foreach (var defCat in data.GroupsDefaultCategories)
        {
            if (!groupsDefaultCategories.Contains(defCat)
                && groups.Any(g => g.Id == defCat.GroupId)
                && categories.Any(c => c.Id == defCat.CategoryId))
            {
                groupsDefaultCategories.Add(defCat);
            }
        }

        foreach (var exp in data.Expenses)
        {
            if (!expenses.Any(e => e.Id == exp.Id)
                && (categories.Any(c => c.Id == exp.CategoryId)
                && (exp.GroupId is null || groups.Any(g => g.Id == exp.GroupId))))
            {
                expenses.Add(exp);
            }
        }

        foreach (var inc in data.Incomes)
        {
            if (!incomes.Any(i => i.Id == inc.Id))
            {
                incomes.Add(inc);
            }
        }

        foreach (var lim in data.Limits)
        {
            if (!limits.Any(i => i.Id == lim.Id))
            {
                limits.Add(lim);
            }
        }

        await Task.Yield();

        await ClearData();

        await _expensesRepo.AddExpenses(expenses);
        await _categoriesRepo.AddCategories(categories);
        await _groupsRepo.AddGroups(groups);
        await _groupDefaultCategoriesRepo.AddGroupDefaultCategories(groupsDefaultCategories);
        await _incomesRepo.AddIncomes(incomes);
        await _limitsRepo.AddLimits(limits);
    }

    public async Task ClearData()
    {
        await _expensesRepo.Clear();
        await _groupsRepo.Clear();
        await _groupDefaultCategoriesRepo.Clear();
        await _categoriesRepo.Clear();
        await _incomesRepo.Clear();
        await _limitsRepo.Clear();
    }

    public async Task<int> ImportExpensesFromFlatList(List<ParsedExpense> parsedExpenses, CancellationToken token)
    {
        if (parsedExpenses.Count == 0) return 0;

        var existedCategories = await _categoriesRepo.GetCategories(token);
        var newCategories = new List<Category>();

        var existedGroups = await _groupsRepo.GetGroups(token);
        var newGroups = new List<Group>();

        foreach (var parsedExpense in parsedExpenses)
        {
            await Task.Yield();

            var category = existedCategories
                .Where(x => x.Name.ToUpper() == parsedExpense.CategoryName.ToUpper())
                .FirstOrDefault();

            if (category != null)
            {
                parsedExpense.Expense.CategoryId = category.Id;
            }
            else
            {
                var createdCategory = newCategories
                    .Where(x => x.Name.ToUpper() == parsedExpense.CategoryName.ToUpper())
                    .FirstOrDefault();

                if (createdCategory != null)
                {
                    parsedExpense.Expense.CategoryId = createdCategory.Id;
                }
                else
                {
                    var newCategory = new Category
                    {
                        Id = Guid.NewGuid(),
                        Name = parsedExpense.CategoryName.Trim()
                    };

                    newCategories.Add(newCategory);
                    parsedExpense.Expense.CategoryId = newCategory.Id;
                }
            }

            if (string.IsNullOrWhiteSpace(parsedExpense.GroupName))
            {
                var catGroup = await _groupDefaultCategoriesRepo.GetGroupDefaultCategories(parsedExpense.Expense.CategoryId, null, token);
                parsedExpense.Expense.GroupId = catGroup.FirstOrDefault()?.GroupId;
            }
            else
            {
                var existedGroup = existedGroups
                    .Where(x => x.Name.ToUpper() == parsedExpense.GroupName?.ToUpper())
                    .FirstOrDefault();

                if (existedGroup != null)
                {
                    parsedExpense.Expense.GroupId = existedGroup.Id;
                }
                else
                {
                    var createdGroup = newGroups
                        .Where(x => x.Name.ToUpper() == parsedExpense.GroupName?.ToUpper())
                        .FirstOrDefault();

                    if (createdGroup != null)
                    {
                        parsedExpense.Expense.GroupId = createdGroup.Id;
                    }
                    else
                    {
                        var newGroup = new Group
                        {
                            Id = Guid.NewGuid(),
                            Name = parsedExpense.GroupName.Trim()
                        };

                        newGroups.Add(newGroup);
                        parsedExpense.Expense.GroupId = newGroup.Id;
                    }
                }
            }
        }

        await Task.Yield();

        await _categoriesRepo.AddCategories(newCategories);
        await _groupsRepo.AddGroups(newGroups);
        var newExpenses = parsedExpenses.Select(x => x.Expense);
        await _expensesRepo.AddExpenses(newExpenses);

        return parsedExpenses.Count;
    }
}
