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
    public class GridValueParser : PreflightValueParser, IPreflightValueParser
    {
        private readonly IEnumerable<IGridEditorConfig> _gridEditorConfig;

        private const string _gridRteJsonPath = "$..controls[?(@.editor.view == 'rte' || @.editor.view == 'textstring' || @.editor.view == 'textarea' || @.editor.alias == 'rte' || @.editor.alias == 'headline' || @.editor.alias == 'quote')]";
        private const string _gridValueJsonPath = "$..value";
        private const string _gridRows = "..rows";
        private const string _gridEditorAlias = "$..editor.alias";
        private const string _gridName = "name";

#if NETCOREAPP
        public GridValueParser(IOptions<IGridConfig> gridConfig, IPluginExecutor pluginExecutor, IMessenger messenger) : base(messenger, pluginExecutor)
#else
        public GridValueParser(IGridConfig gridConfig, IPluginExecutor pluginExecutor, IMessenger messenger) : base(messenger, pluginExecutor)
#endif
        {

#if NETCOREAPP
            _gridEditorConfig = gridConfig.Value.EditorsConfig.Editors;
#else
            _gridEditorConfig = gridConfig.EditorsConfig.Editors;
#endif
        }

        public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams)
        {
            JObject asJson = JObject.Parse(parserParams.PropertyValue);
            List<PreflightPropertyResponseModel> response = new List<PreflightPropertyResponseModel>();

            // a single grid row can contain the same editor multiple times
            // a single grid property can contain the same row multiple times
            // so we need to track the count and append if > 1
            var rowLabelDictionary = new Dictionary<string, int>();

            IEnumerable<JToken> rows = asJson.SelectTokens(_gridRows);
            foreach (JToken row in rows)
            {
                string rowName = AddLabelCount(row[0].Value<string>(_gridName), rowLabelDictionary);

                IEnumerable<JToken> editors = row.SelectTokens(_gridRteJsonPath);
                var editorLabelDictionary = new Dictionary<string, int>();

                foreach (JToken editor in editors)
                {
                    // this is a bit messy - maps the grid editor view to the knownpropertyalias value
                    var editorAlias = editor.SelectToken(_gridEditorAlias)?.ToString();

                    var gridEditorConfig = _gridEditorConfig.FirstOrDefault(x => x.Alias == editorAlias);
                    var gridEditorView = gridEditorConfig.View;

                    var gridEditorName = AddLabelCount(gridEditorConfig.Name, editorLabelDictionary);
                    
                    string label = GetLabel(parserParams.PropertyName, gridEditorName, rowName);

                    JToken value = editor.SelectToken(_gridValueJsonPath);
                    if (value == null || !value.ToString().HasValue())
                    {
                        SendRemoveResponse(gridEditorName, label);
                    }
                    else
                    {
                        parserParams.PropertyValue = value.ToString();
                        parserParams.PropertyEditorAlias = GetGridEditorViewAlias(gridEditorView);

                        var model = ExecutePlugin(parserParams, label, KnownPropertyAlias.Grid);

                        response.Add(model);
                    }
                }
            }

            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="label"></param>
        /// <param name="labels"></param>
        private static string AddLabelCount(string label, Dictionary<string, int> labelDictionary)
        {
            if (labelDictionary.ContainsKey(label))
            {
                labelDictionary[label] += 1;
            } else
            {
                labelDictionary[label] = 1;
            }

            if (labelDictionary[label] > 1)
            {
                label = label + " " + labelDictionary[label];
            }

            return label;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="gridEditorView"></param>
        /// <returns></returns>
        private static string GetGridEditorViewAlias(string gridEditorView)
        {
            if (!gridEditorView.HasValue()) return "";

            switch (gridEditorView)
            {
                case KnownGridEditorAlias.Rte: return KnownPropertyAlias.Rte;
                case KnownGridEditorAlias.Textstring: return KnownPropertyAlias.Textbox;
                case KnownGridEditorAlias.Textarea: return KnownPropertyAlias.Textarea;
            }

            return "";
        }
    }
}
