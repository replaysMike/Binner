using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Binner.Common.Types
{
    public static class AttributeHelpers
    {
        /// <summary>
        /// Get the length value on <typeparamref name="T"/> decorated with <see cref="MaxLengthAttribute"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public static Int32 GetMaxLength<T>(Expression<Func<T, string>> propertyExpression)
        {
            return GetPropertyAttributeValue<T, string, MaxLengthAttribute, Int32>(propertyExpression, attr => attr.Length);
        }

        public static TValue GetPropertyAttributeValue<T, TOut, TAttribute, TValue>(Expression<Func<T, TOut>> propertyExpression, Func<TAttribute, TValue> valueSelector) where TAttribute : Attribute
        {
            var expression = (MemberExpression)propertyExpression.Body;
            var propertyInfo = (PropertyInfo)expression.Member;
            var attr = propertyInfo.GetCustomAttributes(typeof(TAttribute), true).FirstOrDefault() as TAttribute;

            if (attr == null)
            {
                throw new MissingMemberException(typeof(T).Name + "." + propertyInfo.Name, typeof(TAttribute).Name);
            }

            return valueSelector(attr);
        }
    }
}
