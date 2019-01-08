using System;
using System.Collections.Generic;
using Preflight.Models;
using Preflight.Plugins;

namespace Preflight.Site.V8.App_Code
{
    public class PreflightPluginTest : PreflightPlugin
    {
        public PreflightPluginTest()
        {
            Name = "New preflight test plugin";
            PassText = "This test is passing";
            FailText = "This test failed";
            ViewPath = "/app_plugins/preflightplugins/testPluginView.html";

            Settings.Add(new SettingsModel
            {
                Value = 0,
                Label = "Setting for newPlugin",
                Description = "this is the description for newPlugin",
                View = "views/propertyeditors/boolean/boolean.html",
                Order = 4
            });
        }

        public override object Check(string val, out bool failed)
        {
            failed = true;
            var r = new Random();

            List<int> response = new List<int>();
            for (var i = 0; i < 5; i += 1)
            {
                response.Add(r.Next());
            }

            return response;
        }
    }
}
