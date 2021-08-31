using System;

namespace Preflight.Parsers
{
    public enum ParserType
    {
        [ParserTypeInfo(new[] { KnownPropertyAlias.NestedContent }, typeof(NestedContentValueParser))]
        NestedContent = 1,

        [ParserTypeInfo(new[] { KnownPropertyAlias.BlockList }, typeof(BlockListValueParser))]
        BlockList = 2,

        [ParserTypeInfo(new[] { KnownPropertyAlias.Grid }, typeof(GridValueParser))]
        Grid = 3,        

        [ParserTypeInfo(new[] { KnownPropertyAlias.Rte, KnownPropertyAlias.Textarea, KnownPropertyAlias.Textbox }, typeof(StringValueParser))]
        String = 4,
    }

    public class ParserTypeInfoAttribute : Attribute
    {
        internal ParserTypeInfoAttribute(string[] parsableProperties, Type serviceType)
        {
            ParsableProperties = parsableProperties;
            ServiceType = serviceType;
        }

        public string[] ParsableProperties { get; private set; }
        public Type ServiceType { get; private set; }
    }
}
