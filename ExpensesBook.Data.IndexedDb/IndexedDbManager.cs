using Microsoft.JSInterop;

namespace IdbLib;

/// <summary>
/// Класс для создания подключения к indexedDb и выполнения операций с базами
/// </summary>
public sealed class IndexedDbManager : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

    public IndexedDbManager(IJSRuntime jsRuntime)
    {
        moduleTask = new(() => jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./_content/ExpensesBook.Data.IndexedDb/indexedDb.js").AsTask());
    }

    /// <summary>
    /// Создание js-объекта типа IDBDatabase. Если базы не существует, она будет создана
    /// </summary>
    /// <param name="name">Название базы данных</param>
    /// <param name="version">Версия базы данных</param>
    /// <param name="objectStores">Информация для создания списка IDBObjectStore при создании базы данных</param>
    /// <returns>Экземпляр класса, содержащий ссылку на js-объект IDBDatabase и информацию о базе данных</returns>
    /// <exception cref="IndexedDbException">Ошибка при выполнении базы данных</exception>
    public async Task<IndexedDb> Connect(string name, int version, List<ObjectStoreInfo> objectStores)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask<IJSObjectReference>();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("connect", name, version, objectStores, callbacksJsRef);
        var indexedDbRef = await callbacks.WaitTask;

        if (indexedDbRef == null) throw new IndexedDbException("Error on connecting to IndexedDb");

        return new IndexedDb(indexedDbRef, name, version);
    }

    /// <summary>
    /// Добавление данных в IDBObjectStore
    /// </summary>
    /// <typeparam name="T">Тип добавляемых данных</typeparam>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <param name="item">Данные</param>
    /// <returns></returns>
    public async Task AddItem<T>(IndexedDb database, string storeName, T item)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("addItem", database.JsObjectRef, callbacksJsRef, storeName, item);
        await callbacks.WaitTask;
    }

    /// <summary>
    /// Получение записи по ее id
    /// </summary>
    /// <typeparam name="T">Тип возвращаемой записи</typeparam>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <param name="id">Значение id записи</param>
    /// <returns>Полученная из indexedDb запись указанного типа</returns>
    public async Task<T> GetItem<T>(IndexedDb database, string storeName, object id)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask<T>();
        using var callbacksJsRef = callbacks.CreateRefForJs();
        
        await module.InvokeVoidAsync("getItem", database.JsObjectRef, callbacksJsRef, storeName, id);
        return await callbacks.WaitTask;
    }

    /// <summary>
    /// Удаление записи по ее id
    /// </summary>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <param name="id">Значение id записи</param>
    /// <returns></returns>
    public async Task DeleteItem(IndexedDb database, string storeName, object id)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("deleteItem", database.JsObjectRef, callbacksJsRef, storeName, id);
        await callbacks.WaitTask;
    }

    /// <summary>
    /// Получение количества хранящихся в IDBObjectStore записей
    /// </summary>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <returns></returns>
    public async Task<long> GetCount(IndexedDb database, string storeName)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask<long>();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("getCount", database.JsObjectRef, callbacksJsRef, storeName);
        return await callbacks.WaitTask;
    }

    /// <summary>
    /// Удаление всех хрянихся в IDBObjectStore записей
    /// </summary>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <returns></returns>
    public async Task ClearStore(IndexedDb database, string storeName)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("clearItems", database.JsObjectRef, callbacksJsRef, storeName);
        await callbacks.WaitTask;
    }

    /// <summary>
    /// Получение списка объектов из хранилища
    /// </summary>
    /// <typeparam name="T">Тип возвращаемой записи</typeparam>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <returns></returns>
    public async Task<List<T>> GetItems<T>(IndexedDb database, string storeName, List<PropertyCriteria>? filters = null)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask<List<T>>();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("getItemsList", database.JsObjectRef, callbacksJsRef, storeName, filters);
        return await callbacks.WaitTask;
    }

    /// <summary>
    /// Множественное добавление записей в IDBObjectStore
    /// </summary>
    /// <typeparam name="T">Тип элементов последовательности</typeparam>
    /// <param name="database">Информация об экземпляре IDBDatabase</param>
    /// <param name="storeName">Название IDBObjectStore</param>
    /// <param name="items">Последовательность значений</param>
    public async Task AddItemsRange<T>(IndexedDb database, string storeName, IEnumerable<T> items)
    {
        var module = await moduleTask.Value;

        var callbacks = new CallbacksTask();
        using var callbacksJsRef = callbacks.CreateRefForJs();

        await module.InvokeVoidAsync("addItemsRange", database.JsObjectRef, callbacksJsRef, storeName, items.ToList());
        await callbacks.WaitTask;
    }

    public ObjectStore<T> GetObjectStore<T>(IndexedDb database, string storeName) => new(this, database, storeName);


    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
