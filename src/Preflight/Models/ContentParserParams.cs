namespace Preflight.Models
{
    public class ContentParserParams
    {
        public string PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string PropertyEditorAlias { get; set; }
        public string Culture { get; set; }
        public int NodeId { get; set; }
        public bool FromSave { get; set; }
    }
}
