namespace CheckListNotes.Models
{
    using System;
    using CheckListNotes.Models.Interfaces;
    public class InitFile : IInitFile
    {
        public Type LastPageModelName { get; set; }
        public DateTime LastResetTime { get; set; }
        public string LastListName { get; set; }
        public string LastIndex { get; set; }
    }
}
