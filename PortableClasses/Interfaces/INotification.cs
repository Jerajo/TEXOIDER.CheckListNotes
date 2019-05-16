namespace PortableClasses.Interfaces
{
    using System;
    public interface IToast
    {
        string Id { get; set; }
        string Title { get; set; }
        string Body { get; set; }
        DateTime Time { get; set; }
    }
}
