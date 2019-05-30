namespace PortableClasses.Interfaces
{
    using System;
    using PortableClasses.Enums;
    public interface IToast
    {
        string Id { get; set; }
        string Title { get; set; }
        string Body { get; set; }
        DateTime Time { get; set; }
        ToastTypes Type { get; set; }
    }
}
