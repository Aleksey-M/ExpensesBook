using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

internal interface IGroupsService
{
    Task<Group> AddGroup(string groupName, int? sortOrder, IEnumerable<Guid>? relatedCategories);

    Task<List<Group>> GetGroups();

    Task<List<(Group, List<Category>)>> GetGroupsWithRelatedCategories();

    Task UpdateGroup(Guid groupId, string? groupName, int? groupOrder, IEnumerable<Guid>? relatedCategories);

    Task DeleteGroup(Guid groupId);

    Task<List<Category>> GetFreeCategories();

    Task<Group?> GetRelatedGroup(Guid categoryId);
}

internal sealed class GroupsService : IGroupsService
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
            var allRelations = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, null);
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
        var expenses = await _expensesRepo.GetExpenses(null, null);

        foreach (var exp in expenses.Where(e => e.GroupId == groupId))
        {
            exp.GroupId = null;
            await _expensesRepo.UpdateExpense(exp, exp.Date);
        }

        var defCat = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, groupId);
        await _groupDefaultCategRepo.DeleteGroupDefaultCategory(defCat);

        await _groupsRepo.DeleteGroup(groupId);
    }

    public async Task<List<Category>> GetFreeCategories()
    {
        var allCategories = await _categoriesRepo.GetCategories();
        var relCategories = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, null);

        var freeCategories = allCategories
            .Where(c => !relCategories.Any(rc => rc.CategoryId == c.Id))
            .OrderBy(c => c.Sort)
            .ToList();

        return freeCategories;
    }

    public async Task<List<Group>> GetGroups()
    {
        var groups = await _groupsRepo.GetGroups();
        return groups.OrderBy(g => g.Sort).ThenBy(g => g.Name).ToList();
    }

    public async Task<List<(Group, List<Category>)>> GetGroupsWithRelatedCategories()
    {
        var groups = await _groupsRepo.GetGroups();
        var relCateg = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, null);
        var allCategories = await _categoriesRepo.GetCategories();

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

    public async Task<Group?> GetRelatedGroup(Guid categoryId)
    {
        if (categoryId == Guid.Empty) return null;

        var catGroup = await _groupDefaultCategRepo.GetGroupDefaultCategories(categoryId, null);
        if (catGroup is { Count: 0 }) return null;

        var groups = await _groupsRepo.GetGroups();
        var group = groups.SingleOrDefault(g => g.Id == catGroup[0].GroupId);

        return group;
    }

    public async Task UpdateGroup(Guid groupId, string? groupName, int? groupOrder, IEnumerable<Guid>? relatedCategories)
    {
        if (groupName is null && groupOrder is null && relatedCategories is null) return;

        var allGroups = await _groupsRepo.GetGroups();
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
            var relCateg = await _groupDefaultCategRepo.GetGroupDefaultCategories(null, groupId);
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
