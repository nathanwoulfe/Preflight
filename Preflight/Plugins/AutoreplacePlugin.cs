using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using Preflight.Constants;
using Preflight.Extensions;
using Preflight.Models;

namespace Preflight.Plugins
{
    [Export(typeof(IPreflightPlugin))]
    public class AutoreplacePlugin : IPreflightCorePlugin
    {
        public object Result { get; set; }
        public IEnumerable<SettingsModel> Settings { get; set; }

        public bool Failed { get; set; }
        public bool Core => true;

        public int SortOrder => -1;
        public int FailedCount { get; set; }

        public string Name => "Autoreplace";
        public string ViewPath => "";

        private AutoreplacePlugin()
        {
            Settings = PluginSettingsList.Populate(Name,
                false,
                true,
                new GenericSettingModel("Autoreplace terms")
                {
                    Description = "Pipe-separated list of terms to auto-replace in preflight checks - eg 'replace me|new text'.",
                    View = SettingType.MultipleTextbox,
                    Value = "replacethis|new term",
                    Order = 1,
                    Core = true
                }
            );
        }

        /// <summary>
        /// perform autoreplace before readability check
        /// only do this in save handler as there's no point in updating if it's not being saved (potentially)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        /// <param name="settings"></param>
        public void Check(int id, string val, List<SettingsModel> settings)
        {
            Dictionary<string, string> autoreplace = settings.GetValue<string>(KnownSettings.AutoreplaceTerms)?.Split(',')
                .ToDictionary(
                    s => s.Split('|')[0],
                    s => s.Split('|')[1]
                );

            if (autoreplace == null || !autoreplace.Any())
                return;

            foreach (KeyValuePair<string, string> term in autoreplace)
            {
                string pattern = $@"\b{term.Key}\b";
                Regex.Replace(val, pattern, term.Value, RegexOptions.IgnoreCase);
            }
        }
    }
}
