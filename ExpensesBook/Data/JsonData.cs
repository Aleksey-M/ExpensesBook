using System.Linq;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.LocalStorageRepositories;

namespace ExpensesBook.Data;

internal interface IJsonData
{
    ValueTask<string> ExportToJson();

    ValueTask ImportFromJson(string data, bool dataMerge);

    ValueTask ClearData();
}

internal class JsonData : IJsonData
{
    private readonly ILocalStorageGenericRepository<Expense> _expensesRepo;
    private readonly ILocalStorageGenericRepository<Category> _categoriesRepo;
    private readonly ILocalStorageGenericRepository<GroupDefaultCategory> _groupDefaultCategoriesRepo;
    private readonly ILocalStorageGenericRepository<Group> _groupsRepo;
    private readonly ILocalStorageGenericRepository<Income> _incomesRepo;
    private readonly ILocalStorageGenericRepository<Limit> _limitsRepo;

    public JsonData(
        ILocalStorageGenericRepository<Expense> expensesRepo,
        ILocalStorageGenericRepository<Category> categoriesRepo,
        ILocalStorageGenericRepository<GroupDefaultCategory> groupDefaultCategoriesRepo,
        ILocalStorageGenericRepository<Group> groupsRepo,
        ILocalStorageGenericRepository<Income> incomesRepo,
        ILocalStorageGenericRepository<Limit> limitsRepo
        )
    {
        _expensesRepo = expensesRepo;
        _categoriesRepo = categoriesRepo;
        _groupDefaultCategoriesRepo = groupDefaultCategoriesRepo;
        _groupsRepo = groupsRepo;
        _incomesRepo = incomesRepo;
        _limitsRepo = limitsRepo;
    }

    public async ValueTask<string> ExportToJson()
    {
        var expenses = await _expensesRepo.GetCollection();
        var categories = await _categoriesRepo.GetCollection();
        var groups = await _groupsRepo.GetCollection();
        var groupsDefaultCategories = await _groupDefaultCategoriesRepo.GetCollection();
        var incomes = await _incomesRepo.GetCollection();
        var limits = await _limitsRepo.GetCollection();

        var data = new ExpensesDataSerializable
        {
            Categories = categories,
            Expenses = expenses,
            Groups = groups,
            GroupsDefaultCategories = groupsDefaultCategories,
            Incomes = incomes,
            Limits = limits
        };

        var json = ExpensesDataSerializable.SerializeToJson(data);
        return json;
    }

    public async ValueTask ImportFromJson(string data, bool dataMerge)
    {
        var expenses = dataMerge ? await _expensesRepo.GetCollection() : new();
        var categories = dataMerge ? await _categoriesRepo.GetCollection() : new();
        var groups = dataMerge ? await _groupsRepo.GetCollection() : new();
        var groupsDefaultCategories = dataMerge ? await _groupDefaultCategoriesRepo.GetCollection() : new();
        var incomes = dataMerge ? await _incomesRepo.GetCollection() : new();
        var limits = dataMerge ? await _limitsRepo.GetCollection() : new();

        if (!string.IsNullOrWhiteSpace(data))
        {
            var imported = ExpensesDataSerializable.DeserializeFromJson(data);

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

        await _expensesRepo.SetCollection(expenses);
        await _categoriesRepo.SetCollection(categories);
        await _groupsRepo.SetCollection(groups);
        await _groupDefaultCategoriesRepo.SetCollection(groupsDefaultCategories);
        await _incomesRepo.SetCollection(incomes);
        await _limitsRepo.SetCollection(limits);
    }

    public async ValueTask ClearData()
    {
        await _expensesRepo.Clear();
        await _categoriesRepo.Clear();
        await _groupsRepo.Clear();
        await _groupDefaultCategoriesRepo.Clear();
        await _incomesRepo.Clear();
        await _limitsRepo.Clear();
    }
}
