using Binner.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Binner.Web.ServiceHost
{
    public partial class BinnerWebHostService
    {
        private async Task ApplyOrganizationIdPatchAsync(BinnerContext context)
        {
            await PatchTableAsync(context, x => x.Users);
            await PatchTableAsync(context, x => x.OAuthCredentials);
            await PatchTableAsync(context, x => x.OAuthRequests);
            await PatchTableAsync(context, x => x.Parts);
            await PatchTableAsync(context, x => x.PartSuppliers);
            await PatchTableAsync(context, x => x.PartTypes);
            await PatchTableAsync(context, x => x.Pcbs);
            await PatchTableAsync(context, x => x.PcbStoredFileAssignments);
            await PatchTableAsync(context, x => x.ProjectPartAssignments);
            await PatchTableAsync(context, x => x.ProjectPcbAssignments);
            await PatchTableAsync(context, x => x.Projects);
            await PatchTableAsync(context, x => x.StoredFiles);
            //await PatchTableAsync(context, x => x.OrganizationIntegrationConfigurations); // skip, no longer has a UserId
            //await PatchTableAsync(context, x => x.UserLoginHistory); // skip this one, having an OrganizationId = null is valid if user didn't match
            await PatchTableAsync(context, x => x.UserPrinterConfigurations);
            await PatchTableAsync(context, x => x.UserPrinterTemplateConfigurations);
            await PatchTableAsync(context, x => x.UserTokens);
        }

        private async Task PatchTableAsync<T>(BinnerContext context, Func<BinnerContext, DbSet<T>> expression)
        where T : class
        {
            var propertiesToPatch = new[] { "OrganizationId", "UserId" };
            try
            {
                foreach (var propertyName in propertiesToPatch)
                {
                    var parameter = Expression.Parameter(typeof(T), "e");
                    var propExpression = Expression.Property(parameter, propertyName); // OrganizationId, UserId
                    var value = 0;
                    Expression<Func<T, bool>> filterLambda;
                    if (propExpression.Type == typeof(Int32))
                    {
                        var equalCondition = Expression.Equal(propExpression, Expression.Constant(value));
                        filterLambda = Expression.Lambda<Func<T, bool>>(equalCondition, parameter);
                    }
                    else if (Nullable.GetUnderlyingType(propExpression.Type) == typeof(Int32))
                    {
                        // create an expression that does an EqualTo value OR EqualTo null
                        var areEqual = Expression.Equal(propExpression, Expression.Convert(Expression.Constant(value), propExpression.Type));
                        var isNull = Expression.Equal(propExpression, Expression.Convert(Expression.Constant(null), propExpression.Type));

                        var expr1 = Expression.Lambda<Func<T, bool>>(areEqual, parameter);
                        var expr2 = Expression.Lambda<Func<T, bool>>(isNull, parameter);
                        var body = Expression.Or(expr1.Body, expr2.Body);
                        filterLambda = Expression.Lambda<Func<T, bool>>(body, expr1.Parameters[0]);
                    }
                    else
                    {
                        throw new Exception($"Unexpected type: {propExpression.Type}");
                    }

                    var records = await expression.Invoke(context).Where(filterLambda).ToListAsync();
                    foreach (var record in records)
                    {
                        var type = record.GetType();
                        var pi = type.GetProperty("OrganizationId");
                        pi.SetValue(record, 1);
                    }

                    var updatedCount = await context.SaveChangesAsync();
                    if (updatedCount > 0) _logger!.LogWarning($"Patched {updatedCount} {typeof(T).Name}s missing {propertyName}!");
                }
            }
            catch (Exception ex)
            {
                // log the error
                _logger!.LogError(ex, "PatchTableAsync encountered an unexpected error!");
            }
        }
    }
}
