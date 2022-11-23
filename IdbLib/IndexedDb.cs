using Microsoft.JSInterop;

namespace IdbLib;

/// <summary>
/// Класс для хранения ссылки на экземпляр js-класса IDBDatabase, названия и версии базы данных
/// </summary>
public sealed class IndexedDb
{
    private readonly IJSObjectReference _idb;
    private readonly string _dbName;
    private readonly int _initialVersion;

    public IndexedDb(IJSObjectReference indexedDbObjRef, string databaseName, int databaseVersion)
    {
        _idb = indexedDbObjRef;
        _dbName = databaseName;
        _initialVersion = databaseVersion;
    }

    public IJSObjectReference JsObjectRef => _idb;

    public string DatabaseName => _dbName;

    public int InitialVersion => _initialVersion;
}
