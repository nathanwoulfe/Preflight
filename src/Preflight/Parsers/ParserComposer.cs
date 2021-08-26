using Preflight.Services;
using Preflight.Services.Implement;
using System;
using System.Collections.Generic;
#if NETCOREAPP
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
#else
using Umbraco.Core;
using Umbraco.Core.Composing;
#endif

namespace Preflight.Parsers
{
#if NETCOREAPP
    public class PreflightParserComposer : IComposer
    {
        public void Compose(IUmbracoBuilder builder)
        {
            builder.Services.AddSingleton<IValueParserService, ValueParserService>();

            builder.Services.AddTransient<NestedContentValueParser>();
            builder.Services.AddTransient<GridValueParser>();
            builder.Services.AddTransient<BlockListValueParser>();
            builder.Services.AddTransient<StringValueParser>();
            builder.Services.AddTransient<Func<ParserType, IPreflightValueParser>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case ParserType.NestedContent:
                        return serviceProvider.GetService<NestedContentValueParser>();
                    case ParserType.Grid:
                        return serviceProvider.GetService<GridValueParser>();
                    case ParserType.BlockList:
                        return serviceProvider.GetService<BlockListValueParser>();
                    case ParserType.String:
                        return serviceProvider.GetService<StringValueParser>();
                    default:
                        throw new KeyNotFoundException($"Could not get Preflight parser for {key}");
                }
            });
        }
    }
#else
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreflightParserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IValueParserService, ValueParserService>();

            composition.Register<NestedContentValueParser>();
            composition.Register<GridValueParser>();
            composition.Register<BlockListValueParser>();
            composition.Register<StringValueParser>();
            composition.Register<Func<ParserType, IPreflightValueParser>>(serviceProvider => key =>
            {
                switch (key)
                {
                    case ParserType.NestedContent:
                        return serviceProvider.GetInstance<NestedContentValueParser>();
                    case ParserType.Grid:
                        return serviceProvider.GetInstance<GridValueParser>();
                    case ParserType.BlockList:
                        return serviceProvider.GetInstance<BlockListValueParser>();
                    case ParserType.String:
                        return serviceProvider.GetInstance<StringValueParser>();
                    default:
                        throw new KeyNotFoundException($"Could not get Preflight parser for {key}");
                }
            });
        }
    }
#endif
}

