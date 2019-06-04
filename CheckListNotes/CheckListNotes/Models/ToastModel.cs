namespace CheckListNotes.Models
{
    using System;
    using PortableClasses.Enums;
    using PortableClasses.Interfaces;
    public class ToastModel : IToast
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
        public DateTime Time { get; set; }
        public ToastTypes Type { get; set; }
        public string Arguments { get; set; }
    }
}
