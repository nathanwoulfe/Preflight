Preflight is a pluggable framework for content quality assurance. Out of the box, Preflight tests general readability (including Flesch reading level; sentence length; and word length and complexity), checks links, and provides a text replacement mechanism.

Preflight is an Umbraco content app, and supports content in RTEs and textareas, including those nested in the Block List, Block Grid, Grid and Nested Content. 

Please note the Preflight versions supporting Umbraco 8, 9 and 10 are considered feature complete, all future development effort will target Umbraco 11+. Bug fixes targeting earlier versions will be considered where required.

Note also Preflight 10 requires Umbraco 10.4.0 as a minimum dependency (as Block Grid was introduced in this version).

## Getting started
Install Preflight via your CLI of choice => `Install-Package Preflight.Umbraco`

After installing, you'll find a new tree in the settings section, from which you can manage generic Preflight settings and all active plugins.

When navigating the content tree, Preflight will run when a content node is loaded, and reports test findings in the Preflight content app. The app shows a summary of executed tests and their results.

## Generic settings

Preflight is variant-friendly, and provides variant-specific settings as not all tests are relevant to all cultures (eg comprehension rules for English will differ to those for German).
 - **Run Preflight on save**: set to true to run tests when a content node is saved
 - **Cancel save when Preflight tests fail**: set to true to cancel save events, if run on save is true and any tests fail
 - **Properties to test**: set the property types to include in test runs (eg only test changes in TinyMCE and Block Grid properties)
 - **User group opt in/out**: set the user groups to include in test runs (eg only test changes from members of the Editors group)

## Plugins
Preflight plugins are defined in C# classes deriving the `IPreflightPlugin` interface.

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
 - `{guid} Guid`: a unique identifier for the plugin
 - `{Dictionary<string, object>} Value`: a value dictionary, where the key is the variant culture code
 - `{object} DefaultValue`: an optional default value applied to all variants when the setting is first registered
 - `{string} Description`: a friendly explanation of what the setting controls
 - `{string} View`: a path to an Umbraco property editor view (`SettingType` holds common options)
 - `{int} Order`: the order for display on the settings tab
 - `{object} Prevalues`: an optional object to provide property editor prevalues for use in the setting UI (eg a list of checkbox values)

The following settings are added by default to all plugins
 - **Properties to test**: allows subsetting properties per test (eg Plugin 1 should run on all Block Grid properties, Plugin 2 should only run on TextBox) 
 - **Disabled**: set to true to exclude the plugin from test runs
 - **On save only**: only run the plugin on save events, not when loading the content app

To populate the settings on a plugin, use the `PluginSettingsList.Populate()` method.

### Check()
`Check` is the method where the magic happens. The only requirement for the method is that it sets the `Failed`, `FailedCount` and `Result` fields on the enclosing class - the content app depends on these values existing. 

The method receives three arguments - `{int} id`, `{guid} culture`, `{string} val` and `{List<SettingsModel> settings}`:
 - `id`: the id of the current content item under test
 - `culture`: the culture of the current property under test
 - `val`: the string value of the current property under test
 - `settings`: a list of settings applied to all existing Preflight plugins (includes the settings for the current plugin, if any have been added)
 
Use those argument to determine the result for your test - how you do that is entirely up to you, so long as the aforementioned fields are set.

For implementation examples, refer to the core plugins in `Preflight.Plugins` eg `Preflight.Plugins.Readability`.

## Credits
Icon sourced from [Feather Icons](https://feathericons.com/)
