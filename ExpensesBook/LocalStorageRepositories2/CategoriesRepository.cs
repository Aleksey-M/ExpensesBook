using System;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories2;

internal sealed class CategoriesRepository : BaseLocalStorageRepository, ICategoriesRepository
{
    protected override string StorageCollectionName => "categories";

    public CategoriesRepository(ILocalStorageService localStorageService) : base(localStorageService) { }

    public async Task AddCategory(Category category) => await UpdateEntity(string.Empty, category);

    public async Task DeleteCategory(Guid categoryId) => await DeleteEntity(categoryId.ToString());

    public async Task UpdateCategory(Category category) => await UpdateEntity(category.Id.ToString(), category);
}
