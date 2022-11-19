using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ExpensesBook.Data;

[JsonSerializable(typeof(GlobalDataSerializable))]
internal partial class GlobalDataSerializableContext : JsonSerializerContext
{
}

internal static class JsonGlobalData
{
    public static string Export(GlobalDataSerializable data) =>
        JsonSerializer.Serialize(data, GlobalDataSerializableContext.Default.GlobalDataSerializable);

    public static GlobalDataSerializable Import(string jsonString) =>
        string.IsNullOrWhiteSpace(jsonString)
        ? new()
        : JsonSerializer.Deserialize(jsonString, GlobalDataSerializableContext.Default.GlobalDataSerializable)
                ?? throw new Exception("Parsing Error");
}
