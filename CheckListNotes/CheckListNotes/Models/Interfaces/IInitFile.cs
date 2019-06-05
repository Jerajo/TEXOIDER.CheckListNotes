namespace CheckListNotes.Models.Interfaces
{
    using System;
    public interface IInitFile
    {
        Type LastPageModelName { get; set; }
        DateTime LastResetTime { get; set; }
        string LastListName { get; set; }
        string LastIndex { get; set; }
    }
}
