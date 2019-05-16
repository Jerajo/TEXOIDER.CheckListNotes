namespace CheckListNotes.Models.Interfaces
{
    using CheckListNotes.Models;
    using PortableClasses.Interfaces;
    using System.Collections.Generic;
    public interface IDeviceHelper
    {
        bool IsDesktop { get; }
        void CloseApp();
        List<string> GetAllLists();
        List<CheckListTasksModel> GetAllLists(List<string> listOfCheckListNames);
        CheckListTasksModel Read(string path = null);
        void Write(object model, string path = null);
        void Rename(string oldName, string newName);
        void Delete(string name);
        string RegisterNotification(IToast toast);
        string RegisterAlarm(IToast toast);
        void RegisterBackgroundTask(IBackgroundTask task);
        void UnregisterToast(string toasId);
        void UnregisterBackgroundTask(string taskName);
    }
}
