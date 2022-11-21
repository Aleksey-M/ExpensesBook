using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.Domain.Repositories;

public interface ICategoriesRepository
{
    Task AddCategory(Category category);

    Task UpdateCategory(Category category);

    Task DeleteCategory(Guid categoryId);

    Task<List<Category>> GetCategories(CancellationToken token);

    Task AddCategories(IEnumerable<Category> categories);

    Task Clear();    
}
