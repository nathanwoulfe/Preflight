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
        /// <param name="properties"></param>
        public int CheckDirty(IEnumerable<SimpleProperty> properties)
        {
            _settings = _settingsService.Get().Settings;

            var totalTests = 0;

            foreach (SimpleProperty prop in properties)
            {
                List<PreflightPropertyResponseModel> testResult = new List<PreflightPropertyResponseModel>();

                switch (prop.Editor)
                {
                    case KnownPropertyAlias.Grid:
                        testResult = CheckNestedProperty(prop, KnownStrings.RteJsonPath);
                        break;
                    case KnownPropertyAlias.Archetype:
                        testResult = CheckNestedProperty(prop, KnownStrings.ArchetypeRteJsonPath);
                        break;
                    case KnownPropertyAlias.Rte:
                        testResult = CheckProperty(prop.Name, prop.Value).AsEnumerableOfOne().ToList();
                        break;
                }

                foreach (PreflightPropertyResponseModel result in testResult)
                {
                    _hubContext.Clients.All.PreflightTest(result);
                }

                totalTests += testResult.Count;
            }

            return totalTests;
        }

        /// <summary>
        /// Checks all testable properties on the given IContent item
        /// </summary>
        /// <param name="content"></param>
        /// <param name="fromSave"></param>
        /// <returns></returns>
        public PreflightResponseModel Check(IContent content, bool fromSave)
        {
            // make this available to pass into any plugins
            _id = content.Id;
            _fromSave = fromSave;
            _settings = _settingsService.Get().Settings;

            var response = new PreflightResponseModel
            {
                CancelSaveOnFail = _settings.GetValue<bool>(KnownSettings.CancelSaveOnFail)
            };

            IEnumerable<Property> props = content.GetPreflightProperties();
            foreach (Property prop in props)
            {
                switch (prop.PropertyType.PropertyEditorAlias)
                {
                    case KnownPropertyAlias.Grid:
                        response.Properties.AddRange(CheckNestedEditor(prop, KnownStrings.RteJsonPath));
                        break;
                    case KnownPropertyAlias.Archetype:
                        response.Properties.AddRange(CheckNestedEditor(prop, KnownStrings.ArchetypeRteJsonPath));
                        break;
                    case KnownPropertyAlias.Rte:
                        response.Properties.Add(CheckSingleEditor(prop));
                        break;
                }
            }

            response.Failed = response.Properties.Any(p => p.Failed);

            if (!response.Failed) return response;

            response.FailedCount = response.Properties.Sum(p => p.FailedCount);

            return response;
        }


        /// <summary>
        /// Extracts the testable values from a <see cref="SimpleProperty"/> and passes each to CheckProperty 
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="editorPath"></param>
        /// <returns></returns>
        private List<PreflightPropertyResponseModel> CheckNestedProperty(SimpleProperty prop, string editorPath)
        {
            JObject asJson = JObject.Parse(prop.Value);
            IEnumerable<JToken> rtes = asJson.SelectTokens(editorPath);

            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();
            var index = 1;

            foreach (JToken rte in rtes)
            {
                JToken value = rte.SelectToken(KnownStrings.RteValueJsonPath);
                if (value == null) continue;

                PreflightPropertyResponseModel model = CheckProperty(prop.Name, value.ToString());

                model.Label = $"{model.Name} (Editor {index})";
                index += 1;

                response.Add(model);
            }

            return response;
        }

        /// <summary>
        /// Extracts the testable values from a single Property, and passes each to CheckProperty
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="editorPath"></param>
        /// <returns></returns>
        private IEnumerable<PreflightPropertyResponseModel> CheckNestedEditor(Property prop, string editorPath)
        {
            object propValue = prop.GetValue();

            if (propValue == null)
            {
                return new List<PreflightPropertyResponseModel>();
            }

            JObject asJson = JObject.Parse(propValue.ToString());
            IEnumerable<JToken> rtes = asJson.SelectTokens(editorPath);

            string name = prop.PropertyType.Name;

            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();
            var index = 1;

            foreach (JToken rte in rtes)
            {
                JToken value = rte.SelectToken(KnownStrings.RteValueJsonPath);
                if (value == null) continue;

                PreflightPropertyResponseModel model = CheckProperty(name, value.ToString());
                
                model.Label = $"{model.Name} (Editor {index})";
                index += 1;

                response.Add(model);
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prop"></param>
        /// <returns></returns>
        private PreflightPropertyResponseModel CheckSingleEditor(Property prop)
        {
            object propValue = prop.GetValue();
            return propValue == null ? null : CheckProperty(prop.PropertyType.Name, propValue.ToString());
        }

        /// <summary>
        /// Runs the set of plugins against the given string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private PreflightPropertyResponseModel CheckProperty(string name, string val)
        {
            var model = new PreflightPropertyResponseModel
            {
                Label = name,
                Name = name
            };

            var pluginProvider = new PluginProvider();

            foreach (IPreflightPlugin plugin in pluginProvider.Get())
            {
                // settings on the plugin are the defaults - set to correct values from _settings
                IEnumerable<SettingsModel> pluginSettings = _settings.Where(s => s.Tab == plugin.Name).ToList();
                plugin.Settings = pluginSettings;

                //foreach (SettingsModel setting in plugin.Settings)
                //{
                //    setting.Value = pluginSettings.First(s => s.Alias == setting.Alias).Value;
                //}

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
