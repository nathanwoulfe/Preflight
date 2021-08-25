using NPoco;
using Preflight.Constants;
#if NET472
using Umbraco.Core.Persistence.DatabaseAnnotations;
#else
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
#endif

namespace Preflight.Migrations.Schema
{
    [TableName(KnownStrings.SettingsTable)]
    [ExplicitColumns]
    [PrimaryKey("Id", AutoIncrement = true)]
    public class SettingsSchema
    {
        [Column("Id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("Culture")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Culture { get; set; }

        [Column("Core")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public bool Core { get; set; }

        /// <summary>
        /// A UI-friendly label for the setting
        /// </summary>
        [Column("Label")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Label { get; set; }

        /// <summary>
        /// Set the default value for the setting
        /// </summary>
        [Column("Value")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Value { get; set; }

        /// <summary>
        /// Describe the setting
        /// </summary>
        [Column("Description")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Description { get; set; }

        /// <summary>
        /// Name of an Umbraco property editor - full path is built later
        /// </summary>
        [Column("View")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string View { get; set; }

        /// <summary>
        /// Where should the setting sit on the tab
        /// </summary>
        [Column("Order")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int Order { get; set; }

        /// <summary>
        /// Where should the setting be displayed - either reference an existing tab from SettingsTabNames, or add your own
        /// Plugins default to the plugin name
        /// </summary>
        [Column("Tab")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Tab { get; set; }

        /// <summary>
        /// Prevalues for the setting
        /// </summary>
        [Column("Prevalues")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Prevalues { get; set; }
    }
}
