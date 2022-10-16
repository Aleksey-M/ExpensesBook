using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class GroupDefaultCategoryRepository : IGroupDefaultCategoryRepository, ILocalStorageGenericRepository<GroupDefaultCategory>
{
    public GroupDefaultCategoryRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async Task AddGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories)
    {
        if (!groupCategories.Any()) return;

        var list = await (this as ILocalStorageGenericRepository<GroupDefaultCategory>).GetCollection();
        list = list.Union(groupCategories).ToList();
        await (this as ILocalStorageGenericRepository<GroupDefaultCategory>).SetCollection(list);
    }

    public async Task DeleteGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories)
    {
        if (!groupCategories.Any()) return;

        var list = await (this as ILocalStorageGenericRepository<GroupDefaultCategory>).GetCollection();
        list = list.Except(groupCategories).ToList();
        await (this as ILocalStorageGenericRepository<GroupDefaultCategory>).SetCollection(list);
    }

    public async Task<List<GroupDefaultCategory>> GetGroupDefaultCategories(Guid? categoryId, Guid? groupId)
    {
        var fullList = await (this as ILocalStorageGenericRepository<GroupDefaultCategory>).GetCollection();

        Func<GroupDefaultCategory, bool> predicate = (categoryId, groupId) switch
        {
            (not null, not null) => ec => ec.CategoryId == categoryId && ec.GroupId == groupId,
            (null, not null) => ec => ec.GroupId == groupId,
            (not null, null) => ec => ec.CategoryId == categoryId,
            (_, _) => _ => true
        };

        return fullList.Where(predicate).ToList();
    }
}
