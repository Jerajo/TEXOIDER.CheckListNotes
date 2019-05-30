namespace CheckListNotes.Models.Interfaces
{
    using PortableClasses.Interfaces;
    public interface IDeviceHelper
    {
        bool IsDesktop { get; }
        void CloseApp();
        void RegisterBackgroundTask(IBackgroundTask task);
        void UnregisterBackgroundTask(string taskName);
    }
}
