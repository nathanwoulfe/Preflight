namespace Preflight.Parsers;

public enum ParserType
{
    [ParserTypeInfo(new[] { KnownPropertyAlias.NestedContent }, typeof(NestedContentValueParser))]
    NestedContent = 1,

    [ParserTypeInfo(new[] { KnownPropertyAlias.BlockList, KnownPropertyAlias.BlockGrid }, typeof(BlockValueParser))]
    BlockList = 2,

    [ParserTypeInfo(new[] { KnownPropertyAlias.Grid }, typeof(GridValueParser))]
    Grid = 3,

    [ParserTypeInfo(new[] { KnownPropertyAlias.Rte, KnownPropertyAlias.Textarea, KnownPropertyAlias.Textbox }, typeof(StringValueParser))]
    String = 4,
}
