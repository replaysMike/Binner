using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Update;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure.Internal;
using Pomelo.EntityFrameworkCore.MySql.Migrations;

namespace Binner.Data.Generators
{
    /// <summary>
    /// Applies custom translations to generated migrations for MySql/MariaDb.
    /// 
    /// Important note: Modifications are applied on database update, not on add migration!
    /// </summary>
    /// <example>
    /// usage: 
    /// optionsBuilder.UseMySql()
    ///     .ReplaceService&lt;IMigrationsSqlGenerator, MySqlCustomMigrationsSqlGenerator&gt;();
    /// </example>
    public class MySqlCustomMigrationsSqlGenerator : MySqlMigrationsSqlGenerator
    {
        private readonly StringComparison _stringComparison = StringComparison.InvariantCultureIgnoreCase;

        public MySqlCustomMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, ICommandBatchPreparer batchPreparer, IMySqlOptions options)
            : base(dependencies, batchPreparer, options)
        {
            //Console.WriteLine($"  Using MigrationsSqlGenerator '{nameof(MySqlCustomMigrationsSqlGenerator)}'");
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
            // translate the default date value to a mysql compatible function
            if (!string.IsNullOrEmpty(columnOperation.DefaultValueSql)
                && (columnOperation.DefaultValueSql.Equals("getutcdate()", _stringComparison) || columnOperation.DefaultValueSql.Equals("getdate()", _stringComparison)))
            {
                //Console.WriteLine($"   ===== Overriding default date format on '{columnOperation.Name}'");
                //Console.WriteLine($"  Value: {columnOperation.Name}: {columnOperation.DefaultValueSql}");
                columnOperation.DefaultValueSql = TransformDefaultDateColumn(columnOperation.DefaultValueSql);
                //Console.WriteLine($"  New Value: {columnOperation.Name}: {columnOperation.DefaultValueSql}");
            }
        }

        private string? TransformDefaultDateColumn(string input)
        {
            return input
                .Replace("getutcdate()", "(UTC_TIMESTAMP())", _stringComparison)
                .Replace("getdate()", "(CURRENT_TIMESTAMP())", _stringComparison);
        }
    }
}
