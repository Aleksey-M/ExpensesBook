namespace IdbLib;

public enum PropertyCriteriaType : int
{ 
    Unknown = 0,
    ContainsString = 1,
    EqualsTo = 2,
    GreaterThan = 3,
    LessThan = 4 
}

public sealed class PropertyCriteria
{
    public string PropertyJsName { get; set; } = string.Empty;

    public object Value { get; set; } = null!;

    public PropertyCriteriaType Type { get; set; }
}
