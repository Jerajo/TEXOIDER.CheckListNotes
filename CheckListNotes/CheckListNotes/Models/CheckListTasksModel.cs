using PropertyChanged;
using System.Collections.Generic;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class CheckListTasksModel
    {
        public int LastId { get; set; }
        public string Name { get; set; }
        public List<CheckTaskModel> CheckListTasks { get; set; }
    }
}
