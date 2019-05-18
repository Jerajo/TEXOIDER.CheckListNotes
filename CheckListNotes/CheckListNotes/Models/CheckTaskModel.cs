namespace CheckListNotes.Models
{
    using System;
    using PortableClasses.Enums;
    using PortableClasses.Interfaces;
    public class CheckTaskModel : IIdentity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ToastId { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public ToastTypesTime NotifyOn { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDaily { get; set; }
    }
}
