using System.Collections.Generic;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;

namespace ExpensesBook.LocalStorageRepositories2;

internal sealed class AsyncCategoriesListRepository : BaseLocalStorageRepository, IAsyncCategoriesListRepository
{
    protected override string StorageCollectionName => "categories";

    public AsyncCategoriesListRepository(ILocalStorageService localStorageService) : base(localStorageService) { }

    public IAsyncEnumerable<Category> GetCategoriesAsyncEnumerable() => GetEntitiesAsyncEnumerable<Category>();
}
