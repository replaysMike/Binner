using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Binner.Data.Generators
{
    /// <summary>
    /// Applies custom translations to generated migrations for Sqlite
    /// 
    /// Important note: Modifications are applied on database update, not on add migration!
    /// </summary>
    /// <example>
    /// usage: 
    /// optionsBuilder.UseSqlite()
    ///     .ReplaceService&lt;IMigrationsSqlGenerator, SqliteCustomMigrationsSqlGenerator&gt;();
    /// </example>
    public class SqliteCustomMigrationsSqlGenerator : SqliteMigrationsSqlGenerator
    {
        private readonly StringComparison _stringComparison = StringComparison.InvariantCultureIgnoreCase;

        public SqliteCustomMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, IRelationalAnnotationProvider relationalAnnotationProvider)
            : base(dependencies, relationalAnnotationProvider)
        {
            Console.WriteLine($"  Using MigrationsSqlGenerator '{nameof(SqliteCustomMigrationsSqlGenerator)}'");
        }

        protected override void Generate(MigrationOperation operation, IModel? model, MigrationCommandListBuilder builder)
        {
            if (operation is CreateTableOperation createTableOperation)
            {
                foreach (var columnOperation in createTableOperation.Columns)
                {
                    ProcessColumnOperation(columnOperation);
                }
            }
            else if (operation is ColumnOperation columnOperation)
            {
                ProcessColumnOperation(columnOperation);
            }

            base.Generate(operation, model, builder);
        }

        private void ProcessColumnOperation(ColumnOperation columnOperation)
        {
            //PerformIdealFix(columnOperation);
            PerformAlternateFix(columnOperation);
        }

        private void PerformIdealFix(ColumnOperation columnOperation)
        {
            // translate getutcdate() to DATETIME('now')
            if (!string.IsNullOrEmpty(columnOperation.DefaultValueSql)
                && (columnOperation.DefaultValueSql.Equals("getutcdate()", _stringComparison) || columnOperation.DefaultValueSql.Equals("getdate()", _stringComparison)))
            {
                Console.WriteLine($"   ===== Overriding default date format on '{columnOperation.Table}.{columnOperation.Name}'");
                Console.WriteLine($"  Value: {columnOperation.Name}: {columnOperation.DefaultValueSql} ({columnOperation.ColumnType})");
                columnOperation.DefaultValueSql = TransformDefaultDateColumn(columnOperation.DefaultValueSql);
                Console.WriteLine($"  New Value: {columnOperation.Table}.{columnOperation.Name}: {columnOperation.DefaultValueSql} ({columnOperation.ColumnType})");
            }
        }

        private void PerformAlternateFix(ColumnOperation columnOperation)
        {
            // sqlite can only set a default datetime value if the table is empty: https://www.sqlite.org/omitted.html
            if (!string.IsNullOrEmpty(columnOperation.DefaultValueSql)
                && (columnOperation.DefaultValueSql.Equals("getutcdate()", _stringComparison) || columnOperation.DefaultValueSql.Equals("getdate()", _stringComparison)))
            {
                Console.WriteLine($"   ===== Overriding default date format on '{columnOperation.Table}.{columnOperation.Name}'");
                Console.WriteLine($"  Value: {columnOperation.Name}: {columnOperation.DefaultValueSql} ({columnOperation.ColumnType})");
                //columnOperation.DefaultValueSql = TransformDefaultDateColumn(columnOperation.DefaultValueSql);
                // Sqlite has an annoying inability to add a default datetime if the table is not empty.
                // "Cannot add a column with non-constant default"
                //columnOperation.DefaultValueSql = $"\"{DateTime.UtcNow:s}Z\"";
                //columnOperation.IsNullable = true;
                columnOperation.DefaultValueSql = null;
                columnOperation.DefaultValue = $"{DateTime.UtcNow:s}Z";
                columnOperation.ColumnType = "datetime";
                Console.WriteLine($"  New Value: {columnOperation.Table}.{columnOperation.Name}: {columnOperation.DefaultValue} ({columnOperation.ColumnType})");
            }else if (columnOperation.Name.Contains("Date") && columnOperation.ColumnType == "TEXT")
            {
                Console.WriteLine($"  Changed Type of: {columnOperation.Table}.{columnOperation.Name}: ({columnOperation.ColumnType}) to datetime");
                columnOperation.ColumnType = "datetime";
            }
        }

        private string? TransformDefaultDateColumn(string input)
        {
            // note surrounding () are required
            return input
                .Replace("getutcdate()", "DATETIME('now')", _stringComparison)
                .Replace("getdate()", "DATETIME('now')", _stringComparison);
        }
    }
}
