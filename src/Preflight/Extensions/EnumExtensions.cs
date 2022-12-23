using Preflight.Parsers;

namespace Preflight.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TAttribute"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static TAttribute? GetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
    {
        Type type = value.GetType();
        string? name = Enum.GetName(type, value);

        if (name is null)
        {
            return default;
        }

        return type.GetField(name)?
            .GetCustomAttributes(false)
            .OfType<TAttribute>()
            .SingleOrDefault();
    }

    /// <summary>
    /// Gets the ParserType for the given propertyAlias, if one exists
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="propertyAlias"></param>
    /// <returns></returns>
    public static ParserType? GetByParsablePropertyAlias<T>(string propertyAlias)
        where T : struct, IConvertible
    {
        if (!KnownPropertyAlias.All.Contains(propertyAlias))
        {
            return null;
        }

        foreach (ParserType member in (ParserType[])Enum.GetValues(typeof(ParserType)))
        {
            ParserTypeInfoAttribute? attribute = member.GetAttribute<ParserTypeInfoAttribute>();

            if (attribute is null)
            {
                return null;
            }

            if (attribute.ParsableProperties.Contains(propertyAlias))
            {
                return member;
            }
        }

        return null;
    }
}
