using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using TypeSupport.Extensions;

namespace Binner.Model.Common
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
        /// <param name="quotePropertyNames">True to surround property names with double quotes</param>
        /// <param name="encapsulatePropertyNames">True to surround property names with []</param>
        /// <returns></returns>
        public WhereCondition ToParameterizedSql<T>(Expression<Func<T, bool>> expression, bool quotePropertyNames = false, bool encapsulatePropertyNames = true)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var i = 1;
            return RecurseExpression(ref i, expression.Body, isUnary: true, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames);
        }

        /// <summary>
        /// Convert a where condition to SQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <param name="quotePropertyNames">True to surround property names with double quotes</param>
        /// <param name="encapsulatePropertyNames">True to surround property names with []</param>
        /// <returns></returns>
        public string? ToSql<T>(Expression<Func<T, bool>> expression, bool quotePropertyNames = false, bool encapsulatePropertyNames = true)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            var i = 1;
            var whereParts = RecurseExpression(ref i, expression.Body, isUnary: true, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames);

            if (!whereParts.Parameters.Any())
                return whereParts?.Sql;

            var finalQuery = new StringBuilder();
            finalQuery.Append(whereParts.Sql);
            foreach (var p in whereParts.Parameters)
            {
                var val = $"@{p.Key}";
                finalQuery = finalQuery.Replace(val, p.Value?.ToString());
            }
            return finalQuery.ToString();
        }

        private WhereCondition RecurseExpression(ref int i, Expression? expression, string? callStackName = null, bool isUnary = false, string? prefix = null, string? postfix = null, bool quotePropertyNames = false, bool encapsulatePropertyNames = true)
        {
            if (expression is null)
                return new WhereCondition();
            if (expression is UnaryExpression unary)
            {
                return WhereCondition.Concat(NodeTypeToString(unary.NodeType), RecurseExpression(ref i, unary.Operand, null, true, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames));
            }
            if (expression is BinaryExpression body)
            {
                return WhereCondition.Concat(RecurseExpression(ref i, body.Left, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames), NodeTypeToString(body.NodeType), RecurseExpression(ref i, body.Right, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames));
            }
            if (expression is ConstantExpression constant)
            {
                var value = constant.Value;
                if (value is null)
                    return new WhereCondition();
                if (value is int)
                {
                    return WhereCondition.IsSql(value?.ToString());
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
                    return RecurseExpression(ref i, member.Expression, memberName, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames);
                }
                if (member.Expression is ConstantExpression)
                {
                    var name = member.Member.Name;
                    var constantExpression = member.Expression as ConstantExpression;
                    var value = constantExpression?.Value.GetFieldValue(name);
                    var properties = value.GetProperties(PropertyOptions.HasGetter);
                    object? callStackValue = null;
                    if (properties.Any(x => x.Name == callStackName))
                        callStackValue = value.GetPropertyValue(callStackName);
                    else
                        callStackValue = value.GetFieldValue(callStackName);

                    return WhereCondition.IsParameter(i++, callStackValue);
                }
                if (member.Member is PropertyInfo property)
                {
                    var columnName = property.Name;
                    if (isUnary && member.Type == typeof(bool))
                    {
                        return WhereCondition.Concat(RecurseExpression(ref i, expression, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames), "=", WhereCondition.IsParameter(i++, true));
                    }
                    return WhereCondition.IsSql(Surround(columnName, quotePropertyNames, encapsulatePropertyNames));
                }
                if (member.Member is FieldInfo)
                {
                    var value = GetValue<object>(member);
                    if (value is string)
                    {
                        value = prefix + (string)value + postfix;
                    }
                    if (value is null)
                        throw new ParseException($"Expression does not refer to a known property or field: {expression}");
                    return WhereCondition.IsParameter(i++, value);
                }
                throw new ParseException($"Expression does not refer to a known property or field: {expression}");
            }
            if (expression is MethodCallExpression methodCall)
            {
                // LIKE queries:
                if (methodCall.Method == typeof(string).GetMethod("Contains", new[] { typeof(string) }))
                {
                    return WhereCondition.Concat(RecurseExpression(ref i, methodCall.Object, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames), "LIKE", RecurseExpression(ref i, methodCall.Arguments[0], prefix: "'%", postfix: "%'", quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames));
                }
                if (methodCall.Method == typeof(string).GetMethod("StartsWith", new[] { typeof(string) }))
                {
                    return WhereCondition.Concat(RecurseExpression(ref i, methodCall.Object, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames), "LIKE", RecurseExpression(ref i, methodCall.Arguments[0], prefix: "'", postfix: "%'", quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames));
                }
                if (methodCall.Method == typeof(string).GetMethod("EndsWith", new[] { typeof(string) }))
                {
                    return WhereCondition.Concat(RecurseExpression(ref i, methodCall.Object, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames), "LIKE", RecurseExpression(ref i, methodCall.Arguments[0], prefix: "'%", postfix: "'", quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames));
                }
                // IN queries:
                if (methodCall.Method.Name == "Contains")
                {
                    Expression? collection;
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
                    if (collection is null)
                        throw new ParseException($"Unsupported collection: {methodCall.Method.Name}");
                    var values = (IEnumerable?)GetValue<object>(collection);
                    if (values is null)
                        throw new ParseException($"Unsupported null values: {collection.Type.Name}");
                    return WhereCondition.Concat(RecurseExpression(ref i, property, quotePropertyNames: quotePropertyNames, encapsulatePropertyNames: encapsulatePropertyNames), "IN", WhereCondition.IsCollection(ref i, values));
                }
                throw new ParseException($"Unsupported method call: {methodCall.Method.Name}");
            }
            throw new ParseException($"Unsupported expression: {expression.GetType().Name}");
        }

        private static string Surround(string columnName, bool quotePropertyNames, bool encapsulatePropertyNames)
        {
            var columnNameFormatted = columnName;
            if (encapsulatePropertyNames)
                columnNameFormatted = $"[{columnNameFormatted}]";
            if (quotePropertyNames)
                columnNameFormatted = @$"""{columnNameFormatted}""";
            return columnNameFormatted;
        }

        private static object? GetValue<T>(Expression member)
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
}
