namespace CheckListNotes.Models
{
    using PropertyChanged;
    using System.Diagnostics;
    using System.Collections.Generic;
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckListTasksModel
    {
        public int? LastId { get; set; }
        public string Name { get; set; } // Primary Key
        public List<CheckTaskModel> CheckListTasks { get; set; }

        ~CheckListTasksModel()
        {
            LastId = null;
            Name = null;
            if (CheckListTasks != null)
            {
                CheckListTasks.Clear();
                CheckListTasks = null;
            }
#if DEBUG
            //Debug.WriteLine($"Object destroyect: Name: {nameof(CheckListTasksModel)}, Id: {this.GetHashCode()}].");
#endif
        }
    }
}
