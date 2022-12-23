using Preflight.Models;

namespace Preflight.Parsers;

public interface IPreflightValueParser
{
    List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams);
}
