using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TypeSupport.Extensions;

namespace Binner.Common.StorageProviders
{
    /// <summary>
    /// Builds SQL Where condition based on an expression tree
    /// </summary>
    public class SqlWhereExpressionBuilder
    {
        /// <summary>
        /// Convert a where condition to parameterized SQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public WhereCondition ToParameterizedSql<T>(Expression<Func<T, bool>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var i = 1;
            return RecurseExpression(ref i, expression.Body, isUnary: true);
        }

        /// <summary>
        /// Convert a where condition to SQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public string ToSql<T>(Expression<Func<T, bool>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var i = 1;
            var whereParts = RecurseExpression(ref i, expression.Body, isUnary: true);

            if (!whereParts.Parameters.Any())
                return whereParts.Sql;

            var finalQuery = new StringBuilder();
            finalQuery.Append(whereParts.Sql);
            foreach (var p in whereParts.Parameters)
            {
                var val = $"@{p.Key}";
                finalQuery = finalQuery.Replace(val, p.Value.ToString());
            }
            return finalQuery.ToString();
        }

        private WhereCondition RecurseExpression(ref int i, Expression expression, string callStackName = null, bool isUnary = false, string prefix = null, string postfix = null)
        {
            if (expression is UnaryExpression unary)
            {
                return WhereCondition.Concat(NodeTypeToString(unary.NodeType), RecurseExpression(ref i, unary.Operand, null, true));
            }
            if (expression is BinaryExpression body)
            {
                return WhereCondition.Concat(RecurseExpression(ref i, body.Left), NodeTypeToString(body.NodeType), RecurseExpression(ref i, body.Right));
            }
            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression)expression;
                var value = constant.Value;
                if (value is int)
                {
                    return WhereCondition.IsSql(value.ToString());
                }
                if (value is string)
                {
                    if (prefix == null && postfix == null)
                        value = "'" + (string)value + "'";
                    else
                        value = prefix + (string)value + postfix;
                }
                if (value is DateTime)
                {
                    value = "'" + value + "'";
                }
                if (value is DateTime?)
                {
                    value = "'" + (string)value + "'";
                }
                if (value is TimeSpan)
                {
                    value = "'" + value + "'";
                }
                if (value is TimeSpan?)
                {
                    value = "'" + (string)value + "'";
                }
                if (value is bool && isUnary)
                {
                    return WhereCondition.Concat(WhereCondition.IsParameter(i++, value), "=", WhereCondition.IsSql("1"));
                }
                return WhereCondition.IsParameter(i++, value);
            }
            if (expression is MemberExpression member)
            {
                var memberName = member.Member.Name;

                if (member.Expression is MemberExpression)
                {
                    return RecurseExpression(ref i, member.Expression, memberName);
                }
                if (member.Expression is ConstantExpression)
                {
                    var name = member.Member.Name;
                    var constantExpression = member.Expression as ConstantExpression;
                    var val = constantExpression.Value.GetFieldValue(name);
                    var properties = val.GetProperties(PropertyOptions.HasGetter);
                    object callStackValue = null;
                    if (properties.Any(x => x.Name == callStackName))
                        callStackValue = val.GetPropertyValue(callStackName);
                    else
                        callStackValue = val.GetFieldValue(callStackName);

                    return WhereCondition.IsParameter(i++, callStackValue);
                }
                if (member.Member is PropertyInfo property)
                {
                    var colName = property.Name;
                    if (isUnary && member.Type == typeof(bool))
                    {
                        return WhereCondition.Concat(RecurseExpression(ref i, expression), "=", WhereCondition.IsParameter(i++, true));
                    }
                    return WhereCondition.IsSql("[" + colName + "]");
                }
                if (member.Member is FieldInfo)
                {
                    var value = GetValue<object>(member);
                    if (value is string)
                    {
                        value = prefix + (string)value + postfix;
                    }
                    return WhereCondition.IsParameter(i++, value);
                }
                throw new ParseException($"Expression does not refer to a known property or field: {expression}");
            }
            if (expression is MethodCallExpression methodCall)
            {
                // LIKE queries:
                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                {
                    return WhereCondition.Concat(RecurseExpression(ref i, methodCall.Object), "LIKE", RecurseExpression(ref i, methodCall.Arguments[0], prefix: "'%", postfix: "%'"));
                }
                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                {
                    return WhereCondition.Concat(RecurseExpression(ref i, methodCall.Object), "LIKE", RecurseExpression(ref i, methodCall.Arguments[0], prefix: "'", postfix: "%'"));
                }
                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                {
                    return WhereCondition.Concat(RecurseExpression(ref i, methodCall.Object), "LIKE", RecurseExpression(ref i, methodCall.Arguments[0], prefix: "'%", postfix: "'"));
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains")
                {
                    Expression collection;
                    Expression property;
                    if (methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 2)
                    {
                        collection = methodCall.Arguments[0];
                        property = methodCall.Arguments[1];
                    }
                    else if (!methodCall.Method.IsDefined(typeof(ExtensionAttribute)) && methodCall.Arguments.Count == 1)
                    {
                        collection = methodCall.Object;
                        property = methodCall.Arguments[0];
                    }
                    else
                    {
                        throw new ParseException($"Unsupported method call: {methodCall.Method.Name}");
                    }
                    var values = (IEnumerable)GetValue<object>(collection);
                    return WhereCondition.Concat(RecurseExpression(ref i, property), "IN", WhereCondition.IsCollection(ref i, values));
                }
                throw new ParseException($"Unsupported method call: {methodCall.Method.Name}");
            }
            throw new ParseException($"Unsupported expression: {expression.GetType().Name}");
        }

        private static object GetValue<T>(Expression member)
        {
            var objectMember = Expression.Convert(member, typeof(T));
            var getterLambda = Expression.Lambda<Func<T>>(objectMember);
            var getter = getterLambda.Compile();
            return getter();
        }

        private static string NodeTypeToString(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Add:
                    return "+";
                case ExpressionType.And:
                    return "&";
                case ExpressionType.AndAlso:
                    return "AND";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.ExclusiveOr:
                    return "^";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.Modulo:
                    return "%";
                case ExpressionType.Multiply:
                    return "*";
                case ExpressionType.Negate:
                    return "-";
                case ExpressionType.Not:
                    return "NOT";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                    return "|";
                case ExpressionType.OrElse:
                    return "OR";
                case ExpressionType.Subtract:
                    return "-";
                case ExpressionType.Convert:
                    // todo: for now, require no conversion and assume implicit
                    return "";
                default:
                    throw new ParseException($"Unsupported expression node type: {nodeType}");
            }
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message) { }
    }

    public class WhereCondition
    {
        public string Sql { get; set; }
        public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

        public static WhereCondition IsSql(string sql)
        {
            return new WhereCondition()
            {
                Parameters = new Dictionary<string, object>(),
                Sql = sql
            };
        }

        public static WhereCondition IsParameter(int count, object value)
        {
            return new WhereCondition()
            {
                Parameters = { { count.ToString(), value } },
                Sql = $"@{count}"
            };
        }

        public static WhereCondition IsCollection(ref int countStart, IEnumerable values)
        {
            var parameters = new Dictionary<string, object>();
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
