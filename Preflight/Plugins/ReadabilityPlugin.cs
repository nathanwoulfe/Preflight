using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Preflight.Constants;
using Preflight.Models;
using Preflight.Services;
using Preflight.Services.Interfaces;

namespace Preflight.Plugins
{
    [Export(typeof(IPreflightPlugin))]
    public class ReadabilityPlugin : IPreflightCorePlugin
    {
        private readonly IReadabilityService _readabilityService;

        public object Result { get; set; }
        public List<SettingsModel> Settings { get; set; }

        public bool Failed { get; set; }
        public bool Core => true;

        public int SortOrder => -2;
        public int FailedCount { get; set; }

        public string Name => "Readability";
        public string ViewPath => "/app_plugins/preflight/backoffice/plugins/readability.html";

        public ReadabilityPlugin() : this(new ReadabilityService())
        {
        }

        private ReadabilityPlugin(IReadabilityService readabilityService)
        {
            _readabilityService = readabilityService;

            Settings = new List<SettingsModel>
            {
                new DisabledSettingModel(Name),
                new GenericSettingModel("Readability target - minimum")
                {
                    Value = "60",
                    Description = "Readability result must be great than this value",
                    View = SettingType.Slider,
                    Order = 1,
                    Core = true,
                    Tab = Name
                },
                new GenericSettingModel("Readability target - maximum")
                {
                    Value = "100",
                    Description = "Readability result must be less than this value",
                    View = SettingType.Slider,
                    Order = 2,
                    Core = true,
                    Tab = Name
                },
                new GenericSettingModel("Long word syllable count")
                {
                    Value = "5",
                    Description =
                        "Words in text will be flagged as long, if their syllable count is equal to or greater than this value",
                    View = SettingType.Slider,
                    Order = 3,
                    Core = true,
                    Tab = Name
                }
            };
        }

        public void Check(int id, string val, List<SettingsModel> settings)
        {
            // must get a result of any type
            ReadabilityResponseModel result = _readabilityService.Check(val, settings);
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
