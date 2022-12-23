namespace Preflight.Models;

public class ContentParserParams
{
    public string PropertyName { get; set; } = string.Empty;

    public string PropertyValue { get; set; } = string.Empty;

    public string PropertyEditorAlias { get; set; } = string.Empty;

    public string Culture { get; set; } = string.Empty;

    public int NodeId { get; set; }

    public bool FromSave { get; set; }
}
