using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.Common.Upgrade
{
    public class DbIndexColumnInfo
    {
        public string ColumnName { get; set; }
        public bool IsDescending { get; set; }
        public bool IsIncluded { get; set; }
    }

    public class DbIndexInfo
    {
        internal DbTableInfo Table { get; set; }
        internal Dictionary<string, DbIndexColumnInfo> Columns { get; } = new Dictionary<string, DbIndexColumnInfo>();

        public bool Exists { get; set; }
        public bool Removed { get; set; } // Set to true to remove the index from the database
        public bool Altered { get; set; } // Set to true to cause the index to be recreated
        public string IndexName { get; set; }
        public bool IsClustered { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsUnique { get; set; }

        public DbIndexInfo(DbTableInfo table)
        {
            Table = table;
        }

        public DbIndexInfo Description(string description)
        {
            if (String.IsNullOrEmpty(description))
            {
                this.IsClustered = false;
                this.IsPrimaryKey = false;
                this.IsUnique = false;
            }
            else
            {
                this.IsClustered = description.IndexOf("clustered", StringComparison.InvariantCultureIgnoreCase) >= 0;
                this.IsPrimaryKey = description.IndexOf("primary key", StringComparison.InvariantCultureIgnoreCase) >= 0;
                this.IsUnique = description.IndexOf("unique", StringComparison.InvariantCultureIgnoreCase) >= 0;
            }
            return this;
        }

        public DbIndexInfo Keys(string keys)
        {
            if (String.IsNullOrEmpty(keys))
            {
                this.Columns.Clear();
            }
            else
            {
                string[] words = keys.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(var colname in words)
                {
                    if(colname.EndsWith("(-)"))
                        this.Columns.Add(colname, new DbIndexColumnInfo { ColumnName = colname.Substring(0, colname.Length-3), IsDescending = true });
                    else
                        this.Columns.Add(colname, new DbIndexColumnInfo { ColumnName = colname });
                }
            }
            return this;
        }

    }
}
