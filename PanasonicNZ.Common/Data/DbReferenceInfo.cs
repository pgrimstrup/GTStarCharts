using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.Common.Upgrade
{
    public class DbReferenceInfo
    {
        public DbReferenceInfo(DbTableInfo table)
        {
            this.Table = table;
        }

        public DbTableInfo Table { get; }

        public bool ExistsInModel { get; set; }
        public bool ExistsInDatabase { get; set; }
        public bool Dropped { get; set; }

        public string SourceDatabaseName { get; set; }
        public string SourceTableName { get; set; }
        public string SourceSchemaName { get; set; }
        public string ForeignKeyName { get; set; }


        public string TargetDatabaseName { get; set; }
        public string TargetSchemaName { get; set; }
        public string TargetTableName { get; set; }

        public string[] SourceColumns { get; set; }
        public string[] TargetColumns { get; set; }
    }
}
