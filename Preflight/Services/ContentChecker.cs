using System;
using Newtonsoft.Json.Linq;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Preflight.Extensions;
using Preflight.Plugins;
using Umbraco.Core.Models;

namespace Preflight.Services
{
    internal class ContentChecker : IContentChecker
    {
        private readonly IReadabilityService _readabilityService;
        private readonly ISafeBrowsingService _safeBrowsingService;
        private readonly ILinksService _linksService;

        private readonly List<string> _added;
        private readonly List<SettingsModel> _settings;

        private readonly bool _checkLinks;
        private readonly bool _checkReadability;
        private readonly bool _checkSafeBrowsing;

        private int _id;

        private readonly string _apiKey;


        public ContentChecker(IReadabilityService readabilityService, ILinksService linksService, 
            ISafeBrowsingService safeBrowsingService, ISettingsService settingsService)
        {
            _readabilityService = readabilityService;
            _linksService = linksService;
            _safeBrowsingService = safeBrowsingService;

            _added = new List<string>();
            
            _settings = settingsService.Get().Settings;

            _checkLinks = _settings.GetValue<bool>(KnownSettings.CheckLinks);
            _checkReadability = _settings.GetValue<bool>(KnownSettings.CheckReadability);
            _checkSafeBrowsing = _settings.GetValue<bool>(KnownSettings.EnsureSafeLinks);
            _apiKey = _settings.GetValue<string>(KnownSettings.GoogleApiKey);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public PreflightResponseModel Check(IContent content)
        {
            // make this available to pass into any plugins
            _id = content.Id;

            IEnumerable<Property> props = content.GetPreflightProperties();

            var response = new PreflightResponseModel
            {
                CheckLinks = _checkLinks,
                CheckReadability = _checkReadability,
                CheckSafeBrowsing = _checkSafeBrowsing,
                HideDisabled = _settings.GetValue<bool>(KnownSettings.HideDisabled),
                CancelSaveOnFail = _settings.GetValue<bool>(KnownSettings.CancelSaveOnFail)
            };

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
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public IContent Autoreplace(IContent content)
        {
            // perform autoreplace before readability check
            // only do this in save handler as there's no point in updating if it's not being saved (potentially)
            Dictionary<string, string> autoreplace = _settings.GetValue<string>(KnownSettings.AutoreplaceTerms)?.Split(',')
                .ToDictionary(
                    s => s.Split('|')[0], 
                    s => s.Split('|')[1]
                );

            if (autoreplace == null || !autoreplace.Any()) return content;

            IEnumerable<Property> props = content.GetPreflightProperties();

            foreach (Property prop in props)
            {
                foreach (KeyValuePair<string, string> term in autoreplace)
                {
                    string pattern = $@"\b{term.Key}\b";
                    prop.SetValue(Regex.Replace(prop.GetValue().ToString(), pattern, term.Value, RegexOptions.IgnoreCase));
                }
            }

            return content;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="editorPath"></param>
        /// <returns></returns>
        private IEnumerable<PreflightPropertyResponseModel> CheckNestedEditor(Property prop, string editorPath)
        {
            object propValue = prop.GetValue();

            if (propValue == null)
            {
                return null;
            }

            JObject asJson = JObject.Parse(propValue.ToString());
            IEnumerable<JToken> rtes = asJson.SelectTokens(editorPath);

            string name = prop.PropertyType.Name;

            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            foreach (JToken rte in rtes)
            {
                JToken value = rte.SelectToken(KnownStrings.RteValueJsonPath);
                if (value == null) continue;

                PreflightPropertyResponseModel model = CheckProperty(SetName(name), value.ToString());

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
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private PreflightPropertyResponseModel CheckProperty(string name, string val)
        {
            ReadabilityResponseModel readability = _checkReadability ? _readabilityService.Check(val, _settings) : new ReadabilityResponseModel();
            List<BrokenLinkModel> safeBrowsing = _checkSafeBrowsing ? _safeBrowsingService.Check(val, _apiKey) : new List<BrokenLinkModel>();
            List<BrokenLinkModel> links = _checkLinks ? _linksService.Check(val, safeBrowsing) : new List<BrokenLinkModel>();

            var model = new PreflightPropertyResponseModel
            {
                Name = name,
                Readability = readability,
                Links = links,
                SafeBrowsing = safeBrowsing
            };

            // this is a POC and should be refactored
            foreach (PreflightPlugin plugin in PluginsHelper.GetPlugins())
            {
                // ignore disabled plugins
                if (plugin.Settings.Any(s =>
                    s.Alias == plugin.Name.DisabledAlias() && s.Value.ToString() == "1")) continue;

                try
                {
                    if (!plugin.Name.HasValue()) continue;

                    plugin.Result = plugin.Check(_id, val, out bool failed);

                    if (plugin.Result != null)
                    {
                        plugin.Failed = failed;
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
            model.Failed = _checkReadability && readability.Failed || 
                           links.Any() || 
                           safeBrowsing.Any() || 
                           (bool) model.Plugins?.Any(x => x.Failed);

            // counting failed is a bit messy - check all core test for failures, then count failed plugins
            // ultimately, core tests should use plugin structure but set core=true
            var failedCount = 0;
            if (_checkReadability && readability.Failed)
            {
                failedCount += readability.Blacklist.Any() ? 1 : 0;
                failedCount += readability.FailedReadability ? 1 : 0;
                failedCount += readability.LongWords.Any() ? 1 : 0;
            }

            failedCount += links.Any() ? 1 : 0;
            failedCount += safeBrowsing.Any() ? 1 : 0;
            failedCount += model.Plugins.Count(x => x.Failed);

            model.FailedCount = failedCount;

            return model;
        }

        /// <summary>
        /// Helper for formatting the property name in the response
        /// </summary>
        /// <param name="name">Name of the current property</param>
        /// <returns></returns>
        private string SetName(string name)
        {
            string response = _added.IndexOf(name) != -1 ? $"{name} (Editor {_added.IndexOf(name) + 2})" : name;
            _added.Add(name);

            return response;
        }
    }
}
