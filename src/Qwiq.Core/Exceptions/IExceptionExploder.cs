using System;
using System.Collections.ObjectModel;


namespace Qwiq.Exceptions
{
    public interface IExceptionExploder
    {
        ReadOnlyCollection<Exception> Explode(Exception exception);
    }
}

