using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Preflight.Models;

[TableName(KnownStrings.SettingsTable)]
[ExplicitColumns]
[PrimaryKey("Id", AutoIncrement = true)]
public class SettingDto
{
    [Column("Id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Set the default value for the setting
    /// </summary>
    [Column("Value")]
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Set the default value for the setting
    /// </summary>
    [Column("Setting")]
    public Guid Setting { get; set; }

    public SettingDto(SettingsModel? model)
    {
        if (model is null)
        {
            return;
        }

        Id = model.Id;
        Setting = model.Guid;
        Value = model.Value?.ToString() ?? string.Empty;
    }
}
