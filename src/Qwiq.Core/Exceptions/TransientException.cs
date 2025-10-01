using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Qwiq.Exceptions
{
    [Serializable]
    [DebuggerStepThrough]
    public class TransientException : Exception
    {
        public TransientException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.", DiagnosticId = "SYSLIB0051")]
        protected TransientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}