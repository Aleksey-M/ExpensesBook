using ExpensesBook.Domain.Entities;
using IdbLib;

namespace ExpensesBook.IndexedDbRepositories;

public abstract class BaseIndexedDbRepository<T> where T : IEntity
{
    private readonly Lazy<Task<ObjectStore<T>>> _storeTask;

    private readonly IndexedDbManager _manager;

    protected abstract string CollectionName { get; }

    public BaseIndexedDbRepository(IndexedDbManager manager)
    {
        _manager = manager;
        _storeTask = new(async () =>
        {
            var db = await _manager.Connect("ExpensesBookDb", 1, CollectionsNames.ObjectStores.ToList());
            return _manager.GetObjectStore<T>(db, CollectionName);
        });
    }

    public async Task Clear<TCollection>()
    {
        var store = await _storeTask.Value;
        await store.ClearStore();
    }

    protected async Task SetCollection(List<T> collection)
    {
        var store = await _storeTask.Value;
        foreach (var item in collection)
        {
            await store.AddItem(item);
        }
    }

    protected async Task<List<T>> GetCollection()
    {
        var store = await _storeTask.Value;
        return await store.GetItems();
    }

    protected async Task AddEntity(T entity)
    {
        var store = await _storeTask.Value;
        await store.AddItem(entity);
    }

    protected async Task DeleteEntity(Guid entityId)
    {
        var store = await _storeTask.Value;
        await store.DeleteItem(entityId);
    }

    protected async Task UpdateEntity(T entity)
    {
        var store = await _storeTask.Value;

        await store.DeleteItem(entity.Id);
        await store.AddItem(entity);
    }
}
