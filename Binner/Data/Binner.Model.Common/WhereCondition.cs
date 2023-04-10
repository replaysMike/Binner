using System.Collections;
using System.Text;

namespace Binner.Model.Common
{
    public class WhereCondition
    {
        public string? Sql { get; set; }
        public Dictionary<string, object?> Parameters { get; set; } = new Dictionary<string, object?>();

        public static WhereCondition IsSql(string? sql)
        {
            return new WhereCondition()
            {
                Parameters = new Dictionary<string, object?>(),
                Sql = sql
            };
        }

        public static WhereCondition IsParameter(int count, object? value)
        {
            return new WhereCondition()
            {
                Parameters = { { count.ToString(), value } },
                Sql = $"@{count}"
            };
        }

        public static WhereCondition IsCollection(ref int countStart, IEnumerable values)
        {
            var parameters = new Dictionary<string, object?>();
            var sql = new StringBuilder("(");
            foreach (var value in values)
            {
                parameters.Add((countStart).ToString(), value);
                sql.Append($"@{countStart},");
                countStart++;
            }
            if (sql.Length == 1)
            {
                sql.Append("null,");
            }
            sql[sql.Length - 1] = ')';
            return new WhereCondition()
            {
                Parameters = parameters,
                Sql = sql.ToString()
            };
        }

        public static WhereCondition Concat(string @operator, WhereCondition operand)
        {
            return new WhereCondition()
            {
                Parameters = operand.Parameters,
                Sql = $"({@operator} {operand.Sql})"
            };
        }

        public static WhereCondition Concat(WhereCondition left, string @operator, WhereCondition right)
        {
            return new WhereCondition()
            {
                Parameters = left.Parameters.Union(right.Parameters).ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                Sql = $"({left.Sql} {@operator} {right.Sql})"
            };
        }
    }


}
