namespace CheckListNotes.Models
{
    using System;
    using PortableClasses.Enums;
    using PortableClasses.Interfaces;
    using System.Collections.Generic;
    using PortableClasses.Extensions;
    public class CheckTaskModel : IIdentity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int LastSubId { get; set; }
        public string ToastId { get; set; }
        public DateTime? ReminderTime { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public List<CheckTaskModel> SubTasks { get; set; }
        public ToastTypesTime NotifyOn { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool IsTaskGroup { get; set; }
        public bool IsChecked { get; set; }
        public bool IsDaily { get; set; }
    }

    public static class CheckTaskModelExtension
    {
        public static void Remove(this CheckTaskModel model, 
            CheckListTasksModel parent)
        {
            if (model.Id.Contains("."))
            {
                var parentIndex = model.Id.RemoveLastSplit('.');
                var parentTask = GlobalDataService.IndexNavigation(parent.CheckListTasks, parentIndex);
                parentTask.SubTasks.Remove(model);
            }
            else parent.CheckListTasks.Remove(model);
        }
    }
}
