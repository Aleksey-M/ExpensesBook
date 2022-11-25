namespace IdbLib;

/// <summary>
/// Хэлпер для работы с отдельным хранилищем объектов
/// </summary>
/// <typeparam name="T">Тип объектов хранилища</typeparam>
public sealed class ObjectStore<T>
{
    private readonly IndexedDbManager _manager;
    private readonly IndexedDb _indexedDb;
    private readonly string _storageName;

    public ObjectStore(IndexedDbManager manager, IndexedDb indexedDb, string storageName)
    {
        _manager = manager;
        _indexedDb = indexedDb;
        _storageName = storageName;
    }

    public Task AddItem(T item) => _manager.AddItem(_indexedDb, _storageName, item);

    public Task AddItemsRange(IEnumerable<T> item) => _manager.AddItemsRange(_indexedDb, _storageName, item);

    public Task<T> GetItem(object id) => _manager.GetItem<T>(_indexedDb, _storageName, id);

    public Task DeleteItem(object id) => _manager.DeleteItem(_indexedDb, _storageName, id);

    public Task<long> GetCount() => _manager.GetCount(_indexedDb, _storageName);

    public Task ClearStore() => _manager.ClearStore(_indexedDb, _storageName);

    public Task<List<T>> GetItems(List<PropertyCriteria>? filters = null) => _manager.GetItems<T>(_indexedDb, _storageName, filters);
}
