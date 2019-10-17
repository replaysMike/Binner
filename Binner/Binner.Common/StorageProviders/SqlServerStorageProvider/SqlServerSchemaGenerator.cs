using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using TypeSupport;
using TypeSupport.Extensions;

namespace Binner.Common.StorageProviders
{
    public class SqlServerSchemaGenerator<T>
    {
        private string _dbName;
        private ExtendedType _dbType;
        private ICollection<ExtendedProperty> _tables;

        public SqlServerSchemaGenerator(string databaseName)
        {
            _dbName = databaseName;
            _dbType = typeof(T).GetExtendedType();
            var properties = typeof(T).GetProperties(PropertyOptions.HasGetter);
            _tables = properties.Where(x => x.Type.GetExtendedType().IsCollection).ToList();
        }

        public string CreateDatabaseIfNotExists()
        {
            return $@"
IF (db_id(N'{_dbName}') IS NULL)
BEGIN
    CREATE DATABASE {_dbName};
END
";
        }

        public string CreateTableSchemaIfNotExists()
        {
            return $@"
    USE {_dbName};
    -- create tables
    {string.Join("\r\n", GetTableSchemas())}
";
        }

        private ICollection<string> GetTableSchemas()
        {
            var tableSchemas = new List<string>();
            foreach (var tableProperty in _tables)
            {
                var tableExtendedType = tableProperty.Type.GetExtendedType();
                var columnProps = tableExtendedType.ElementType.GetProperties(PropertyOptions.HasGetter);
                var tableSchema = new List<string>();
                foreach (var columnProp in columnProps)
                    tableSchema.Add(GetColumnSchema(columnProp));
                tableSchemas.Add(CreateTableIfNotExists(tableProperty.Name, string.Join(",\r\n", tableSchema)));
            }
            return tableSchemas;
        }

        private string GetColumnSchema(ExtendedProperty prop)
        {
            var columnSchema = "";
            var propExtendedType = prop.Type.GetExtendedType();
            var maxLength = GetMaxLength(prop);
            if (propExtendedType.IsCollection)
            {
                // store as string, data will be comma delimited
                columnSchema = $"{prop.Name} nvarchar({maxLength})";
            }
            else
            {
                switch (propExtendedType)
                {
                    case var p when p.NullableBaseType == typeof(byte):
                        columnSchema = $"{prop.Name} tinyint";
                        break;
                    case var p when p.NullableBaseType == typeof(short):
                        columnSchema = $"{prop.Name} smallint";
                        break;
                    case var p when p.NullableBaseType == typeof(int):
                        columnSchema = $"{prop.Name} integer";
                        break;
                    case var p when p.NullableBaseType == typeof(long):
                        columnSchema = $"{prop.Name} bigint";
                        break;
                    case var p when p.NullableBaseType == typeof(string):
                        columnSchema = $"{prop.Name} nvarchar({maxLength})";
                        break;
                    case var p when p.NullableBaseType == typeof(DateTime):
                        columnSchema = $"{prop.Name} datetime";
                        break;
                    case var p when p.NullableBaseType == typeof(TimeSpan):
                        columnSchema = $"{prop.Name} time";
                        break;
                    case var p when p.NullableBaseType == typeof(byte[]):
                        columnSchema = $"{prop.Name} varbinary({maxLength})";
                        break;
                    default:
                        throw new InvalidOperationException($"Unsupported data type: {prop.Type}");
                }
            }
            if (prop.CustomAttributes.ToList().Any(x => x.AttributeType == typeof(KeyAttribute)))
            {
                if (propExtendedType.NullableBaseType != typeof(string) && propExtendedType.NullableBaseType.IsValueType)
                    columnSchema = columnSchema + " IDENTITY";
                columnSchema = columnSchema + " PRIMARY KEY NOT NULL";
            }
            else if (propExtendedType.Type != typeof(string) && !propExtendedType.IsNullable && !propExtendedType.IsCollection)
                columnSchema = columnSchema + " NOT NULL";
            return columnSchema;
        }

        private string GetMaxLength(ExtendedProperty prop)
        {
            var maxLengthAttr = prop.CustomAttributes.ToList().FirstOrDefault(x => x.AttributeType == typeof(MaxLengthAttribute));
            var maxLength = "max";
            if (maxLengthAttr != null)
            {
                maxLength = maxLengthAttr.ConstructorArguments.First().Value.ToString();
            }
            return maxLength;
        }

        private string CreateTableIfNotExists(string tableName, string tableSchema)
        {
            return $@"IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='{tableName}' and xtype='U')
BEGIN
    CREATE TABLE {tableName} (
        {tableSchema}
    );
END";
        }
    }
}
