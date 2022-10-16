using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;

namespace ExpensesBook.LocalStorageRepositories;

internal interface ILocalStorageGenericRepository<T> where T : IEntity
{
    ILocalStorageService LocalStorage { get; }

    async Task Clear()
    {
        var collectionName = typeof(T).Name;
        await LocalStorage.RemoveItemAsync(collectionName);
    }

    async Task SetCollection(IEnumerable<T> collection)
    {
        var collectionName = typeof(T).Name;
        await LocalStorage.SetItemAsync(collectionName, collection.ToList());
    }

    async Task<List<T>> GetCollection()
    {
        var collectionName = typeof(T).Name;
        var items = await LocalStorage.GetItemAsync<List<T>>(collectionName);

        return items ?? new List<T>();
    }

    async Task AddEntity(T entity)
    {
        var collectionName = typeof(T).Name;
        var list = await LocalStorage.GetItemAsync<List<T>>(collectionName);

        list ??= new List<T>();

        list.Add(entity);
        await LocalStorage.SetItemAsync(collectionName, list);
    }

    async Task DeleteEntity(Guid entityId)
    {
        var collectionName = typeof(T).Name;
        var list = await LocalStorage.GetItemAsync<List<T>>(collectionName);
        if (list is null) return;

        var e = list.SingleOrDefault(e => e.Id == entityId);
        if (e is null) return;

        list.Remove(e);
        await LocalStorage.SetItemAsync(collectionName, list);
    }

    async Task<List<string>> GetKeys()
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

    async Task UpdateEntity(T entity)
    {
        var collectionName = typeof(T).Name;
        var entities = await LocalStorage.GetItemAsync<List<T>>(collectionName);

        if (entities == null) throw new Exception($"Collection '{collectionName}' is empty");
        var ent = entities.SingleOrDefault(ent => ent.Id == entity.Id);

        if (ent == null) throw new Exception($"Entity with Id='{entity.Id}' does not exists in '{collectionName}' collection");

        entities.Remove(ent);
        entities.Add(entity);
        await LocalStorage.SetItemAsync(collectionName, entities);
    }
}
