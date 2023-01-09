using UmbConstants = Umbraco.Cms.Core.Constants;

namespace Preflight;

public static class KnownPropertyAlias
{
    public const string Grid = UmbConstants.PropertyEditors.Aliases.Grid;
    public const string Rte = UmbConstants.PropertyEditors.Aliases.TinyMce;
    public const string Textarea = UmbConstants.PropertyEditors.Aliases.TextArea;
    public const string Textbox = UmbConstants.PropertyEditors.Aliases.TextBox;
    public const string NestedContent = UmbConstants.PropertyEditors.Aliases.NestedContent;
    public const string BlockList = UmbConstants.PropertyEditors.Aliases.BlockList;
    public const string BlockGrid = UmbConstants.PropertyEditors.Aliases.BlockGrid;

    public static IEnumerable<string?> All => typeof(KnownPropertyAlias).GetFields()
            .Where(x => x.IsLiteral).Select(x => x.GetRawConstantValue()?.ToString());
}

public static class KnownGridEditorAlias
{
    public const string Rte = "rte";
    public const string Textarea = "textarea";
    public const string Textstring = "textstring";
}
