﻿namespace CheckListNotes.Models
{
    using System;
    using System.Diagnostics;
    using PortableClasses.Enums;
    using PortableClasses.Interfaces;
    using System.Collections.Generic;
    public class CheckTaskModel : IIdentity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int? Position { get; set; }
        public string ToastId { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public List<CheckTaskModel> SubTasks { get; set; }
        public ToastTypesTime? NotifyOn { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool? IsTaskGroup { get; set; }
        public bool? IsChecked { get; set; }
        public bool? IsDaily { get; set; }

        #region Disose

        ~CheckTaskModel()
        {
            Id = null;
            Name = null;
            ToastId = null;
            Position = null;
            ReminderTime = null;
            ExpirationDate = null;
            CompletedDate = null;
            IsTaskGroup = null;
            IsChecked = null;
            NotifyOn = null;
            IsDaily = null;
            if (SubTasks != null)
            {
                SubTasks.Clear();
                SubTasks = null;
            }
#if DEBUG
            //Debug.WriteLine($"Object destroyect: Name: {nameof(CheckTaskModel)}, Id: {this.GetHashCode()}].");
#endif
        }
        #endregion
    }
}
