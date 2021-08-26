using Preflight.Extensions;
using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
#else
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using IProperty = Umbraco.Core.Models.Property;
#endif

namespace Preflight.Services.Implement
{
    /// <summary>
    /// Where the magic happens. ContentChecker extracts property values and passes them into the set of plugins for testing
    /// </summary>
    internal class ContentChecker : IContentChecker
    {
        private readonly IContentService _contentService;
        private readonly IMessenger _messenger;
        private readonly IValueParserService _valueParserService;

        private int _id;
        private bool _fromSave;

        public ContentChecker(IContentService contentService, IMessenger messenger, IValueParserService valueParserService)
        {
            _contentService = contentService ?? throw new ArgumentNullException(nameof(contentService));
            _messenger = messenger ?? throw new ArgumentNullException(nameof(messenger));
            _valueParserService = valueParserService ?? throw new ArgumentNullException(nameof(valueParserService));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirtyProperties"></param>
        public bool CheckDirty(DirtyProperties dirtyProperties)
        {
            _id = dirtyProperties.Id;
            _fromSave = false;

            var failed = false;

            foreach (SimpleProperty prop in dirtyProperties.Properties)
            {
                string propValue = prop.Value?.ToString();

                // only continue if the prop has a value
                if (!propValue.HasValue())
                {
                    _messenger.SendTestResult(new PreflightPropertyResponseModel
                    {
                        Name = prop.Name,
                        Remove = true
                    });

                    continue;
                }

                failed = TestAndBroadcast(prop.Name, dirtyProperties.Culture, propValue, prop.Editor) || failed;
            }

            _messenger.PreflightComplete();

            return failed;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="culture"></param>
        /// <param name="fromSave"></param>
        /// <returns></returns>
        public bool CheckContent(int id, string culture, bool fromSave) => CheckContent(_contentService.GetById(id), culture, fromSave);


        /// <summary>
        /// Checks all testable properties on the given IContent item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="culture"></param>
        /// <param name="fromSave"></param>
        /// <returns></returns>
        public bool CheckContent(IContent content, string culture, bool fromSave)
        {
            _id = content.Id;
            _fromSave = fromSave;

            var failed = false;

            IEnumerable<IProperty> props = content.GetPreflightProperties();

            foreach (IProperty prop in props)
            {
                string propValue = (prop.GetValue(culture) ?? prop.GetValue())?.ToString();

                // only continue if the prop has a value
                if (!propValue.HasValue())
                {
                    _messenger.SendTestResult(new PreflightPropertyResponseModel
                    {
                        Name = prop.PropertyType.Name,
                        Remove = true
                    });

                    continue;
                }

                failed = TestAndBroadcast(prop.PropertyType.Name, culture, propValue, prop.PropertyType.PropertyEditorAlias) || failed;
            }

            _messenger.PreflightComplete();

            return failed;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        private bool TestAndBroadcast(string name, string culture, string value, string alias)
        {
            List<PreflightPropertyResponseModel> testResult = new List<PreflightPropertyResponseModel>();

            bool failed = false;

            switch (alias)
            {
                case KnownPropertyAlias.NestedContent:
                    testResult = _valueParserService.ParseNestedContent(name, value, culture, _id, _fromSave);
                    break;
                case KnownPropertyAlias.Grid:
                    testResult = _valueParserService.ParseGridContent(name, value, culture, _id, _fromSave);
                    break;
                case KnownPropertyAlias.BlockList:
                    testResult = _valueParserService.ParseBlockListContent(name, value, culture, _id, _fromSave);
                    break;
                case KnownPropertyAlias.Rte:
                case KnownPropertyAlias.Textarea:
                case KnownPropertyAlias.Textbox:
                    testResult = new[] { _valueParserService.ParseStringContent(name, value, culture, alias, _id, _fromSave) }.ToList();
                    break;
            }

            // return the results via signalr for perceived perf
            foreach (PreflightPropertyResponseModel result in testResult)
            {
                // ignore results where no plugins ran
                if (result.Plugins.Count > 0)
                {
                    if (result.Failed)
                    {
                        failed = true;
                    }

                    // announce the result
                    _messenger.SendTestResult(result);
                }
            }

            return failed;
        }     
    }
}
