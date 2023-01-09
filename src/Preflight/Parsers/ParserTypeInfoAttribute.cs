namespace Preflight.Parsers;

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
