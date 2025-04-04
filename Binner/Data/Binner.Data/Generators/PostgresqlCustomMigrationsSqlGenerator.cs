using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;
using Npgsql.EntityFrameworkCore.PostgreSQL.Migrations;

namespace Binner.Data.Generators
{
    /// <summary>
    /// Applies custom translations to generated migrations for Postgresql.
    /// 
    /// Important note: Modifications are applied on database update, not on add migration!
    /// </summary>
    /// <example>
    /// usage: 
    /// optionsBuilder.UseNpgsql()
    ///     .ReplaceService&lt;IMigrationsSqlGenerator, PostgresqlCustomMigrationsSqlGenerator&gt;();
    /// </example>
    public class PostgresqlCustomMigrationsSqlGenerator : NpgsqlMigrationsSqlGenerator
    {
        private readonly StringComparison _stringComparison = StringComparison.InvariantCultureIgnoreCase;

        public PostgresqlCustomMigrationsSqlGenerator(MigrationsSqlGeneratorDependencies dependencies, INpgsqlSingletonOptions options)
            : base(dependencies, options)
        {
            //Console.WriteLine($"  Using MigrationsSqlGenerator '{nameof(PostgresqlCustomMigrationsSqlGenerator)}'");
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
            // translate the default date value to a postgresql compatible function
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
                .Replace("getutcdate()", "timezone('utc', now())", _stringComparison)
                .Replace("getdate()", "now()", _stringComparison);
        }
    }
}
