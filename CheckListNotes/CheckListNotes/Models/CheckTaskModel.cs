namespace CheckListNotes.Models
{
    using System;
    using PortableClasses.Interfaces;
    using CheckListNotes.Models.Enums;
    public class CheckTaskModel : IIdentity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ToastId { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public NotificationTime NotifyOn { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDaily { get; set; }
    }
}
