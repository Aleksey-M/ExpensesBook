﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Serialization;

namespace ExpensesBook.LocalStorageRepositories;

internal abstract class BaseLocalStorageRepository<T> where T : IEntity
{
    protected ILocalStorageService LocalStorage { get; }

    protected abstract string CollectionName { get; }

    private List<T>? _cash = null;

    public BaseLocalStorageRepository(ILocalStorageService localStorage)
    {
        LocalStorage = localStorage;
    }

    private async Task EnsureCashLoaded(CancellationToken token)
    {
        await Task.Delay(1, token);

        if (_cash == null)
        {
            var serialized = await LocalStorage.GetItemAsStringAsync(CollectionName);
            await Task.Delay(1, token);

            _cash = new();

            if (!string.IsNullOrEmpty(serialized))
            {
                var items = EntitiesJsonSerializer.GetEntitiesFromJsonString<List<T>>(serialized);                
                _cash.AddRange(items);
            }
        }
    }

    private async Task WriteCash()
    {
        await Task.Delay(1);

        var serialized = EntitiesJsonSerializer.GetJsonString(_cash);
        await Task.Delay(1);

        await LocalStorage.SetItemAsStringAsync(CollectionName, serialized);
    }

    public async Task Clear<TCollection>()
    {
        _cash?.Clear();
        _cash = null;
        await LocalStorage.RemoveItemAsync(CollectionName);
    }

    protected async Task SetCollection(List<T> collection)
    {
        await EnsureCashLoaded(token: default);

        _cash?.AddRange(collection);

        await WriteCash();
    }

    protected async Task<List<T>> GetCollection(CancellationToken token)
    {
        await EnsureCashLoaded(token);

        return _cash!.ToList();
    }

    protected async Task AddEntity(T entity)
    {
        await EnsureCashLoaded(token: default);

        _cash!.Add(entity);

        await WriteCash();
    }

    protected async Task DeleteEntity(Guid entityId)
    {
        await EnsureCashLoaded(token: default);

        var e = _cash!.SingleOrDefault(e => e.Id == entityId);
        if (e == null) return;

        _cash!.Remove(e);

        await WriteCash();
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

    protected async Task UpdateEntity(T entity)
    {
        await EnsureCashLoaded(token: default);

        var ent = _cash!.SingleOrDefault(ent => ent.Id == entity.Id);

        if (ent == null) throw new Exception($"Entity with Id='{entity.Id}' does not exists in '{CollectionName}' collection");

        _cash!.Remove(ent);
        _cash!.Add(entity);

        await WriteCash();
    }
}
