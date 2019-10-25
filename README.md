# Preflight - Pre-publishing checks for Umbraco

[![Latest build](https://ci.appveyor.com/api/projects/status/mhwi63lfxdqglrlk?svg=true)](https://ci.appveyor.com/project/nathanwoulfe/preflight/build/artifacts)
[![NuGet release](https://img.shields.io/nuget/dt/Preflight.Umbraco.svg)](https://www.nuget.org/packages/Preflight.Umbraco)
[![Our Umbraco project page](https://img.shields.io/badge/our-umbraco-brightgreen.svg)](https://our.umbraco.org/projects/backoffice-extensions/preflight)

Preflight is a pluggable framework for content quality assurance. Out of the box, Preflight tests general readability (including Flesch reading level; sentence length; and word length and complexity), checks links, and provides a text replacement mechanism.

Preflight is an Umbraco 8 (8.1+) content app, and supports content in RTEs and textareas, including those nested in the Grid. 

## Plugins
Preflight plugins are defined in C# classes deriving the `IPreflightPlugin` interface. All plugins MUST be decorated with an `ExportAttribute` (found in the `System.ComponentModel.Composition` namespace), per the example below:

```csharp
[Export(typeof(IPreflightPlugin))]
```

The interface requires a range of fields be populated, but the heavy lifting takes place in the `Check()` method:
 - `{string} Name`: displayed in the settings as the name of the collapsable panel (which contains the plugin settings)
 - `{string} Summary`: a brief description of what the plugin tests, also displayed in the settings section
 - `{string} Description`: a longer description of the plugin. The value is available in the test results and can be displayed within the content app (ie in an overlay, if the test is complex and requires explanation)
 - `{string} ViewPath`: the path to an AngularJs view used to display the test results in the content app. There is no default, if this is not set to a valid view, the results will not be displayed.
 - `{int} SortOrder`: set the return order for the plugin result
 - `{int} FailedCount`: the number of tests failed. `Check()` can return multiple values (ie readability tests length, blacklist, reading ease, and returns values for each which are summed into `FailedCount`. The value is used to set the content app badge, and badges against each property in the result view.
 - `{bool} Failed`: set this in `Check()` to reflect the completed test status
 - `{object} Result`: returned to the content app (and ultimately to the AngularJs view defined at ViewPath, as $scope.model).
 - `{IEnumerable<SettingsModel> Settings`: a collection of settings for the plugin (refer below)
 
### Settings
A plugin can have multiple user-editable settings, of type `GenericSettingModel` - these are displayed in the backoffice settings section, and configured in a plugin constructor. Each setting requires: 
 - `{string} Name`: should be obvious?
 - `{string} Value`: a default value
 - `{string} Description`: a friendly explanation of what the setting controls
 - `{string} View`: a path to an Umbraco property editor view (`SettingType` holds common options)
 - `{int} Order`: the order for display on the settings tab
 
To populate the settings on a plugin, use the `PluginSettingsList.Populate()` method - it adds a couple of common default settings (disabled, and run on save only) by default, to save some boilerplate.

### Check()
`Check` is the method where the magic happens. The only requirement for the method is that it sets the `Failed`, `FailedCount` and `Result` fields on the enclosing class - the content app depends on these values existing. 

The method receives three arguments - `{int} id`, `{string} val` and `{List<SettingsModel> settings}`:
 - `id`: the id of the current content item under test
 - `val`: the string value of the current property under test
 - `settings`: a list of settings applied to all existing Preflight plugins (includes the settings for the current plugin, if any have been added)
 
Use those argument to determine the result for your test - how you do that is entirely up to you, so long as the aforementioned fields are set.
