using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

public interface IGroupDefaultCategoryRepository
{
    Task AddGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories);

    Task DeleteGroupDefaultCategory(IEnumerable<GroupDefaultCategory> groupCategories);

    Task<List<GroupDefaultCategory>> GetGroupDefaultCategories(Guid? categoryId, Guid? groupId, CancellationToken token);

    Task AddGroupDefaultCategories(IEnumerable<GroupDefaultCategory> groupDefaultCategories);

    Task Clear();
}
