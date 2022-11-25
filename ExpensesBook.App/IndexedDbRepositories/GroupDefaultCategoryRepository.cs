using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public sealed class GroupDefaultCategoryRepository :
    BaseIndexedDbRepository<GroupDefaultCategory>, IGroupDefaultCategoryRepository
{
    protected override string CollectionName => "groupdefaultcategories";

    public GroupDefaultCategoryRepository(IndexedDbManager manager) : base(manager)
    {
    }

    public async Task AddGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories)
    {
        if (!groupCategories.Any()) return;

        var list = await GetCollection() ?? new();
        list = list.Union(groupCategories).ToList();

        await SetCollection(list);
    }

    public async Task DeleteGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories)
    {
        if (!groupCategories.Any()) return;

        var list = await GetCollection() ?? new();
        list = list.Except(groupCategories).ToList();

        await SetCollection(list);
    }

    public async Task<List<GroupDefaultCategory>> GetGroupDefaultCategories(Guid? categoryId,
        Guid? groupId, CancellationToken token)
    {
        var fullList = await GetCollection() ?? new();

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
