using System.Linq.Expressions;
using System.Reflection;

namespace Binner.StorageProvider.EntityFrameworkCore
{
    // I've shamelessly taken this from: https://codereview.stackexchange.com/questions/193435/expression-conversion
    public class ExpressionConverter<TFrom, TTo> : ExpressionVisitor
        where TFrom : class, new()
        where TTo : class, new()
    {
        private readonly MappedConverter<TFrom, TTo> _converter;
        private ParameterExpression? _fromParameter;
        private ParameterExpression? _toParameter;

        public ExpressionConverter(MappedConverter<TFrom, TTo> converter)
        {
            _converter = converter;
        }

        public override Expression? Visit(Expression? node)
        {
            if (_fromParameter == null)
            {
                if (node == null || node.NodeType != ExpressionType.Lambda)
                {
                    throw new ArgumentException("Expression must be a lambda");
                }

                var lambda = (LambdaExpression)node;
                if (lambda.ReturnType != typeof(bool) || lambda.Parameters.Count != 1 ||
                    lambda.Parameters[0].Type != typeof(TFrom))
                {
                    throw new ArgumentException("Expression must be a Func<TFrom, bool>");
                }

                _fromParameter = lambda.Parameters[0];
                _toParameter = Expression.Parameter(typeof(TTo), _fromParameter.Name);
            }
            return base.Visit(node);
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (_fromParameter == node)
            {
                return _toParameter;
            }
            return base.VisitParameter(node);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == _fromParameter)
            {
                var member = _converter.GetMappingFromMemberName<TFrom>(node.Member.Name);
                if (member == null) throw new NullReferenceException();
                return Expression.Property(_toParameter, member);
            }

            return base.VisitMember(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (typeof(T) == typeof(Func<TFrom, bool>))
            {
                return Expression.Lambda<Func<TTo, bool>>(Visit(node.Body), new[] { _toParameter });
            }
            return base.VisitLambda(node);
        }
    }

    public class MappedConverter<Dto, Entity> where Dto : class, new() where Entity : class, new()
    {
        public List<Mapping<Dto, Entity>> Mappings { get; set; }

        public MappedConverter(params Mapping<Dto, Entity>[] maps)
        {
            Mappings = maps.ToList();
        }

        public PropertyInfo? GetMappingFromMemberName<T>(string name)
        {
            if (typeof(T) == typeof(Dto))
            {
                return Mappings.SingleOrDefault(x => x.DtoProperty?.Name == name)?.EntityProperty;
            }
            else if (typeof(T) == typeof(Entity))
            {
                return Mappings.SingleOrDefault(x => x.EntityProperty?.Name == name)?.DtoProperty;
            }
            throw new Exception($"Cannot get mapping for {typeof(T).Name} from MappedConverter<{typeof(Dto).Name}, {typeof(Entity).Name}>");
        }
    }

    public class Mapping<Dto, Entity>
    {
        public PropertyInfo? DtoProperty { get; }
        public PropertyInfo? EntityProperty { get; }
        public Mapping(Expression<Func<Dto, object>> dtoPropertyExpression, Expression<Func<Entity, object>> entityPropertyExpression)
        {
            DtoProperty = GetPropertyPathInternal(dtoPropertyExpression.Body);
            EntityProperty = GetPropertyPathInternal(entityPropertyExpression.Body);
        }

        private PropertyInfo? GetPropertyPathInternal(Expression expression)
        {
            switch (expression)
            {
                case MemberExpression m:
                    {
                        return m.Member.DeclaringType?.GetProperty(m.Member.Name);
                    }
                case UnaryExpression u when u.Operand is MemberExpression m:
                    {
                        return m.Member.DeclaringType?.GetProperty(m.Member.Name);
                    }
                case LambdaExpression l when l.Body is MemberExpression m:
                    {
                        return m.Member.DeclaringType?.GetProperty(m.Member.Name);
                    }
                case LambdaExpression l when l.Body is UnaryExpression u:
                    {
                        var operand = (MemberExpression)u.Operand;
                        return operand.Member.DeclaringType?.GetProperty(operand.Member.Name);
                    }
                case LambdaExpression l when l.Body is MethodCallExpression m:
                    {
                    }
                    break;
                case MethodCallExpression mc when mc.Method is MethodInfo m:
                    {
                    }
                    break;
                default:
                    throw new NotImplementedException(expression.GetType().ToString());
            }
            return null;
        }
    }
}
