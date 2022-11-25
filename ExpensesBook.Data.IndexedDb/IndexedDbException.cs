namespace IdbLib;

/// <summary>
/// Ошибка, возникшая при ожидании завершения операции с indexedDb
/// </summary>
public class IndexedDbException : Exception
{
    public IndexedDbException(string message) : base(message)
    {
    }
}
