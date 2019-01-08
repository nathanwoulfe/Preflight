using System.Collections.Generic;
using Preflight.Models;

namespace Preflight.Plugins
{
    public interface IPreflightPlugin
    {
        bool Failed { get; set; }

        string PassText { get; set; }
        string FailText { get; set; }
        string Name { get; set; }
        string ViewPath { get; set; }

        object Result { get; set; }
        object Check(string val, out bool failed);

        List<SettingsModel> Settings { get; set; }
    }
}
