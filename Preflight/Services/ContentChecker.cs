using System;
using Newtonsoft.Json.Linq;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.SignalR;
using Preflight.Extensions;
using Preflight.Plugins;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Preflight.Services
{
    /// <summary>
    /// Where the magic happens. ContentChecker extracts property values and passes them into the set of plugins for testing
    /// </summary>
    internal class ContentChecker : IContentChecker
    {
        private readonly ISettingsService _settingsService;
        private readonly IHubContext _hubContext;

        private int _id;
        private bool _fromSave;
        private List<SettingsModel> _settings;

        public ContentChecker(ISettingsService settingsService)
        {
            _settingsService = settingsService;
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<PreflightHub>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="dirtyProperties"></param>
        public bool CheckDirty(DirtyProperties dirtyProperties)
        {
            _id = dirtyProperties.Id;
            _settings = _settingsService.Get().Settings;

            var failed = false;

            foreach (SimpleProperty prop in dirtyProperties.Properties)
            {
                List<PreflightPropertyResponseModel> testResult = new List<PreflightPropertyResponseModel>();

                switch (prop.Editor)
                {
                    case KnownPropertyAlias.Grid:
                        testResult = ExtractValuesFromSimpleProperty(prop, KnownStrings.RteJsonPath);
                        break;
                    case KnownPropertyAlias.Rte:
                    case KnownPropertyAlias.Textarea:
                    case KnownPropertyAlias.Textbox:
                        testResult = RunPluginsAgainstValue(prop.Name, prop.Value).AsEnumerableOfOne().ToList();
                        break;
                }

                foreach (PreflightPropertyResponseModel result in testResult)
                {
                    if (result.Failed)
                    {
                        failed = true;
                    }
                    _hubContext.Clients.All.PreflightTest(result);
                }
            }

            return failed;
        }


        /// <summary>
        /// Checks all testable properties on the given IContent item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fromSave"></param>
        /// <returns></returns>
        public bool CheckContent(IContent content, bool fromSave)
        {
            // make this available to pass into any plugins
            _id = content.Id;
            _fromSave = fromSave;
            _settings = _settingsService.Get().Settings;

            var failed = false;

            IEnumerable<Property> props = content.GetPreflightProperties();
            foreach (Property prop in props)
            {
                List<PreflightPropertyResponseModel> testResult = new List<PreflightPropertyResponseModel>();

                // since the first two can return multiple, treat all three the same
                switch (prop.PropertyType.PropertyEditorAlias)
                {
                    case KnownPropertyAlias.Grid:
                        testResult = ExtractValuesFromUmbracoProperty(prop, KnownStrings.RteJsonPath);
                        break;
                    case KnownPropertyAlias.Rte:
                    case KnownPropertyAlias.Textarea:
                    case KnownPropertyAlias.Textbox:
                        testResult = RunPluginsAgainstValue(prop.PropertyType.Name, prop.GetValue()?
                            .ToString()).AsEnumerableOfOne().ToList();
                        break;
                }

                // return the results via signalr for perceived perf
                foreach (PreflightPropertyResponseModel result in testResult)
                {
                    if (result.Failed)
                    {
                        failed = true;
                    }
                    _hubContext.Clients.All.PreflightTest(result);
                }
            }

            return failed;
        }


        /// <summary>
        /// Extracts the testable values from a <see cref="SimpleProperty"/> and passes each to <see cref="ProcessJTokenValues" />
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="editorPath"></param>
        /// <returns></returns>
        private List<PreflightPropertyResponseModel> ExtractValuesFromSimpleProperty(SimpleProperty prop, string editorPath)
        {
            if (prop.Value == null)
            {
                return new List<PreflightPropertyResponseModel>();
            }

            JObject asJson = JObject.Parse(prop.Value);
            IEnumerable<JToken> rtes = asJson.SelectTokens(editorPath);

            return ProcessJTokenValues(rtes, prop.Name);
        }


        /// <summary>
        /// Extracts the testable values from a single Property, and passes each to <see cref="ProcessJTokenValues" />
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="editorPath"></param>
        /// <returns></returns>
        private List<PreflightPropertyResponseModel> ExtractValuesFromUmbracoProperty(Property prop, string editorPath)
        {
            object propValue = prop.GetValue();

            if (propValue == null)
            {
                return new List<PreflightPropertyResponseModel>();
            }

            JObject asJson = JObject.Parse(propValue.ToString());
            IEnumerable<JToken> rtes = asJson.SelectTokens(editorPath);

            return ProcessJTokenValues(rtes, prop.PropertyType.Name);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="rtes"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private List<PreflightPropertyResponseModel> ProcessJTokenValues(IEnumerable<JToken> rtes, string name)
        {
            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();
            var index = 1;

            foreach (JToken rte in rtes)
            {
                JToken value = rte.SelectToken(KnownStrings.RteValueJsonPath);
                if (value == null) continue;

                PreflightPropertyResponseModel model = RunPluginsAgainstValue(name, value.ToString());

                // todo => extract the grid section, area, editor names for building the alias. Won't work for archetype though
                model.Label = $"{model.Name} (Editor {index})";
                index += 1;

                response.Add(model);
            }

            return response;
        }


        /// <summary>
        /// Runs the set of plugins against the given string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private PreflightPropertyResponseModel RunPluginsAgainstValue(string name, string val)
        {
            var model = new PreflightPropertyResponseModel
            {
                Label = name,
                Name = name
            };

            if (val == null)
                return model;

            var pluginProvider = new PluginProvider();

            foreach (IPreflightPlugin plugin in pluginProvider.Get())
            {
                // settings on the plugin are the defaults - set to correct values from _settings
                IEnumerable<SettingsModel> pluginSettings = _settings.Where(s => s.Tab == plugin.Name).ToList();
                plugin.Settings = pluginSettings;

                // ignore disabled plugins
                if (plugin.IsDisabled()) continue;
                if(!_fromSave && plugin.IsOnSaveOnly()) continue;

                try
                {
                    Type pluginType = plugin.GetType();
                    if (pluginType.GetMethod("Check") == null) continue;

                    plugin.Check(_id, val, _settings);

                    if (plugin.Result != null)
                    {
                        if (plugin.FailedCount == 0)
                        {
                            plugin.FailedCount = plugin.Failed ? 1 : 0;
                        }

                        model.Plugins.Add(plugin);
                    }
                }
                catch (Exception e)
                {
                    // todo => log
                    string m = e.Message;
                }
            }

            // mark as failed if any sub-tests have failed
            model.FailedCount = model.Plugins.Sum(x => x.FailedCount);
            model.Failed = model.FailedCount > 0;

            model.Plugins = model.Plugins.OrderBy(p => p.SortOrder).ToList();

            return model;
        }
    }
}
