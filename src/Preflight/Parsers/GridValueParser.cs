using Newtonsoft.Json.Linq;
using Preflight.Executors;
using Preflight.Extensions;
using Preflight.Models;
using Preflight.Services;
using Umbraco.Cms.Core.Configuration.Grid;

namespace Preflight.Parsers;

public class GridValueParser : PreflightValueParserBase, IPreflightValueParser
{
    private const string GridRteJsonPath = "$..controls[?(@.editor.view == 'rte' || @.editor.view == 'textstring' || @.editor.view == 'textarea' || @.editor.alias == 'rte' || @.editor.alias == 'headline' || @.editor.alias == 'quote')]";
    private const string GridValueJsonPath = "$..value";
    private const string GridRows = "..rows";
    private const string GridEditorAlias = "$..editor.alias";
    private const string GridName = "name";

    private readonly IEnumerable<IGridEditorConfig> _gridEditorConfig;

    public GridValueParser(IGridConfig gridConfig, IPluginExecutor pluginExecutor, IMessenger messenger)
        : base(messenger, pluginExecutor)

    {
        _gridEditorConfig = gridConfig.EditorsConfig.Editors;
    }

    public List<PreflightPropertyResponseModel> Parse(ContentParserParams parserParams)
    {
        var asJson = JObject.Parse(parserParams.PropertyValue);
        List<PreflightPropertyResponseModel> response = [];

        // a single grid row can contain the same editor multiple times
        // a single grid property can contain the same row multiple times
        // so we need to track the count and append if > 1
        var rowLabelDictionary = new Dictionary<string, int>();

        IEnumerable<JToken> rows = asJson.SelectTokens(GridRows);

        if (!rows.Any())
        {
            return response;
        }

        foreach (JToken row in rows)
        {
            if (!row.HasValues)
            {
                continue;
            }

            string rowName = AddLabelCount(row[0]!.Value<string>(GridName)!, rowLabelDictionary);

            IEnumerable<JToken> editors = row.SelectTokens(GridRteJsonPath);
            var editorLabelDictionary = new Dictionary<string, int>();

            foreach (JToken editor in editors)
            {
                // this is a bit messy - maps the grid editor view to the knownpropertyalias value
                string? editorAlias = editor.SelectToken(GridEditorAlias)?.ToString();

                IGridEditorConfig? gridEditorConfig = _gridEditorConfig.FirstOrDefault(x => x.Alias == editorAlias);

                if (gridEditorConfig is null || gridEditorConfig.Name is null)
                {
                    continue;
                }

                string? gridEditorView = gridEditorConfig.View;

                if (gridEditorView is null)
                {
                    continue;
                }

                string gridEditorName = AddLabelCount(gridEditorConfig.Name, editorLabelDictionary);

                string label = GetLabel(parserParams.PropertyName, gridEditorName, rowName);

                JToken? value = editor.SelectToken(GridValueJsonPath);
                if (value is null || !value.ToString().HasValue())
                {
                    SendRemoveResponse(gridEditorName, label, parserParams.NodeId);
                }
                else
                {
                    parserParams.PropertyValue = value.ToString();
                    parserParams.PropertyEditorAlias = GetGridEditorViewAlias(gridEditorView);

                    PreflightPropertyResponseModel model = ExecutePlugin(parserParams, label, KnownPropertyAlias.Grid);

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
    /// <param name="labelDictionary"></param>
    private static string AddLabelCount(string label, Dictionary<string, int> labelDictionary)
    {
        if (labelDictionary.ContainsKey(label))
        {
            labelDictionary[label] += 1;
        }
        else
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
        if (!gridEditorView.HasValue())
        {
            return string.Empty;
        }

        switch (gridEditorView)
        {
            case KnownGridEditorAlias.Rte:
                return KnownPropertyAlias.Rte;
            case KnownGridEditorAlias.Textstring:
                return KnownPropertyAlias.Textbox;
            case KnownGridEditorAlias.Textarea:
                return KnownPropertyAlias.Textarea;
            default:
                break;
        }

        return string.Empty;
    }
}
