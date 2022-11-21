using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

public sealed class GroupDefaultCategoryRepository :
    BaseLocalStorageRepository<GroupDefaultCategory>, IGroupDefaultCategoryRepository
{
    protected override string CollectionName => "groupdefaultcategories";

    public GroupDefaultCategoryRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task AddGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories)
    {
        if (!groupCategories.Any()) return;

        var list = await GetCollection(token: default) ?? new();
        list = list.Union(groupCategories).ToList();

        await SetCollection(list);
    }

    public async Task DeleteGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories)
    {
        if (!groupCategories.Any()) return;

        var list = await GetCollection(token: default) ?? new();
        list = list.Except(groupCategories).ToList();

        await SetCollection(list);
    }

    public async Task<List<GroupDefaultCategory>> GetGroupDefaultCategories(Guid? categoryId,
        Guid? groupId, CancellationToken token)
    {
        var fullList = await GetCollection(token) ?? new();

        Func<GroupDefaultCategory, bool> predicate = (categoryId, groupId) switch
        {
            (not null, not null) => ec => ec.CategoryId == categoryId && ec.GroupId == groupId,
            (null, not null) => ec => ec.GroupId == groupId,
            (not null, null) => ec => ec.CategoryId == categoryId,
            (_, _) => _ => true
        };

        return fullList.Where(predicate).ToList();
    }

    public async Task AddGroupDefaultCategories(IEnumerable<GroupDefaultCategory> groupDefaultCategories) =>
        await SetCollection(groupDefaultCategories.ToList());

    public async Task Clear() => await Clear<List<GroupDefaultCategory>>();
}
