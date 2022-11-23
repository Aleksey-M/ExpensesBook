namespace IdbLib;

/// <summary>
/// Класс для передачи информации о IDBObjectStore
/// </summary>
public sealed class ObjectStoreInfo
{
    public string Name { get; set; } = string.Empty;

    public string KeyPath { get; set; } = string.Empty;
}
