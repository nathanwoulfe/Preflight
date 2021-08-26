using Newtonsoft.Json.Linq;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using System.Collections.Generic;
using System.Linq;
#if NETCOREAPP
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Grid;
#else
using Umbraco.Core.Configuration.Grid;
#endif 

namespace Preflight.Parsers
{
    public class GridValueParser : IPreflightValueParser
    {
        private readonly IPluginExecutor _pluginExecutor;
        private readonly IMessenger _messenger;

        private readonly IEnumerable<IGridEditorConfig> _gridEditorConfig;

        private const string _gridRteJsonPath = "$..controls[?(@.editor.view == 'rte' || @.editor.view == 'textstring' || @.editor.view == 'textarea' || @.editor.alias == 'rte' || @.editor.alias == 'headline' || @.editor.alias == 'quote')]";
        private const string _gridValueJsonPath = "$..value";

#if NETCOREAPP
        public GridValueParser(IOptions<IGridConfig> gridConfig, IPluginExecutor pluginExecutor, IMessenger messenger)
#else
        public GridValueParser(IGridConfig gridConfig, IPluginExecutor pluginExecutor, IMessenger messenger)
#endif
        {
            _pluginExecutor = pluginExecutor;
            _messenger = messenger;

#if NETCOREAPP
            _gridEditorConfig = gridConfig.Value.EditorsConfig.Editors;
#else
            _gridEditorConfig = gridConfig.EditorsConfig.Editors;
#endif
        }

        public List<PreflightPropertyResponseModel> Parse(string propertyName, string propertyValue, string culture, int nodeId, bool fromSave)
        {
            JObject asJson = JObject.Parse(propertyValue);
            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            IEnumerable<JToken> rows = asJson.SelectTokens("..rows");
            foreach (JToken row in rows)
            {
                string rowName = row[0].Value<string>("name");
                IEnumerable<JToken> editors = row.SelectTokens(_gridRteJsonPath);

                foreach (JToken editor in editors)
                {
                    // this is a bit messy - maps the grid editor view to the knownpropertyalias value
                    var editorViewAlias = "";
                    var editorAlias = editor.SelectToken("$..editor.alias")?.ToString();

                    var gridEditorConfig = _gridEditorConfig.FirstOrDefault(x => x.Alias == editorAlias);
                    var gridEditorName = gridEditorConfig.Name;
                    var gridEditorView = gridEditorConfig.View;

                    string label = $"{propertyName} ({rowName} - {gridEditorName})";

                    JToken value = editor.SelectToken(_gridValueJsonPath);
                    if (value == null || !value.ToString().HasValue())
                    {
                        _messenger.SendTestResult(new PreflightPropertyResponseModel
                        {
                            Name = gridEditorName,
                            Label = label,
                            Remove = true
                        });
                    }
                    else
                    {
                        if (gridEditorView.HasValue())
                        {
                            switch (gridEditorView)
                            {
                                case "rte": editorViewAlias = KnownPropertyAlias.Rte; break;
                                case "textstring": editorViewAlias = KnownPropertyAlias.Textbox; break;
                                case "textarea": editorViewAlias = KnownPropertyAlias.Textarea; break;
                            }
                        }

                        PreflightPropertyResponseModel model = _pluginExecutor.Execute(propertyName, culture, value.ToString(), editorViewAlias, nodeId, fromSave, KnownPropertyAlias.Grid);

                        model.Label = label;
                        model.TotalTests = model.Plugins.Aggregate(0, (acc, x) => acc + x.TotalTests);

                        response.Add(model);
                    }
                }
            }

            return response;
        }
    }
}
