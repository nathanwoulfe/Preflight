using Newtonsoft.Json.Linq;
using Preflight.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Preflight.Services;
using Preflight.Services.Interfaces;
using Umbraco.Core.Models;

namespace Preflight
{
    public class ContentChecker
    {
        private readonly IReadabilityService _readabilityService;
        private readonly ILinksService _linksService;

        private readonly List<string> _added;

        private readonly List<SettingsModel> _settings;

        private readonly bool _checkLinks;
        private readonly bool _checkReadability;
        private readonly bool _checkSafeBrowsing;

        private readonly string _apiKey;

        public ContentChecker() : this(new ReadabilityService(), new LinksService(), new SettingsService(), new List<string>())
        {
        }

        private ContentChecker(IReadabilityService readabilityService, ILinksService linksService, ISettingsService settingsService, List<string> added)
        {
            _readabilityService = readabilityService;
            _linksService = linksService;

            _added = added;

            _settings = settingsService.Get();

            _checkLinks = _settings.Any(s => s.Alias == Constants.CheckLinksAlias && s.Value.ToString() == "1");
            _checkReadability = _settings.Any(s => s.Alias == Constants.CheckReadabilityAlias && s.Value.ToString() == "1");
            _checkSafeBrowsing = _settings.Any(s => s.Alias == Constants.CheckSafeBrowsingAlias && s.Value.ToString() == "1");
            _apiKey = _settings.First(s => s.Alias == KnownSettingAlias.GoogleApiKey).Value.ToString();
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
                CheckSafeBrowsing = _checkSafeBrowsing
            };

            foreach (Property prop in props)
            {
                switch (prop.PropertyType.PropertyEditorAlias)
                {
                    case KnownPropertyAlias.Grid:
                        response.Properties.AddRange(CheckNestedEditor(prop, Constants.RteJsonPath));
                        break;
                    case KnownPropertyAlias.Archetype:
                        response.Properties.AddRange(CheckNestedEditor(prop, Constants.ArchetypeRteJsonPath));
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
            Dictionary<string, string> autoreplace = ((string)_settings.First(s => s.Alias == KnownSettingAlias.Autoreplace).Value)
                .Split(',').ToDictionary(s => s.Split('|')[0], s => s.Split('|')[1]);

            if (!autoreplace.Any()) return content;

            IEnumerable<Property> props = GetProperties(content);

            foreach (Property prop in props)
            {
                foreach (KeyValuePair<string, string> term in autoreplace)
                {
                    string pattern = $@"\b{term.Key}\b";
                    prop.Value = Regex.Replace(prop.Value.ToString(), pattern, term.Value, RegexOptions.IgnoreCase);
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
            if (prop.Value == null)
            {
                return null;
            }

            JObject asJson = JObject.Parse(prop.Value.ToString());
            IEnumerable<JToken> rtes = asJson.SelectTokens(editorPath);

            string name = prop.PropertyType.Name;

            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            foreach (JToken rte in rtes)
            {
                JToken value = rte.SelectToken(Constants.RteValueJsonPath);
                if (value == null) continue;

                string val = value.ToString();

                ReadabilityResponseModel readability = _checkReadability ? _readabilityService.Check(val, _settings) : new ReadabilityResponseModel();
                List<BrokenLinkModel> links = _linksService.Check(val, _checkLinks, _checkSafeBrowsing, _apiKey);

                response.Add(new PreflightPropertyResponseModel
                {
                    Name = SetName(name),
                    Readability = readability,
                    Links = links,
                    Failed = _checkReadability && readability.Failed || links.Any()
                });
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
            if (prop.Value == null)
            {
                return null;
            }

            string val = prop.Value.ToString();

            ReadabilityResponseModel readability = _checkReadability ? _readabilityService.Check(val, _settings) : new ReadabilityResponseModel();
            List<BrokenLinkModel> links = _linksService.Check(val, _checkLinks, _checkSafeBrowsing, _apiKey);

            return new PreflightPropertyResponseModel
            {
                Name = prop.PropertyType.Name,
                Readability = readability,
                Links = links,
                Failed = _checkReadability && readability.Failed || links.Any()
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        private static IEnumerable<Property> GetProperties(IContent content)
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
            string response = _added.IndexOf(name) != -1 ? name + " (Editor " + (_added.IndexOf(name) + 2) + ")" : name;
            _added.Add(name);

            return response;
        }
    }
}
