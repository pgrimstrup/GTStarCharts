using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.Common.Upgrade
{
    public class DbTableInfo
    {
        public Type EntityType { get; set; }
        public string SchemaName { get; set; }
        public string TableName { get; set; }
        public bool ExistsInDatabase { get; set; }
        public bool ExistsInModel { get; set; }
        public DateTime? DateCreated { get; set; }

        public Dictionary<string, DbColumnInfo> Columns { get; } = new Dictionary<string, DbColumnInfo>(StringComparer.InvariantCultureIgnoreCase);
        public List<DbIndexInfo> Indexes { get; } = new List<DbIndexInfo>();
        public List<DbReferenceInfo> References { get; } = new List<DbReferenceInfo>();

        public bool AlterNeeded
        {
            get { return Columns.Values.Any(c => c.ExistsInModel && (c.Altered || !c.ExistsInDatabase)); }
        }

        public DbTableInfo()
        {
        }

        internal string GetColumnName(string propertyname)
        {
            var column = Columns.Values.FirstOrDefault(c => c.PropertyName == propertyname);
            return column?.ColumnName;
        }

        internal string[] GetColumnNames(string[] propertynames)
        {
            return propertynames.Select(p => GetColumnName(p)).ToArray();
        }

        internal DbReferenceInfo FindReferenceToProperties(DbTableInfo tableto, string[] fromprops, string[] toprops)
        {
            // Find the column name for the specified property names
            var fromcols = this.GetColumnNames(fromprops);
            var tocols = tableto.GetColumnNames(toprops);

            return FindReferenceToProperties(tableto, fromcols, tocols);
        }

        internal DbReferenceInfo FindReferenceTo(DbTableInfo tableto, string[] fromcols, string[] tocols)
        {
            return FindReferenceTo(tableto.SchemaName, tableto.TableName, fromcols, tocols);
        }

        internal DbReferenceInfo FindReferenceTo(string schema, string name, string[] fromcols, string[] tocols)
        {

            if (fromcols == null || tocols.Length == 0)
                return null;
            if (tocols == null || tocols.Length == 0)
                return null;
            if (fromcols.Length != tocols.Length)
                return null;

            foreach (var r in References)
            {
                if ((schema == null || r.TargetSchemaName.Equals(schema, StringComparison.InvariantCultureIgnoreCase)) &&
                    r.TargetTableName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                {
                    // Quick check to see if the number of columns match up
                    if (r.SourceColumns == null || r.SourceColumns.Length != fromcols.Length)
                        continue;
                    if (r.TargetColumns == null || r.TargetColumns.Length != tocols.Length)
                        continue;

                    // All four arrays are the same length
                    // Property names ARE case sensitive. Column names ARE NOT case sensitive.
                    bool match = true;
                    for (int i = 0; i < fromcols.Length; i++)
                    {
                        if (!fromcols[i].Equals(r.SourceColumns[i], StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Source/From columns differ - this is not a matching foreign key
                            match = false;
                            break;
                        }

                        if (!tocols[i].Equals(r.TargetColumns[i], StringComparison.InvariantCultureIgnoreCase))
                        {
                            // Target/To columns differ - this is not a matching foreign key
                            match = false;
                            break;
                        }
                    }

                    if (match) return r;
                }
            }

            return null;
        }

        internal DbIndexInfo FindIndex(string[] columns)
        {
            foreach(var index in Indexes)
            {
                bool matched = true;
                string[] indexcols = index.Columns.Keys.ToArray(); // This includes Included columns

                for(int i = 0; i < indexcols.Length && i < columns.Length; i++)
                {
                    if(!String.Equals(indexcols[i], columns[i], StringComparison.InvariantCultureIgnoreCase))
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                    return index;
            }

            return null;
        }

        internal IEnumerable<string> GetPropertyCreationOrder()
        {
            if(EntityType == null)
            {
                // Simply return all property names for columns
                return Columns.Values.Select(c => c.PropertyName).ToArray();
            }
            else
            {
                // Return all properties with matching columns in declaration order
                return EntityType.GetProperties().Select(p => p.Name).ToArray();
            }
        }
    }

    public class DbTableInfo<T> where T : class
    {
        public DbTableInfo(DbTableInfo table)
        {
        }
    }
}
