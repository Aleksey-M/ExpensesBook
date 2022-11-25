using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

public interface IGroupsService
{
    Task<Group> AddGroup(string groupName, int? sortOrder, IEnumerable<Guid>? relatedCategories);

    Task<List<Group>> GetGroups(CancellationToken token);

    Task<List<(Group, List<Category>)>> GetGroupsWithRelatedCategories(CancellationToken token);

    Task UpdateGroup(Guid groupId, string? groupName, int? groupOrder, IEnumerable<Guid>? relatedCategories);

    Task DeleteGroup(Guid groupId);

    Task<List<Category>> GetFreeCategories(CancellationToken token);

    Task<Group?> GetRelatedGroup(Guid categoryId, CancellationToken token);
}

public sealed class GroupsService : IGroupsService
{
    private readonly IGroupsRepository _groupsRepo;
    private readonly ICategoriesRepository _categoriesRepo;
    private readonly IGroupDefaultCategoryRepository _groupDefaultCategRepo;
    private readonly IExpensesRepository _expensesRepo;

    public GroupsService(IGroupsRepository gRepo, ICategoriesRepository catRepo,
        IGroupDefaultCategoryRepository gDefCatRepo, IExpensesRepository expensesRepo)
    {
        _groupsRepo = gRepo;
        _categoriesRepo = catRepo;
        _groupDefaultCategRepo = gDefCatRepo;
        _expensesRepo = expensesRepo;
    }

    public async Task<Group> AddGroup(string groupName, int? sortOrder, IEnumerable<Guid>? relatedCategories)
    {
        var group = new Group
        {
            Id = Guid.NewGuid(),
            Name = groupName,
            Sort = sortOrder ?? 0
        };

        await _groupsRepo.AddGroup(group);

        if (relatedCategories != null && relatedCategories.Any())
        {
            var allRelations = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, null, token: default);
            var filtered = relatedCategories.Where(rc => !allRelations.Any(r => r.CategoryId == rc)).ToList();

            var relations = filtered.Select(id => new GroupDefaultCategory
            {
                Id = Guid.NewGuid(),
                GroupId = group.Id,
                CategoryId = id
            })
                .ToList();

            await _groupDefaultCategRepo.AddGroupDefaultCategory(relations);
        }

        return group;
    }

    public async Task DeleteGroup(Guid groupId)
    {
        var expenses = await _expensesRepo.GetExpenses(filters: null, token: default);

        foreach (var exp in expenses.Where(e => e.GroupId == groupId))
        {
            exp.GroupId = null;
            await _expensesRepo.UpdateExpense(exp);
        }

        var defCat = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, groupId, token: default);
        await _groupDefaultCategRepo.DeleteGroupDefaultCategory(defCat);

        await _groupsRepo.DeleteGroup(groupId);
    }

    public async Task<List<Category>> GetFreeCategories(CancellationToken token)
    {
        var allCategories = await _categoriesRepo.GetCategories(token);
        var relCategories = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, null, token);

        var freeCategories = allCategories
            .Where(c => !relCategories.Any(rc => rc.CategoryId == c.Id))
            .OrderBy(c => c.Sort)
            .ToList();

        return freeCategories;
    }

    public async Task<List<Group>> GetGroups(CancellationToken token)
    {
        var groups = await _groupsRepo.GetGroups(token);
        return groups.OrderBy(g => g.Sort).ThenBy(g => g.Name).ToList();
    }

    public async Task<List<(Group, List<Category>)>> GetGroupsWithRelatedCategories(CancellationToken token)
    {
        var groups = await _groupsRepo.GetGroups(token);
        var relCateg = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, null, token);
        var allCategories = await _categoriesRepo.GetCategories(token);

        var relCategoriesList = relCateg
            .Join(allCategories, gdc => gdc.CategoryId, c => c.Id, (gdc, c) => (defCat: gdc, category: c))
            .GroupBy(rc => rc.defCat.GroupId);

        var res = groups.Select(g => (group: g,
            categories: relCategoriesList
                .SingleOrDefault(gr => gr.Key == g.Id)
                ?.Select(gr => gr.category)
                ?.ToList() ?? new()))
            .OrderBy(g => g.group.Sort)
            .ToList();

        return res;
    }

    public async Task<Group?> GetRelatedGroup(Guid categoryId, CancellationToken token)
    {
        if (categoryId == Guid.Empty) return null;

        var catGroup = await _groupDefaultCategRepo.GetGroupDefaultCategories(categoryId, null, token);
        if (catGroup is { Count: 0 }) return null;

        var groups = await _groupsRepo.GetGroups(token);
        var group = groups.SingleOrDefault(g => g.Id == catGroup[0].GroupId);

        return group;
    }

    public async Task UpdateGroup(Guid groupId, string? groupName, int? groupOrder, IEnumerable<Guid>? relatedCategories)
    {
        if (groupName is null && groupOrder is null && relatedCategories is null) return;

        var allGroups = await _groupsRepo.GetGroups(token: default);
        var group = allGroups.SingleOrDefault(g => g.Id == groupId);

        if (group == null) throw new ArgumentException($"Group with Id='{groupId}' does not exists");

        var updatedGroup = new Group
        {
            Id = group.Id,
            Name = groupName ?? group.Name,
            Sort = groupOrder ?? group.Sort
        };

        await _groupsRepo.UpdateGroup(updatedGroup);

        if (relatedCategories is not null)
        {
            var relCateg = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, groupId, token: default);
            await _groupDefaultCategRepo.DeleteGroupDefaultCategory(relCateg);
            relCateg = relatedCategories
                .Select(c => new GroupDefaultCategory
                {
                    Id = Guid.NewGuid(),
                    GroupId = groupId,
                    CategoryId = c
                })
                .ToList();

            await _groupDefaultCategRepo.AddGroupDefaultCategory(relCateg);
        }
    }
}
