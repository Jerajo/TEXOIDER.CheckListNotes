namespace CheckListNotes.Models
{
    using System;
    using System.Collections.Generic;
    using CheckListNotes.Models.Interfaces;
    public class InitFile : IInitFile
    {
        public string LastPageName { get; set; }
        public DateTime LastResetTime { get; set; }
        public List<CheckListTasksModel> ListOfLists { get; set; }
        public CheckListTasksModel LastList { get; set; }
        public CheckTaskModel LastTask { get; set; }
    }
}
