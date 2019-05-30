using CheckListNotes.Models;
using Windows.System.Profile;
using Windows.ApplicationModel.Core;
using CheckListNotes.Models.Interfaces;
using Windows.ApplicationModel.Background;

namespace CheckListNotes.UWP
{
    public class Dependencies : IDeviceHelper
    {
        #region Device Management

        public bool IsDesktop
        {
            get
            {
                var device = AnalyticsInfo.VersionInfo.DeviceFamily;
                return (device == "Windows.Desktop");
            }
        }

        public void CloseApp() => CoreApplication.Exit();

        #endregion

        #region Background Task Management

        public async void RegisterBackgroundTask(PortableClasses.Interfaces.IBackgroundTask task)
        {
            if (task is BackgroundTaskModel model)
            {
                using (var service = new BackgroundTaskService())
                {
                    await service.RegisterOutOfProcessBackgroundTask(
                        model.Title,
                        model.EntryPoint,
                        model.Trigger as IBackgroundTrigger,
                        //new SystemCondition(SystemConditionType.UserPresent),
                        taskCompleted: model.OnComplete
                    );
                }
            }
        }

        public void UnregisterBackgroundTask(string taskName)
        {
            var service = new BackgroundTaskService();
            service.UnregisterBackgroundTask(taskName);
        }

        #endregion
    }
}
