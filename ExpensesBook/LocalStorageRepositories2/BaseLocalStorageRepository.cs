using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using ExpensesBook.Domain.Entities;
using ExpensesBook.Serialization;

namespace ExpensesBook.LocalStorageRepositories2;

internal abstract class BaseLocalStorageRepository
{
    protected ILocalStorageService LocalStorage { get; }

    protected abstract string StorageCollectionName { get; }

    public BaseLocalStorageRepository(ILocalStorageService localStorageService)
    {
        LocalStorage = localStorageService;
    }

    protected async Task<string> GetCollectionHash()
    {
        var key = StorageCollectionName + "-hash";
        var keyExists = await LocalStorage.ContainKeyAsync(key);

        string? hash = null;

        if (keyExists)
        {
            hash = await LocalStorage.GetItemAsync<string?>(key);
        }

        return hash ?? Guid.NewGuid().ToString();
    }

    protected async Task<string> UpdateCollectionHash()
    {
        var newHash = Guid.NewGuid().ToString();
        await LocalStorage.SetItemAsync(StorageCollectionName + "-hash", newHash);
        return newHash;
    }

    protected async Task<List<string>> GetEntitiesIdentifiers()
    {
        var key = StorageCollectionName + "-list";
        var listExists = await LocalStorage.ContainKeyAsync(key);

        if (!listExists)
        {
            return new List<string>();
        }

        var data = await LocalStorage.GetItemAsync<string>(StorageCollectionName + "-list");
        return EntitiesJsonSerializer.GetEntityFromUtf8Json<List<string>>(data);
    }

    protected async Task UpdateEntitiesIdentifiers(List<string> identifiers)
    {
        var serialized = EntitiesJsonSerializer.GetUtf8JsonString(identifiers);
        await LocalStorage.SetItemAsync(StorageCollectionName + "-list", serialized);
    }

    protected async Task<T> GetEntity<T>(string entityId)
    {
        var data = await LocalStorage.GetItemAsync<string>(entityId);
        return EntitiesJsonSerializer.GetEntityFromUtf8Json<T>(data);
    }

    protected async Task DeleteEntity(string entityId)
    {
        await LocalStorage.RemoveItemAsync(entityId);

        _ = await UpdateCollectionHash();
    }

    protected async Task UpdateEntity<T>(string id, T entity) where T: IEntity
    {
        if (id == string.Empty)
        {
            if (entity.Id == default)
            {
                entity.Id = Guid.NewGuid();
            }

            id = entity.Id.ToString();

            var idsList = await GetEntitiesIdentifiers();
            idsList.Add(id);
            await UpdateEntitiesIdentifiers(idsList);
        }

        var serialized = EntitiesJsonSerializer.GetUtf8JsonString(entity);
        await LocalStorage.SetItemAsync(id, serialized);

        _ = await UpdateCollectionHash();
    }

    protected async Task Clear()
    {
        var identifiers = await GetEntitiesIdentifiers();
        foreach (var id in identifiers)
        {
            await LocalStorage.RemoveItemAsync(id);
        }

        _ = await UpdateCollectionHash();
    }

    protected async IAsyncEnumerable<T> GetEntitiesAsyncEnumerable<T>()
    {
        var identifiers = await GetEntitiesIdentifiers();

        foreach (var id in identifiers)
        {
            yield return await GetEntity<T>(id);
        }
    }
}
