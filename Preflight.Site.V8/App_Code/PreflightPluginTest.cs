//using System;
//using System.Collections.Generic;
//using Preflight.Models;
//using Preflight.Plugins;

//namespace Preflight.Site.V8.App_Code
//{
//    public class PreflightPluginTest : PreflightPlugin
//    {
//        public PreflightPluginTest()
//        {
//            Name = "New preflight test plugin";
//            ViewPath = "/app_plugins/preflightplugins/testPluginView.html";

//            Settings.Add(new SettingsModel
//            {
//                Value = 0,
//                Label = "Setting for newPlugin",
//                Description = "this is the description for newPlugin",
//                View = "views/propertyeditors/boolean/boolean.html",
//                Order = 4
//            });
//        }

//        public override object Check(int id, string culture, string val, out bool failed)
//        {
//            var r = new Random();
//            failed = r.Next(0, 2) == 1;

//            List<int> response = new List<int>();
//            for (var i = 0; i < 5; i += 1)
//            {
//                response.Add(r.Next());
//            }

//            return response;
//        }
//    }
//}
