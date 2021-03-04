using ExpensesBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ExpensesBook.Domain.Repositories
{
    internal interface ICategoriesRepository
    {
        ValueTask AddCategory(Category category);
        ValueTask<List<Category>> GetCategories();
        ValueTask UpdateCategory(Category category);
        ValueTask DeleteCategory(Guid categoryId);
    }
}
