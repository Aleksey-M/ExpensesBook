using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class CategoriesRepository : ICategoriesRepository, ICategoriesListRepository, ILocalStorageGenericRepository<Category>
{
    public CategoriesRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async Task AddCategory(Category category) =>
        await (this as ILocalStorageGenericRepository<Category>).AddEntity(category);

    public Task Clear() => throw new NotImplementedException();

    public async Task DeleteCategory(Guid categoryId) =>
        await (this as ILocalStorageGenericRepository<Category>).DeleteEntity(categoryId);

    public async Task<List<Category>> GetCategories() =>
        await (this as ILocalStorageGenericRepository<Category>).GetCollection();

    public async Task UpdateCategory(Category category) =>
        await (this as ILocalStorageGenericRepository<Category>).UpdateEntity(category);
}
