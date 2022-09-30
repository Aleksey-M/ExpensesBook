using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories;

internal sealed class CategoriesRepository : ICategoriesRepository, ILocalStorageGenericRepository<Category>
{
    public CategoriesRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    public ILocalStorageService LocalStorage { get; }

    public async ValueTask AddCategory(Category category) =>
        await (this as ILocalStorageGenericRepository<Category>).AddEntity(category);

    public async ValueTask DeleteCategory(Guid categoryId) =>
        await (this as ILocalStorageGenericRepository<Category>).DeleteEntity(categoryId);

    public async ValueTask<List<Category>> GetCategories() =>
        await (this as ILocalStorageGenericRepository<Category>).GetCollection();

    public async ValueTask UpdateCategory(Category category) =>
        await (this as ILocalStorageGenericRepository<Category>).UpdateEntity(category);
}
