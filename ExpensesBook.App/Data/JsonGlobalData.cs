using System.Text.Json;

namespace ExpensesBook.Data;

public static class JsonGlobalData
{
    public static string Export(GlobalDataSerializable data) =>
        JsonSerializer.Serialize(data);

    public static GlobalDataSerializable Import(string jsonString) =>
        string.IsNullOrWhiteSpace(jsonString)
        ? new()
        : JsonSerializer.Deserialize<GlobalDataSerializable>(jsonString)
                ?? throw new Exception("Parsing Error");
}