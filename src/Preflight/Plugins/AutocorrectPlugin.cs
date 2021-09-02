using Preflight.Extensions;
using Preflight.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
#if NETCOREAPP
using CharArrays = Umbraco.Cms.Core.Constants.CharArrays;
#else
using CharArrays = Umbraco.Core.Constants.CharArrays;
#endif

namespace Preflight.Plugins
{
    public class AutocorrectPlugin : IPreflightCorePlugin
    {
        public object Result { get; set; }
        public IEnumerable<SettingsModel> Settings { get; set; }

        public bool Failed { get; set; }
        public bool Core => true;

        public int SortOrder => -1;
        public int FailedCount { get; set; }

        public string Name => "Autocorrect";
        public string ViewPath => "";
        public string Summary => "Automatically replace naughty words with less naughty words, or fix common spelling mistakes.";
        public string Description { get; set; }
        public int TotalTests => 1;

        /// <summary>
        /// 
        /// </summary>
        public AutocorrectPlugin()
        {
            Settings = PluginSettingsList.Populate(Name,
                false,
                true,
                new[] { "7d34c7dd-0167-42c9-a1a4-f7245d5e555a", "9665c018-80be-402f-890a-4bb7f56deaac", "8a567a58-5417-4562-a1b9-12f1e80c8dbb" },
                settings: new SettingsModel[] {
                    new GenericSettingModel("Autocorrect terms")
                    {
                        Description = "Pipe-separated list of terms to autocorrect in Preflight checks - eg 'replace me|new text'",
                        View = SettingType.MultipleTextbox,
                        Value = "replacethis|new term",
                        Order = 1,
                        Core = true,
                        Guid = new Guid(KnownSettings.AutocorrectTerms),
                    }
                }
            );
        }

        /// <summary>
        /// perform autocorrect before readability check
        /// only do this in save handler as there's no point in updating if it's not being saved (potentially)
        /// </summary>
        /// <param name="id"></param>
        /// <param name="val"></param>
        /// <param name="settings"></param>
        public void Check(int id, string culture, string val, List<SettingsModel> settings)
        {
            Dictionary<string, string> autocorrect = settings.GetValue<string>(KnownSettings.AutocorrectTerms, culture)?.Split(CharArrays.Comma)
                .ToDictionary(
                    s => s.Split(CharArrays.VerticalTab)[0],
                    s => s.Split(CharArrays.VerticalTab)[1]
                );

            if (autocorrect == null || !autocorrect.Any())
                return;

            foreach (KeyValuePair<string, string> term in autocorrect)
            {
                string pattern = $@"\b{term.Key}\b";
                Regex.Replace(val, pattern, term.Value, RegexOptions.IgnoreCase);
            }
        }
    }
}
