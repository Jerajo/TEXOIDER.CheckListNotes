using Windows.System.Threading;
using Windows.ApplicationModel.Background;

namespace CheckListNotes.Tasks
{
    public sealed class Notification : IBackgroundTask
    {
        #region Atributes

        // Indicate wether the task has beem completed or not.
        private BackgroundTaskDeferral deferral;
        // Indicate what was the resaon for cancelation.
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        // Hold the current tasks instance.
        private IBackgroundTaskInstance currentTaskInstance = null;

        #endregion

        #region Methods

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            this.deferral = taskInstance.GetDeferral();
            this.currentTaskInstance = taskInstance;

            // Code Here

            this.deferral.Complete();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            //this.cancelRequested = true;
            this.cancelReason = reason;
        }

        #endregion
    }
}
