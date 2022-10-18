using System;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Data;

internal interface IJsonData
{
    Task<string> ExportToJson();

    Task ImportFromJson(string data, bool dataMerge);

    Task ClearData();
}

internal sealed class JsonData : IJsonData
{
    private readonly IExpensesRepository _expensesRepo;
    private readonly ICategoriesRepository _categoriesRepo;
    private readonly IGroupDefaultCategoryRepository _groupDefaultCategoriesRepo;
    private readonly IGroupsRepository _groupsRepo;
    private readonly IIncomesRepository _incomesRepo;
    private readonly ILimitsRepository _limitsRepo;

    public JsonData(
        IExpensesRepository expensesRepo,
        ICategoriesRepository categoriesRepo,
        IGroupDefaultCategoryRepository groupDefaultCategoriesRepo,
        IGroupsRepository groupsRepo,
        IIncomesRepository incomesRepo,
        ILimitsRepository limitsRepo
        )
    {
        _expensesRepo = expensesRepo;
        _categoriesRepo = categoriesRepo;
        _groupDefaultCategoriesRepo = groupDefaultCategoriesRepo;
        _groupsRepo = groupsRepo;
        _incomesRepo = incomesRepo;
        _limitsRepo = limitsRepo;
    }

    public async Task<string> ExportToJson()
    {
        var categories = await _categoriesRepo.GetCategories();
        var expenses = await _expensesRepo.GetExpenses(null, null);
        var groups = await _groupsRepo.GetGroups();
        var groupsDefaultCategories = await _groupDefaultCategoriesRepo.GetGroupDefaultCategories(null, null);
        var incomes = await _incomesRepo.GetIncomes();
        var limits = await _limitsRepo.GetLimits();

        var data = new ExpensesDataSerializable
        {
            Categories = categories,
            Expenses = expenses,
            Groups = groups,
            GroupsDefaultCategories = groupsDefaultCategories,
            Incomes = incomes,
            Limits = limits
        };

        byte[] utf8Json = JsonSerializer.SerializeToUtf8Bytes(data, JsonContext.Default.ExpensesDataSerializable);

        return Encoding.UTF8.GetString(utf8Json);
    }

    public async Task ImportFromJson(string data, bool dataMerge)
    {
        var expenses = dataMerge ? await _expensesRepo.GetExpenses(null, null) : new();
        var categories = dataMerge ? await _categoriesRepo.GetCategories() : new();
        var groups = dataMerge ? await _groupsRepo.GetGroups() : new();
        var groupsDefaultCategories = dataMerge ? await _groupDefaultCategoriesRepo.GetGroupDefaultCategories(null, null) : new();
        var incomes = dataMerge ? await _incomesRepo.GetIncomes() : new();
        var limits = dataMerge ? await _limitsRepo.GetLimits() : new();

        if (!string.IsNullOrWhiteSpace(data))
        {
            var imported = JsonSerializer.Deserialize(data, JsonContext.Default.ExpensesDataSerializable)
                ?? throw new Exception("Parsing Error");

            if (dataMerge)
            {
                foreach (var cat in imported.Categories)
                {
                    if (!categories.Any(c => c.Id == cat.Id))
                    {
                        categories.Add(cat);
                    }
                }

                foreach (var gr in imported.Groups)
                {
                    if (!groups.Any(g => g.Id == gr.Id))
                    {
                        groups.Add(gr);
                    }
                }

                foreach (var defCat in imported.GroupsDefaultCategories)
                {
                    if (!groupsDefaultCategories.Contains(defCat)
                        && groups.Any(g => g.Id == defCat.GroupId)
                        && categories.Any(c => c.Id == defCat.CategoryId))
                    {
                        groupsDefaultCategories.Add(defCat);
                    }
                }

                foreach (var exp in imported.Expenses)
                {
                    if (!expenses.Any(e => e.Id == exp.Id)
                        && (categories.Any(c => c.Id == exp.CategoryId)
                        && (exp.GroupId is null || groups.Any(g => g.Id == exp.GroupId))))
                    {
                        expenses.Add(exp);
                    }
                }

                foreach (var inc in imported.Incomes)
                {
                    if (!incomes.Any(i => i.Id == inc.Id))
                    {
                        incomes.Add(inc);
                    }
                }

                foreach (var lim in imported.Limits)
                {
                    if (!limits.Any(i => i.Id == lim.Id))
                    {
                        limits.Add(lim);
                    }
                }
            }
            else
            {
                expenses.AddRange(imported.Expenses);
                categories.AddRange(imported.Categories);
                groups.AddRange(imported.Groups);
                groupsDefaultCategories.AddRange(imported.GroupsDefaultCategories);
                incomes.AddRange(imported.Incomes);
                limits.AddRange(imported.Limits);
            }
        }

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
}
