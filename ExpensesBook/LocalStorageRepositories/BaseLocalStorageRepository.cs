using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Serialization;

namespace ExpensesBook.LocalStorageRepositories;

internal abstract class BaseLocalStorageRepository
{
    protected ILocalStorageService LocalStorage { get; }

    protected abstract string CollectionName { get; }

    public BaseLocalStorageRepository(ILocalStorageService localStorage)
    {
        LocalStorage = localStorage;
    }

    public async Task Clear<TCollection>()
    {
        await LocalStorage.RemoveItemAsync(CollectionName);
    }

    protected async Task SetCollection<TCollection>(TCollection collection)
    {
        var serialized = EntitiesJsonSerializer.GetUtf8JsonString(collection);
        await LocalStorage.SetItemAsStringAsync(CollectionName, serialized);
    }

    protected async Task<TCollection?> GetCollection<TCollection>()
    {
        var serialized = await LocalStorage.GetItemAsStringAsync(CollectionName);

        if (string.IsNullOrEmpty(serialized))
        {
            return default;
        }

        var items = EntitiesJsonSerializer.GetEntityFromUtf8Json<TCollection>(serialized);
        return items;
    }

    protected async Task AddEntity<T>(T entity) where T : IEntity
    {
        var items = await GetCollection<List<T>>();

        items ??= new List<T>();
        items.Add(entity);

        await SetCollection(items);
    }

    protected async Task DeleteEntity<T>(Guid entityId) where T : IEntity
    {
        var items = await GetCollection<List<T>>();
        if (items == null) return;

        var e = items.SingleOrDefault(e => e.Id == entityId);
        if (e == null) return;

        items.Remove(e);

        await SetCollection(items);
    }

    protected async Task<List<string>> GetKeys()
    {
        var keysCount = await LocalStorage.LengthAsync();
        var keys = new List<string>();
        for (int i = 0; i < keysCount; i++)
        {
            var key = await LocalStorage.KeyAsync(i);
            keys.Add(key);
        }

        return keys;
    }

    protected async Task UpdateEntity<T>(T entity) where T : IEntity
    {
        var items = await GetCollection<List<T>>();
        if (items == null) throw new Exception($"Collection '{CollectionName}' is empty");

        var ent = items.SingleOrDefault(ent => ent.Id == entity.Id);

        if (ent == null) throw new Exception($"Entity with Id='{entity.Id}' does not exists in '{CollectionName}' collection");

        items.Remove(ent);
        items.Add(entity);

        await SetCollection(items);
    }
}
