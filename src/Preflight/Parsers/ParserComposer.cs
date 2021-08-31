using System;
using System.Collections.Generic;
using Preflight.Extensions;
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
            builder.Services.AddTransient<NestedContentValueParser>();
            builder.Services.AddTransient<GridValueParser>();
            builder.Services.AddTransient<BlockListValueParser>();
            builder.Services.AddTransient<StringValueParser>();
            builder.Services.AddTransient<Func<ParserType, IPreflightValueParser>>(serviceProvider => key => GetServiceInstance(serviceProvider, key));
        }

        public IPreflightValueParser GetServiceInstance(IServiceProvider serviceProvider, ParserType key)
        {
            Type serviceType = key.GetAttribute<ParserTypeInfoAttribute>()?.ServiceType;

            if (serviceType == null)
                throw new KeyNotFoundException($"Could not get Preflight parser for {key}");

            var service = serviceProvider.GetService(serviceType);

            if (service is IPreflightValueParser parser)
                return parser;

            throw new KeyNotFoundException($"Could not get Preflight parser for {key}");
        }
    }
#else
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreflightParserComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<NestedContentValueParser>();
            composition.Register<GridValueParser>(); 
            composition.Register<BlockListValueParser>();
            composition.Register<StringValueParser>();

            composition.Register<Func<ParserType, IPreflightValueParser>>(serviceProvider => key => GetServiceInstance(serviceProvider, key));
        }

        public IPreflightValueParser GetServiceInstance(IFactory serviceProvider, ParserType key)
        {
            Type serviceType = key.GetAttribute<ParserTypeInfoAttribute>()?.ServiceType;

            if (serviceType == null)
                throw new KeyNotFoundException($"Could not get Preflight parser for {key}");

            var service = serviceProvider.GetInstance(serviceType);

            if (service is IPreflightValueParser parser)
                return parser;

            throw new KeyNotFoundException($"Could not get Preflight parser for {key}");
        }
    }
#endif
}

