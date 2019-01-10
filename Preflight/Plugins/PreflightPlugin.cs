using Newtonsoft.Json;
using Preflight.Models;
using System.Collections.Generic;
using Preflight.Extensions;

namespace Preflight.Plugins
{
    /// <summary>
    /// Inherit from this class to create Preflight test plugins - these will be discovered and run as part of the test suite
    /// </summary>
    public abstract class PreflightPlugin : IPreflightPlugin
    {
        /// <summary>
        /// Will be set by ContentChecker to indicate the state of the completed test
        /// </summary>
        [JsonProperty("failed")]
        public bool Failed { get; set; }

        /// <summary>
        /// Give the plugin a descriptive, sensible name - this will appear in the UI
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Text to display when this test passes
        /// </summary>
        [JsonProperty("passText")]
        public string PassText { get; set; }

        /// <summary>
        /// Text to display when this test fails - will not be shown when ViewPath is set
        /// </summary>
        [JsonProperty("failText")]
        public string FailText { get; set; }

        /// <summary>
        /// Provide an alias for the plugin
        /// </summary>
        [JsonProperty("alias")]
        internal string Alias => Name.Camel();

        /// <summary>
        /// Path to an AngularJs view to display the test results on the Content App
        /// </summary>
        [JsonProperty("viewPath")]
        public string ViewPath { get; set; }

        /// <summary>
        /// Will store the test result and return it, as a JSON object, to the view defined in ViewPath.
        /// This value will be available as $scope.model in the AngularJs view
        /// </summary>
        [JsonProperty("result")]
        public object Result { get; set; }

        /// <summary>
        /// Check performs the test - Preflight will provide the current node id and property value as paramaters
        /// Currently, checks are assumed to be property-based, not whole-node. This may change in the future
        /// Return any serializable object, and be sure to set failed to the appropriate value - will be false unless modified
        /// </summary>
        /// <param name="id">The current node id</param>
        /// <param name="val">The stringified property data</param>
        /// <param name="failed">Set this to indicate the test result - pass or fail</param>
        /// <returns>Whatever object you choose to return, which will be returned to the custom view as $scope.model</returns>
        public abstract object Check(int id, string val, out bool failed);

        /// <summary>
        /// The settings list should be populated as part of the plugin to allow inclusion of items on the settings dashboard
        /// Disabled is included by default
        /// </summary>
        [JsonProperty("settings")]
        public List<SettingsModel> Settings { get; set; }

        protected PreflightPlugin()
        {
            Failed = false;
            Settings = new List<SettingsModel>();
        }
    }
}
