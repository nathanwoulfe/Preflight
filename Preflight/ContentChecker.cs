using System;
using Newtonsoft.Json.Linq;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Services;
using Preflight.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Preflight.Extensions;
using Preflight.Plugins;
using Umbraco.Core.Models;

namespace Preflight
{
    public class ContentChecker
    {
        private readonly IReadabilityService _readabilityService;
        private readonly ISafeBrowsingService _safeBrowsingService;
        private readonly ILinksService _linksService;

        private readonly List<string> _added;
        private readonly List<SettingsModel> _settings;

        private readonly bool _checkLinks;
        private readonly bool _checkReadability;
        private readonly bool _checkSafeBrowsing;

        private readonly string _apiKey;

        public ContentChecker() : this(new ReadabilityService(), new LinksService(), new SafeBrowsingService(), 
            new SettingsService(), new List<string>())
        {
        }

        private ContentChecker(IReadabilityService readabilityService, ILinksService linksService, 
            ISafeBrowsingService safeBrowsingService, ISettingsService settingsService, List<string> added)
        {
            _readabilityService = readabilityService;
            _linksService = linksService;
            _safeBrowsingService = safeBrowsingService;

            _added = added;
            
            _settings = settingsService.Get().Settings;

            _checkLinks = _settings.Any(s => s.Alias == KnownSettings.CheckLinks.Camel() && s.Value.ToString() == "1");
            _checkReadability = _settings.Any(s => s.Alias == KnownSettings.CheckReadability.Camel() && s.Value.ToString() == "1");
            _checkSafeBrowsing = _settings.Any(s => s.Alias == KnownSettings.EnsureSafeLinks.Camel() && s.Value.ToString() == "1");
            _apiKey = _settings.First(s => s.Alias == KnownSettings.GoogleApiKey.Camel()).Value?.ToString();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public PreflightResponseModel Check(IContent content)
        {
            IEnumerable<Property> props = GetProperties(content);

            var response = new PreflightResponseModel
            {
                CheckLinks = _checkLinks,
                CheckReadability = _checkReadability,
                CheckSafeBrowsing = _checkSafeBrowsing,
                HideDisabled = _settings.Any(s => s.Alias == KnownSettings.HideDisabled.Camel() && s.Value.ToString() == "1"),
                CancelSaveOnFail = _settings.Any(s => s.Alias == KnownSettings.CancelSaveOnFail.Camel() && s.Value.ToString() == "1")
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
            Dictionary<string, string> autoreplace = ((string)_settings.First(s => s.Alias == KnownSettings.AutoreplaceTerms.Camel()).Value)
                .Split(',').ToDictionary(s => s.Split('|')[0], s => s.Split('|')[1]);

            if (!autoreplace.Any()) return content;

            IEnumerable<Property> props = GetProperties(content);

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

                PreflightPropertyResponseModel model = Check(SetName(name), value.ToString());

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
            return propValue == null ? null : Check(prop.PropertyType.Name, propValue.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        private PreflightPropertyResponseModel Check(string name, string val)
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
                    s.Alias == $"{plugin.Name} disabled".Camel() && s.Value.ToString() == "1")) continue;

                try
                {
                    if (!plugin.Name.HasValue()) continue;

                    plugin.Result = plugin.Check(val, out bool failed);

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

            return model;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static IEnumerable<Property> GetProperties(IContentBase content)
        {
            return content.Properties
                .Where(p => p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Grid ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Archetype ||
                            p.PropertyType.PropertyEditorAlias == KnownPropertyAlias.Rte);
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
