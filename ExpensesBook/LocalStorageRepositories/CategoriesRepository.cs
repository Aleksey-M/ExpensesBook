using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class CategoriesRepository : BaseLocalStorageRepository<Category>, ICategoriesRepository
{
    protected override string CollectionName => "categories";

    public CategoriesRepository(ILocalStorageService localStorageService) : base(localStorageService)
    {
    }

    public async Task AddCategory(Category category) => await AddEntity(category);

    public async Task DeleteCategory(Guid categoryId) => await DeleteEntity(categoryId);

    public async Task<List<Category>> GetCategories() => await GetCollection() ?? new();

    public async Task UpdateCategory(Category category) => await UpdateEntity(category);

    public async Task AddCategories(IEnumerable<Category> categories) => await SetCollection(categories.ToList());

    public async Task Clear() => await Clear<List<Category>>();
}
