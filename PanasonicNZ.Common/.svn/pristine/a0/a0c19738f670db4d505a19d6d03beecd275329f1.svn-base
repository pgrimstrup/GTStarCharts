using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.Common.Upgrade
{
    public class DbColumnInfo
    {
        internal DbTableInfo Table { get; }

        public bool ExistsInDatabase { get; set; }
        public bool ExistsInModel { get; set; }
        public bool Altered { get; set; }
        public int Index { get; set; }
        public string PropertyName { get; set; }
        public string ColumnName { get; set; }
        public SqlDbType Type { get; set; }
        public int? Length { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsNullable { get; set; } = true;
        public bool IsIdentity { get; set; }
        public bool IsPrimaryKey { get; set; }

        public int IdentitySeed { get; set; } = 1;
        public int IdentityIncrement { get; set; } = 1;
        public bool IdentityRemoved { get; set; }

        public string DefaultConstraintName { get; set; }
        public string DefaultValueExpression { get; set; }

        public DbColumnInfo(DbTableInfo table)
        {
            Table = table;
        }
    }
}
