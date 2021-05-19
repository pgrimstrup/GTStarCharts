using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Mapping;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PanasonicNZ.Common.Upgrade
{
    public class DbInfoLoader
    {
        public void LoadInfo(DbContext context, IDictionary<string, DbTableInfo> tables)
        {
            try
            {
                // Determine what tables to load info for
                // Make sure that all tables have been created or upgraded
                var objectContext = ((IObjectContextAdapter)context).ObjectContext;
                var metadata = objectContext.MetadataWorkspace;
                var csspace = metadata.GetItems<EntityContainerMapping>(DataSpace.CSSpace).Single();

                // Find the mapping between conceptual and storage model for this entity set
                var mappings = csspace.EntitySetMappings.ToArray();
                foreach (var mapping in mappings)
                {
                    // Determine the CLR type used in O-Space
                    var type = metadata.GetClrTypeFromCSpaceType(mapping);
                    LoadTableInfo(context, tables, mapping, type);
                }

                foreach (var mapping in mappings)
                    LoadReferenceInfo(tables, mapping);

                // Many-to-many relationships
                var relations = csspace.AssociationSetMappings.ToArray();
                foreach (var relation in relations)
                    LoadAssociationInfo(context, tables, relation);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw;
            }
        }

        private void LoadAssociationInfo(DbContext context, IDictionary<string, DbTableInfo> tables, AssociationSetMapping relation)
        {
            // Determine name of the association table
            string name = relation.StoreEntitySet.Table;

            if(!tables.TryGetValue(name, out DbTableInfo table))
            {
                table = new DbTableInfo();
                table.SchemaName = relation.StoreEntitySet.Schema ?? "dbo";
                table.TableName = relation.StoreEntitySet.Table;
                table.ExistsInModel = true;
                tables.Add(name, table);
            }

            // Load the table info
            LoadInfoFromDatabase(context, table);
            LoadInfoFromModel(table, relation.StoreEntitySet);
            LoadFromAttributes(table);
            CheckPrimaryKey(table);
            CheckDefaultValues(table);

        }

        private void LoadTableInfo(DbContext context, IDictionary<string, DbTableInfo> tables, EntitySetMapping mapping, Type clrType)
        {
            // Find the storage entity set (table) that the entity is mapped
            var entityset = mapping
                .EntityTypeMappings.Single()
                .Fragments.Single()
                .StoreEntitySet;

            // Determine the name of the table
            string name = entityset.Table;

            // See if we have already loaded the table info
            if (!tables.TryGetValue(name, out DbTableInfo table))
            {
                table = new DbTableInfo();
                table.SchemaName = entityset.Schema ?? "dbo";
                table.TableName = entityset.Table;
                table.ExistsInModel = true;
                tables.Add(name, table);
            }

            table.EntityType = clrType;

            // Load the table info
            LoadInfoFromDatabase(context, table);
            LoadInfoFromModel(table, entityset);
            LoadFromAttributes(table);
            CheckPrimaryKey(table);
            CheckDefaultValues(table);
        }

        private void CheckDefaultValues(DbTableInfo table)
        {
            foreach(var col in table.Columns.Values)
            {
                if(col.ExistsInModel && !col.ExistsInDatabase && !col.IsNullable && !col.IsIdentity && col.DefaultValueExpression == null)
                {
                    // Need to create a default expression so the column can be added to a non-empty database
                    switch(col.Type)
                    {
                        case SqlDbType.BigInt:
                        case SqlDbType.Bit:
                        case SqlDbType.Decimal:
                        case SqlDbType.Float:
                        case SqlDbType.Int:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                        case SqlDbType.SmallMoney:
                        case SqlDbType.TinyInt:
                            col.DefaultValueExpression = "0";
                            break;

                        case SqlDbType.Char:
                        case SqlDbType.NChar:
                        case SqlDbType.NText:
                        case SqlDbType.NVarChar:
                        case SqlDbType.Text:
                        case SqlDbType.VarChar:
                            col.DefaultValueExpression = "''";
                            break;

                        case SqlDbType.Date:
                        case SqlDbType.DateTime:
                        case SqlDbType.DateTime2:
                        case SqlDbType.DateTimeOffset:
                        case SqlDbType.SmallDateTime:
                        case SqlDbType.Time:
                            col.DefaultValueExpression = "CURRENT_TIMESTAMP";
                            break;

                        case SqlDbType.UniqueIdentifier:
                            col.DefaultValueExpression = "NEWID()";
                            break;

                    }
                }
            }
        }

        private void CheckPrimaryKey(DbTableInfo table)
        {
            // Make sure the primary key index matches the fields listed as being in the primary key
            var fields = table.Columns.Values.Where(c => c.IsPrimaryKey).ToArray();
            var pkindex = table.Indexes.FirstOrDefault(i => i.IsPrimaryKey);

            if (fields.Length == 0 && pkindex != null)
            {
                pkindex.Removed = true;
            }
            else if (pkindex == null)
            {
                // PK index does not exist, so add it in
                pkindex = new DbIndexInfo(table);
                pkindex.IndexName = $"PK_{table.TableName}";
                pkindex.IsPrimaryKey = true;
                pkindex.IsClustered = !table.Indexes.Any(i => i.IsClustered);
                table.Indexes.Add(pkindex);

                foreach (var field in fields)
                    pkindex.Columns.Add(field.ColumnName, new DbIndexColumnInfo { ColumnName = field.ColumnName });
            }
            else
            {
                // Check whether the PK index has been changed
                var keys = pkindex.Columns.Values.ToArray();
                if (keys.Length != fields.Length)
                    pkindex.Altered = true;
                else
                {
                    for (int i = 0; i < fields.Length; i++)
                        if (fields[i].ColumnName != keys[i].ColumnName)
                        {
                            pkindex.Altered = true;
                            break;
                        }
                }
            }
        }

        private void LoadFromAttributes(DbTableInfo table)
        {
            // If the DbContext is in a different namespace to the Entities, then EntityType may not be correctly matched
            if (table.EntityType == null)
                return;

            var props = table.EntityType.GetProperties();
            foreach (var prop in props)
            {
                if (table.Columns.TryGetValue(prop.Name, out DbColumnInfo column))
                {
                    if (prop.TryGetCustomAttribute(out KeyAttribute key))
                        column.IsPrimaryKey = true;

                    if (prop.TryGetCustomAttribute(out ColumnAttribute ca))
                        column.Index = ca.Order;

                    if (prop.TryGetCustomAttribute(out RequiredAttribute ra))
                        column.IsNullable = false;

                    if (prop.TryGetCustomAttribute(out StringLengthAttribute sl))
                        column.Length = sl.MaximumLength;

                    if (prop.TryGetCustomAttribute(out DatabaseGeneratedAttribute dg))
                        if (dg.DatabaseGeneratedOption == DatabaseGeneratedOption.Identity)
                            column.IsIdentity = true;
                }
            }
        }

        private void LoadInfoFromDatabase(DbContext context, DbTableInfo table)
        {
            table.ExistsInDatabase = false;
            table.DateCreated = null;
            table.Columns.Clear();
            table.Indexes.Clear();

            if (context.TableExists(table.SchemaName, table.TableName, out string schema, out string name))
            {
                table.SchemaName = schema;
                table.TableName = name;
                table.ExistsInDatabase = true;
            }

            if (table.ExistsInDatabase)
            {
                context.EnsureOpen();
                using (var cmd = context.Database.Connection.CreateCommand())
                {
                    cmd.CommandText = $"EXEC sp_help '{table.SchemaName}.{table.TableName}'";
                    using (var rdr = cmd.ExecuteReader())
                    {
                        // 1. Table details
                        if (rdr.Read())
                        {
                            table.DateCreated = rdr.GetDateTime(3);
                        }

                        // 2. Column details
                        if (rdr.NextResult())
                        {
                            int index = 0;
                            while (rdr.Read())
                            {
                                var colname = rdr.GetString(0);
                                if (!table.Columns.TryGetValue(colname, out DbColumnInfo column))
                                {
                                    column = new DbColumnInfo(table);
                                    column.ColumnName = colname;
                                    column.PropertyName = colname;
                                }

                                column.ExistsInDatabase = true;
                                column.Index = index++;
                                column.Type = (SqlDbType)Enum.Parse(typeof(SqlDbType), rdr.GetString(1), true);
                                switch (column.Type)
                                {
                                    case SqlDbType.Binary:
                                    case SqlDbType.VarBinary:
                                    case SqlDbType.Char:
                                    case SqlDbType.VarChar:
                                        column.Length = rdr.GetInt32(3);
                                        break;

                                    case SqlDbType.NChar:
                                    case SqlDbType.NVarChar:
                                        column.Length = rdr.GetInt32(3);
                                        if (column.Length.HasValue && column.Length.Value > 0)
                                            column.Length = column.Length / 2;
                                        break;
                                }
                                if (!rdr.IsDBNull(4) && Int32.TryParse(rdr.GetString(4), out int prec))
                                    column.Precision = prec;
                                if (!rdr.IsDBNull(5) && Int32.TryParse(rdr.GetString(5), out int scale))
                                    column.Scale = scale;
                                column.IsNullable = rdr.GetString(6) == "yes";

                                if (table.Columns.ContainsKey(column.ColumnName))
                                    table.Columns.Remove(column.ColumnName);

                                table.Columns[column.ColumnName] = column;
                            }
                        }

                        // 3. Identity column details
                        if (rdr.NextResult())
                        {
                            while (rdr.Read())
                            {
                                string colname = rdr.GetString(0);
                                if (!rdr.IsDBNull(1) && !rdr.IsDBNull(2))
                                {
                                    int seed = (int)rdr.GetDecimal(1);
                                    int increment = (int)rdr.GetDecimal(2);

                                    if (table.Columns.TryGetValue(colname, out DbColumnInfo column))
                                    {
                                        column.IsIdentity = true;
                                        column.IdentitySeed = seed;
                                        column.IdentityIncrement = increment;
                                    }
                                }
                            }
                        }

                        // 4. RowGuid column details
                        if (rdr.NextResult())
                        {
                            while (rdr.Read())
                            {

                            }
                        }

                        // 5. File Group details
                        if (rdr.NextResult())
                        {
                            while (rdr.Read())
                            {

                            }
                        }

                        // 6. Index details
                        if (rdr.NextResult())
                        {
                            while (rdr.Read())
                            {
                                var index = new DbIndexInfo(table);
                                index.IndexName = rdr.GetString(0);
                                index.Description(rdr.GetString(1));
                                index.Keys(rdr.GetString(2));
                                index.Exists = true;
                                table.Indexes.Add(index);

                                if (index.IsPrimaryKey)
                                {
                                    foreach (var columnName in index.Columns.Keys)
                                    {
                                        if (table.Columns.TryGetValue(columnName, out DbColumnInfo column))
                                            column.IsPrimaryKey = true;
                                    }
                                }

                            }
                        }

                        // 7. Primary key, Foreign key and Default details
                        if (rdr.NextResult())
                        {
                            while (rdr.Read())
                            {
                                string type = rdr.GetString(0);
                                if (type.StartsWith("DEFAULT"))
                                {
                                    string columnName = type.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Last();
                                    string defname = rdr.GetString(1);
                                    string keys = rdr.GetString(6);
                                    if (table.Columns.TryGetValue(columnName, out DbColumnInfo column))
                                    {
                                        column.DefaultConstraintName = defname;
                                        if (keys.StartsWith("(") && keys.EndsWith(")"))
                                            column.DefaultValueExpression = keys.Substring(1, keys.Length - 2);
                                        else
                                            column.DefaultValueExpression = keys;
                                    }
                                }
                                if (type.StartsWith("FOREIGN KEY"))
                                {
                                    // First record is the Source table
                                    string fkname = rdr.GetString(1);
                                    string[] fromcols = rdr.GetString(6).ToColumnNameArray();

                                    // Second record is the Target table
                                    if (rdr.Read())
                                    {
                                        string reference = rdr.GetString(6);
                                        if (reference.StartsWith("REFERENCES "))
                                        {
                                            int index = reference.IndexOf('(');
                                            string[] target = reference.Substring(11, index - 11).Trim().Split('.');
                                            string[] tocols = reference.Substring(index).Trim('(', ')').ToColumnNameArray();

                                            string targetschema = target.Length > 1 ? target[target.Length - 2] : null;
                                            string targetname = target.Length > 0 ? target[target.Length - 1] : null;

                                            var refinfo = table.FindReferenceTo(targetschema, targetname, fromcols, tocols);
                                            if(refinfo == null)
                                            {
                                                // Create a new foreign key reference
                                                refinfo = new DbReferenceInfo(table);
                                                refinfo.SourceSchemaName = table.SchemaName;
                                                refinfo.SourceTableName = table.TableName;
                                                refinfo.ForeignKeyName = fkname;
                                                refinfo.SourceColumns = fromcols;
                                                refinfo.TargetSchemaName = targetschema;
                                                refinfo.TargetTableName = targetname;
                                                refinfo.TargetColumns = tocols;
                                                table.References.Add(refinfo);
                                            }
                                            else
                                            {
                                                // Database name overrides calculated name from the model
                                                refinfo.ForeignKeyName = fkname;
                                            }

                                            refinfo.ExistsInDatabase = true;
                                        }
                                    }
                                }
                                if (type.StartsWith("PRIMARY KEY"))
                                {

                                }
                            }
                        }

                        // 8. Foreign Keys referencing this table
                        if (rdr.NextResult())
                        {

                        }
                    }
                }
            }
        }

        private void LoadInfoFromModel(DbTableInfo table, EntitySet entityset)
        {
            // Get info about each mapped member
            foreach (EdmProperty member in entityset.ElementType.Members)
            {
                if (!table.Columns.TryGetValue(member.Name, out DbColumnInfo column))
                {
                    column = new DbColumnInfo(table);
                    column.ColumnName = member.Name;
                }

                if (member.MetadataProperties.Contains("PreferredName"))
                    column.PropertyName = (string)member.MetadataProperties["PreferredName"]?.Value ?? column.ColumnName;
                else
                    column.PropertyName = member.Name;

                table.Columns[column.ColumnName] = column;
                column.ExistsInModel = true;

                bool pk = entityset.ElementType.KeyMembers.Any(m => m.Name == member.Name);
                if (column.IsPrimaryKey != pk)
                {
                    column.IsPrimaryKey = pk;
                    column.Altered = true;
                }

                if (column.IsNullable != member.Nullable)
                {
                    column.IsNullable = member.Nullable;
                    column.Altered = true;
                }

                if (column.IsIdentity != (member.StoreGeneratedPattern == StoreGeneratedPattern.Identity))
                {
                    if (column.IsIdentity)
                    {
                        // This going to require a table rebuild - currently not supported and will be ignored
                        column.IdentityRemoved = true;
                        column.IsIdentity = false;
                    }
                    else
                        column.IsIdentity = true;
                    column.Altered = true;
                }

                string[] typeparts = member.TypeName.Split(new char[] { '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
                if (Enum.TryParse(typeparts.First(), true, out SqlDbType dbtype) && column.Type != dbtype)
                {
                    column.Type = dbtype;
                    column.Altered = true;
                }

                if (member.IsFixedLengthConstant)
                {
                    if (member.MaxLength.Value > Int16.MaxValue)
                    {
                        // Assume (MAX) specifier
                        if (column.Length > 0)
                        {
                            column.Length = -1;
                            column.Altered = true;
                        }
                    }
                    else
                    {
                        // Assume (n) specifier
                        if (column.Length != member.MaxLength)
                        {
                            column.Length = member.MaxLength;
                            column.Altered = true;
                        }
                    }
                }

                if (member.IsScaleConstant && column.Scale != member.Scale)
                {
                    switch (column.Type)
                    {
                        case SqlDbType.Decimal:
                        case SqlDbType.Float:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                            column.Scale = member.Scale;
                            column.Altered = true;
                            break;
                    }
                }

                if (member.IsPrecisionConstant && column.Precision != member.Precision)
                {
                    switch (column.Type)
                    {
                        case SqlDbType.Decimal:
                        case SqlDbType.Float:
                        case SqlDbType.Money:
                        case SqlDbType.Real:
                            column.Precision = member.Precision;
                            column.Altered = true;
                            break;
                    }
                }
            }
        }

        private void LoadReferenceInfo(IDictionary<string, DbTableInfo> tables, EntitySetMapping mapping)
        {
            // Now we can get the foreign key information from the model
            var entitytype = mapping.EntityTypeMappings.Single().EntityType;
            foreach (var member in entitytype.NavigationProperties)
            {
                var assoc = (AssociationType)member.RelationshipType;
                if (assoc.IsForeignKey && member.FromEndMember.RelationshipMultiplicity == RelationshipMultiplicity.Many)
                {
                    var fromtype = member.FromEndMember.GetEntityType();
                    var totype = member.ToEndMember.GetEntityType();

                    var tablefrom = tables.Values.FirstOrDefault(t => t.EntityType.FullName == fromtype.FullName);
                    var tableto = tables.Values.FirstOrDefault(t => t.EntityType.FullName == totype.FullName);

                    var constraint = assoc.ReferentialConstraints.Single();
                    // Note that the From/To Properties contain the properties from the _OTHER_ end of the relationship, so we sswitch them
                    string[] fromcols = constraint.ToProperties.Select(p => tablefrom.GetColumnName(p.Name)).ToArray();
                    string[] tocols = constraint.FromProperties.Select(p => tableto.GetColumnName(p.Name)).ToArray();

                    var reference = tablefrom.FindReferenceTo(tableto, fromcols, tocols);
                    if (reference == null)
                    {
                        // Need to create a Foreign Key reference
                        reference = new DbReferenceInfo(tablefrom);
                        reference.SourceSchemaName = tablefrom.SchemaName;
                        reference.SourceTableName = tablefrom.TableName;
                        reference.ForeignKeyName = $"FK_{assoc.Name}"; // Calculated name for new FK
                        reference.SourceColumns = fromcols;
                        reference.TargetSchemaName = tableto.SchemaName;
                        reference.TargetTableName = tableto.TableName;
                        reference.TargetColumns = tocols;

                        tablefrom.References.Add(reference);
                    }

                    reference.ExistsInModel = true;
                }
            }
        }

    }

    internal static class DbInfoLoaderExtensions
    {
        public static bool TryGetCustomAttribute<T>(this PropertyInfo prop, out T attrib) where T : Attribute
        {
            if (prop.IsDefined(typeof(T), true))
            {
                attrib = prop.GetCustomAttribute<T>();
                return true;
            }

            attrib = null;
            return false;
        }
    }

}
