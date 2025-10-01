using System;
using System.Runtime.Serialization;

namespace Qwiq
{
    [Serializable]
    public class PageSizeRangeException : ApplicationException
    {
        public PageSizeRangeException()
            :base("TF237117: PageSize has to be between 50 and 200")
        {

        }

        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051")]
        protected PageSizeRangeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
