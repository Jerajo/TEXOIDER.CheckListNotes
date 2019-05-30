namespace CheckListNotes.Models.Interfaces
{
    using System;
    using System.Collections.Generic;
    public interface IInitFile
    {
        string LastPageName { get; set; }
        DateTime LastResetTime { get; set; }
        List<CheckListTasksModel> ListOfLists { get; set; }
        CheckListTasksModel LastList { get; set; }
        CheckTaskModel LastTask { get; set; }
    }
}
