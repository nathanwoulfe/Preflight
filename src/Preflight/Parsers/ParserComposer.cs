using Microsoft.Extensions.DependencyInjection;
using Preflight.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Preflight.Parsers;

public class PreflightParserComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        _ = builder.Services.AddTransient<NestedContentValueParser>();
        _ = builder.Services.AddTransient<GridValueParser>();
        _ = builder.Services.AddTransient<BlockListValueParser>();
        _ = builder.Services.AddTransient<StringValueParser>();
        _ = builder.Services.AddTransient<Func<ParserType, IPreflightValueParser>>(serviceProvider => key => GetServiceInstance(serviceProvider, key));
    }

    public IPreflightValueParser GetServiceInstance(IServiceProvider serviceProvider, ParserType key)
    {
        Type? serviceType = key.GetAttribute<ParserTypeInfoAttribute>()?.ServiceType;

        if (serviceType is null)
        {
            throw new KeyNotFoundException($"Could not get Preflight parser for {key}");
        }

        object? service = serviceProvider.GetService(serviceType);

        if (service is IPreflightValueParser parser)
        {
            return parser;
        }

        throw new KeyNotFoundException($"Could not get Preflight parser for {key}");
    }
}

