using System;

namespace SeasonViewer.Core
{
    public interface IDatabaseService
    {
        void Do(Action<IDatabaseContext> action);
        T Do<T>(Func<IDatabaseContext, T> action);
    }
}