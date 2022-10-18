using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Domain.Repositories;
using ExpensesBook.Extensions;

namespace ExpensesBook.LocalStorageRepositories2;

internal sealed class CategoriesListRepository : BaseLocalStorageRepository, ICategoriesListRepository
{
    private string _hash = "";
    private List<Category> _cashedCollection = new();

    protected override string StorageCollectionName => "categories";

    public CategoriesListRepository(ILocalStorageService localStorageService) : base(localStorageService) { }

    public async Task<List<Category>> GetCategories()
    {
        var hash = await GetCollectionHash();
        if (hash != _hash)
        {
            _cashedCollection = await GetEntitiesAsyncEnumerable<Category>().ToListAsync();
            _hash = hash;
        }

        return _cashedCollection.ToList();
    }

    public new async Task Clear() => await base.Clear();
}
