namespace PortableClasses.Helpers
{
    using System;
    public class ExplicitGarbageCollector : IDisposable
    {
        public ExplicitGarbageCollector() { }
        ~ExplicitGarbageCollector() { }
        public void Dispose() { }
    }
}
