﻿using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.Domain.Services;

public interface ICategoriesService
{
    Task<Category> AddCategory(string categoryName, int? sortOrder);

    Task<List<Category>> GetCategories(CancellationToken token);

    Task UpdateCategory(Guid categoryId, string? categoryName, int? sortOrder);

    Task DeleteCategory(Guid categoryId);

    Task<bool> IsEmptyCategory(Guid categoryId, CancellationToken token);
}

public sealed class CategoriesService : ICategoriesService
{
    private readonly ICategoriesRepository _categoriesRepo;
    private readonly IGroupDefaultCategoryRepository _groupDefaultCategoryRepo;
    private readonly IExpensesRepository _expenseRepo;

    public CategoriesService(ICategoriesRepository categoriesRepository, IExpensesRepository expenseRepo,
        IGroupDefaultCategoryRepository groupDefaultCategoryRepo)
    {
        _categoriesRepo = categoriesRepository;
        _groupDefaultCategoryRepo = groupDefaultCategoryRepo;
        _expenseRepo = expenseRepo;
    }

    public async Task<Category> AddCategory(string categoryName, int? sortOrder)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = categoryName ?? "",
            Sort = sortOrder ?? 0
        };

        await _categoriesRepo.AddCategory(category);

        return category;
    }

    public async Task DeleteCategory(Guid categoryId)
    {
        bool isEmpty = await IsEmptyCategory(categoryId, default);
        if (!isEmpty) throw new Exception($"Category with Id='{categoryId}' using for some expenses");

        var defCateg = await _groupDefaultCategoryRepo.GetGroupDefaultCategories(categoryId, null, default);
        await _categoriesRepo.DeleteCategory(categoryId);
        await _groupDefaultCategoryRepo.DeleteGroupDefaultCategory(defCateg);
    }

    public async Task<bool> IsEmptyCategory(Guid categoryId, CancellationToken token)
    {
        var expenses = await _expenseRepo.GetExpenses(
            startDate: null,
            endDate: null,
            filter: null,
            token);

        return !expenses.Any(e => e.CategoryId == categoryId);
    }

    public async Task<List<Category>> GetCategories(CancellationToken token)
    {
        var list = await _categoriesRepo.GetCategories(token);
        return list.OrderBy(c => c.Sort).ThenBy(c => c.Name).ToList();
    }

    public async Task UpdateCategory(Guid categoryId, string? categoryName, int? sortOrder)
    {
        if (categoryName is null && sortOrder is null) return;

        var allCategories = await _categoriesRepo.GetCategories(token: default);
        var category = allCategories.SingleOrDefault(c => c.Id == categoryId);

        if (category == null) throw new ArgumentException($"Category with Id='{categoryId}' does not exists");

        var updatedCategory = new Category
        {
            Id = category.Id,
            Name = categoryName ?? category.Name,
            Sort = sortOrder ?? category.Sort
        };

        await _categoriesRepo.UpdateCategory(updatedCategory);
    }
}
