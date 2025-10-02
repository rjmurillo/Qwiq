using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;


namespace Qwiq.Linq
{
    public static class Projector
    {
        public static object Project( IEnumerable<LambdaExpression> projections, IEnumerable<object> data)
        {
            if (projections == null) throw new ArgumentNullException(nameof(projections));
            if (data == null) throw new ArgumentNullException(nameof(data));

            var projectedData = data;
            foreach (var projection in projections)
            {
                Debug.Assert(projection != null, "projection != null");
                var compiledProjection = projection.Compile();
                projectedData = projectedData.Select(r => compiledProjection.DynamicInvoke(r));
            }

            return projectedData;
        }
    }
}

