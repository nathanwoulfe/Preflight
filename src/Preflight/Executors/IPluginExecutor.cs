using Preflight.Extensions;
using Preflight.Models;
using Preflight.Plugins;
using System;
using System.Linq;
using Preflight.Services;
using System.Collections.Generic;
#if NETCOREAPP
using Microsoft.Extensions.Logging;
#else
using Preflight.Logging;
#endif

namespace Preflight.Executors
{
    public interface IPluginExecutor
    {
        PreflightPropertyResponseModel Execute(string name, string culture, string val, string alias, int id, bool fromSave, string parentAlias = "");
    }

    public class PluginExecutor : IPluginExecutor
    {
        private string _testableProperties;
        private List<SettingsModel> _settings;

        private readonly ILogger<PluginExecutor> _logger;
        private readonly ISettingsService _settingsService;
        private readonly PreflightPluginCollection _plugins;

        public PluginExecutor(PreflightPluginCollection plugins, ISettingsService settingsService)
        {
            _plugins = plugins;
            _settingsService = settingsService;
        }

        /// <summary>
        /// Runs the set of plugins against the given string
        /// </summary>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <returns></returns>
        public PreflightPropertyResponseModel Execute(string name, string culture, string val, string alias, int id, bool fromSave, string parentAlias = "")
        {
            _settings = _settingsService.Get().Settings;
            _testableProperties = _settings.GetValue<string>(KnownSettings.PropertiesToTest, culture);

            var model = new PreflightPropertyResponseModel
            {
                Label = name,
                Name = name
            };

            if (val == null || !_testableProperties.Contains(alias) || (parentAlias.HasValue() && !_testableProperties.Contains(parentAlias)))
                return model;

            foreach (IPreflightPlugin plugin in _plugins)
            {
                // settings on the plugin are the defaults - set to correct values from _settings
                plugin.Settings = _settings.Where(s => s.Tab == plugin.Name)?.ToList();

                // ignore disabled plugins
                if (plugin.IsDisabled(culture)) continue;
                if (!fromSave && plugin.IsOnSaveOnly(culture)) continue;

                string propsValue = plugin.Settings.FirstOrDefault(x => x.Alias.EndsWith(KnownStrings.PropertiesToTestSuffix))?.Value.ForVariant(culture);
                string propsToTest = propsValue ?? string.Join(KnownStrings.Comma, KnownPropertyAlias.All);

                // only continue if the field alias is include for testing, or the parent alias has been set, and is included for testing
                if (!propsToTest.Contains(alias) || (parentAlias.HasValue() && !propsToTest.Contains(parentAlias))) continue;

                try
                {
                    Type pluginType = plugin.GetType();
                    if (pluginType.GetMethod("Check") == null) continue;

                    plugin.Check(id, culture, val, _settings);

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
                    _logger.LogError(e, "Preflight couldn't take off: {Message}", e.Message);
                }
            }

            // mark as failed if any sub-tests have failed
            model.FailedCount = model.Plugins.Sum(x => x.FailedCount);
            model.Failed = model.FailedCount > 0;

            model.Plugins = model.Plugins.OrderBy(p => p.SortOrder).ToList();
            model.TotalTests = model.Plugins.Aggregate(0, (acc, x) => acc + x.TotalTests);

            return model;
        }
    }
}
