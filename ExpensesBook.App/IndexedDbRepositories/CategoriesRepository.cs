using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public sealed class CategoriesRepository : BaseIndexedDbRepository<Category>, ICategoriesRepository
{
    protected override string CollectionName => "categories";

    public CategoriesRepository(IndexedDbManager manager) : base(manager)
    {
    }

    public async Task AddCategory(Category category) => await AddEntity(category);

    public async Task DeleteCategory(Guid categoryId) => await DeleteEntity(categoryId);

    public async Task<List<Category>> GetCategories(CancellationToken token) => await GetCollection() ?? new();

    public async Task UpdateCategory(Category category) => await UpdateEntity(category);

    public async Task AddCategories(IEnumerable<Category> categories) => await SetCollection(categories.ToList());

    public async Task Clear() => await Clear<List<Category>>();
}
