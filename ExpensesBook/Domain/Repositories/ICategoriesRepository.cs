using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

internal interface ICategoriesListRepository
{
    Task<List<Category>> GetCategories();

    Task Clear();
}

internal interface IAsyncCategoriesListRepository
{
    IAsyncEnumerable<Category> GetCategoriesAsyncEnumerable();
}

internal interface ICategoriesRepository
{
    Task AddCategory(Category category);

    Task UpdateCategory(Category category);

    Task DeleteCategory(Guid categoryId);
}
