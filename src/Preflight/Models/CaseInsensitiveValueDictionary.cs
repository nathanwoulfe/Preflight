namespace Preflight.Models;

public class CaseInsensitiveValueDictionary : Dictionary<string, object?>
{
    public CaseInsensitiveValueDictionary()
        : base(StringComparer.InvariantCultureIgnoreCase) { }
}

