namespace Preflight.Models;

/// <summary>
/// A simple class for passing single property from the backoffice into the content checker
/// </summary>
public class SimpleProperty
{
    public string Name { get; set; } = string.Empty;

    public string Editor { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

public class DirtyProperties
{
    public int Id { get; set; }

    public string Culture { get; set; } = string.Empty;

    public List<SimpleProperty> Properties { get; set; } = new();
}
