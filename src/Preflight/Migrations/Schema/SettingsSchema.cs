using NPoco;
using System;
#if NETCOREAPP
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
#else
using Umbraco.Core.Persistence.DatabaseAnnotations;
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

        /// <summary>
        /// Set the default value for the setting
        /// </summary>
        [Column("Value")]
        public string Value { get; set; }

        /// <summary>
        /// Set the default value for the setting
        /// </summary>
        [Column("Setting")]
        public Guid Setting { get; set; }
    }
}
