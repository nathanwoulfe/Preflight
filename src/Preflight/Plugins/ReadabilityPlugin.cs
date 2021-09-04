using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Preflight.Plugins
{
    public class ReadabilityPlugin : IPreflightCorePlugin
    {
        private readonly IReadabilityService _readabilityService;

        public string DisabledSettingIdentifier => "dfeab01a-e5c0-49ed-a018-9f1c546dfe10";
        public string OnSaveOnlySettingIdentifier => "1485f35b-4c2e-4a53-a0b2-fefa02bcf810";
        public string PropertiesToTestSettingIdentifier => "3984cf76-b598-4bb8-b073-698f39111f7c";
        public object Result { get; set; }

        public IEnumerable<SettingsModel> Settings { get; set; }
        public string Description { get; set; }

        public bool Failed { get; set; }
        public bool Core => true;

        public int SortOrder => -2;
        public int FailedCount { get; set; }

        public int TotalTests => 3;

        public string Name => "Readability";
        public string Summary => "Ensure content meets minimum readability measures, using the Flesch reading ease algorithm.";
        public string ViewPath => "/App_Plugins/Preflight/Backoffice/plugins/readability/readability.html";

        /// <summary>
        /// 
        /// </summary>
        /// <param name="readabilityService"></param>
        public ReadabilityPlugin(IReadabilityService readabilityService)
        {
            _readabilityService = readabilityService;

            this.GenerateDefaultSettings(false, false,
                settings: new SettingsModel[] {
                    new GenericSettingModel("Readability target - minimum", new Guid(KnownSettings.ReadabilityMin))
                    {
                        Value = "60",
                        Description = "Readability result must be great than this value",
                        View = SettingType.Slider,
                        Order = 1,
                        Core = true,
                    },
                    new GenericSettingModel("Readability target - maximum", new Guid(KnownSettings.ReadabilityMax))
                    {
                        Value = "100",
                        Description = "Readability result must be less than this value",
                        View = SettingType.Slider,
                        Order = 2,
                        Core = true,
                    },
                    new GenericSettingModel("Long word syllable count", new Guid(KnownSettings.LongWordSyllables))
                    {
                        Value = "5",
                        Description =
                            "Words in text will be flagged as long, if their syllable count is equal to or greater than this value",
                        View = SettingType.Slider,
                        Order = 3,
                        Core = true,
                    }
                }
            );

            Description = @"<p>If your content is too difficult for your visitors to read, you're all going to have a bad time.</p>
                <p>The readability test runs your content through the Flesch reading ease algorithm to determine text complexity.</p>
                <h5>The algorithm</h5>
                <p><code>RE = 206.835 - (1.015 x ASL) - (84.6 x ASW)</code></p>
                <p>Where <code>RE</code> is Readability Ease, <code>ASL</code> is Average Sentence Length, and <code>ASW</code> is Average Syllables per Word</p>
                <p>The result is a number between 0 and 100, where a higher score means better readability, with a score between 60 and 69 largely considered acceptable.</p>
                <h5>Readability test results</h5>
                <p>As well as the Flesch score, the readability test returns sentence length; average syllables per word; and long or complex words.</p>";
        }

        public void Check(int id, string culture, string val, List<SettingsModel> settings)
        {
            // must get a result of any type
            ReadabilityResponseModel result = _readabilityService.Check(val, culture, settings);
            // then set Failed
            Failed = result.Failed;
            // and set Result
            Result = result;

            // finally, tally failed tests
            FailedCount = result.FailedReadability ? 1 : 0;
            FailedCount += result.Blacklist.Any() ? 1 : 0;
            FailedCount += result.LongWords.Any() ? 1 : 0;
        }

    }
}
