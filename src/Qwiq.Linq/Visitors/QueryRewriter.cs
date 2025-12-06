using System;
using System.Linq;
using System.Linq.Expressions;
using System.Diagnostics;
using Qwiq.Linq.WiqlExpressions;

namespace Qwiq.Linq.Visitors
{
    // Developed from http://blogs.msdn.com/b/mattwar/archive/2007/07/30/linq-building-an-iqueryable-provider-part-i.aspx
    /// <summary>
    /// This class is going to visit each node in the query's expression
    /// tree and for expressions that don't map well into WIQL, replace those nodes with
    /// new expressions that map more closely to WIQL. Other nodes are reduced if possible
    /// and the rest are left alone.
    /// </summary>
    public class QueryRewriter : ExpressionVisitor
    {
        private static Expression StripQuotes(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Select")
            {
                var select = Visit(node.Arguments[0]);
                var projection = Visit(StripQuotes(node.Arguments[1])) as LambdaExpression;

                if (projection == null)
                {
                    throw new NotSupportedException("Performing a select without a lambda is not supported");
                }

                return new SelectExpression(node.Type, select, projection);
            }

            if (node.Method.DeclaringType == typeof(Queryable) && node.Method.Name == "Where")
            {
                var select = Visit(node.Arguments[0]);
                var filter = Visit(StripQuotes(node.Arguments[1]));

                return new WhereExpression(node.Type, select, filter);
            }

            if (node.Method.DeclaringType == typeof(Queryable) && (node.Method.Name == "OrderBy" || node.Method.Name == "ThenBy"))
            {
                var source = Visit(node.Arguments[0]);
                var orderSelector = Visit(StripQuotes(node.Arguments[1]));

                return new OrderExpression(node.Type, source, orderSelector, OrderOptions.Ascending);
            }

            if (node.Method.DeclaringType == typeof(Queryable) && (node.Method.Name == "OrderByDescending" || node.Method.Name == "ThenByDescending"))
            {
                var source = Visit(node.Arguments[0]);
                var orderSelector = Visit(StripQuotes(node.Arguments[1]));

                return new OrderExpression(node.Type, source, orderSelector, OrderOptions.Descending);
            }

            if (node.Method.DeclaringType == typeof(string) && node.Method.Name == "StartsWith")
            {
                var subject = Visit(node.Object);
                var target = Visit(node.Arguments[0]);

                return new UnderExpression(node.Type, subject!, target!);
            }

            if (node.Method.DeclaringType == typeof(QueryExtensions) && node.Method.Name == "WasEver")
            {
                var subject = Visit(node.Arguments[0]);
                var target = Visit(node.Arguments[1]);

                return new WasEverExpression(node.Type, subject, target);
            }

            if (node.Method.DeclaringType == typeof(QueryExtensions) && node.Method.Name == "InGroup")
            {
                var subject = Visit(node.Arguments[0]);
                var target = Visit(node.Arguments[1]);

                return new InGroupExpression(node.Type, subject, target);
            }

            if (node.Method.DeclaringType == typeof(QueryExtensions) && node.Method.Name == "NotInGroup")
            {
                var subject = Visit(node.Arguments[0]);
                var target = Visit(node.Arguments[1]);

                return new NotInGroupExpression(node.Type, subject, target);
            }

            // Handle Contains method calls
            if (node.Method.Name == "Contains")
            {
                var declaringType = node.Method.DeclaringType;
                System.IO.File.AppendAllText("/tmp/contains-debug.txt", 
                    $"Contains: DeclaringType={declaringType?.FullName}, Args={node.Arguments.Count}, Object={(node.Object != null ? "yes" : "null")}\n");
                
                // This is a contains used to do substring matching on a value, such as: bug => bug.Status.Contains("Approved")
                if (declaringType == typeof(string))
                {
                    var subject = Visit(node.Object);
                    var target = Visit(node.Arguments[0]);

                    return new ContainsExpression(node.Type, subject!, target!);
                }
                
                // This is a contains used to see if a value is in a list, such as: bug => aliases.Contains(bug.AssignedTo)
                // For now, accept ALL non-string Contains and figure out the argument order based on count
                // TODO: Add back restrictions for Collection<T>, HashSet<T> after fixing array support
                
                Expression subject, target;
                
                if (node.Arguments.Count == 2)
                {
                    // Extension method: Contains(source, value)
                    System.IO.File.AppendAllText("/tmp/contains-debug.txt", "  -> Using 2-arg extension method pattern\n");
                    subject = Visit(node.Arguments[1]);
                    target = Visit(node.Arguments[0]);
                }
                else if (node.Arguments.Count == 1)
                {
                    // Instance method syntax: source.Contains(value)
                    System.IO.File.AppendAllText("/tmp/contains-debug.txt", "  -> Using 1-arg instance method pattern\n");
                    subject = Visit(node.Arguments[0]);
                    target = Visit(node.Object!);
                }
                else
                {
                    // Unknown Contains signature
                    System.IO.File.AppendAllText("/tmp/contains-debug.txt", "  -> Unknown signature, falling through\n");
                    goto unknown_method;
                }

                return new InExpression(node.Type, subject, target);
            }
            
            unknown_method:

            if (node.Method.DeclaringType == typeof(QueryExtensions) && node.Method.Name == "AsOf")
            {
                Visit(node.Arguments[0]);
                var time = (ConstantExpression)Visit(node.Arguments[1])!;

                return new AsOfExpression(node.Type, (DateTime)time.Value!);
            }

            if (node.Method.Name == "ToUpper" || node.Method.Name == "ToUpperInvariant" || node.Method.Name == "ToLower" || node.Method.Name == "ToLowerInvariant")
            {
                throw new NotSupportedException($"The method {node.Method.Name} is not supported. Queries are case insensitive, so string comparisons should use the regular operators ( ==, > <=, etc.)");
            }

            if (node.Method.Name == "ToString")
            {
                return Expression.TypeAs(Visit(node.Object)!, typeof(string));
            }

            if (node.Method.Name == "get_Item")
            {
                var subject = node.Object;
                var name = Visit(node.Arguments[0])!;
                return new IndexerExpression(node.Type, subject, name);
            }

            // Unknown method call
            var declaringTypeName = node.Method.DeclaringType?.FullName ?? "null";
            throw new NotSupportedException($"The method '{node.Method.Name}' from type '{declaringTypeName}' is not supported");
        }
    }
}

