using System;
using Windows.System.Threading;
using Windows.ApplicationModel.Background;

namespace CheckListNotes.Tasks
{
    public sealed class BaseBakgroundTask : IBackgroundTask
    {
        #region Atributes

        // Indicate wether the task has beem completed or not.
        private BackgroundTaskDeferral deferral;
        // Indicate what was the resaon for cancelation.
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        // Indicate wether the task has beem cancel or not.
        private volatile bool cancelRequested = false;
        // Indicarte the runing execution time.
        private ThreadPoolTimer periodicTimer = null;
        // Indicate the task current progress from 0 to 100 percent.
        private uint progress = 0;
        // Hold the current tasks instance.
        private IBackgroundTaskInstance currentTaskInstance = null;

        #endregion

        #region Methods

        public void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            this.deferral = taskInstance.GetDeferral();
            this.currentTaskInstance = taskInstance;

            try
            {
                periodicTimer = ThreadPoolTimer.CreatePeriodicTimer(OnTimer, TimeSpan.FromSeconds(1));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                this.deferral.Complete();
            }

        }

        private void OnTimer(ThreadPoolTimer timer)
        {
            if (this.progress > 100 && !cancelRequested)
            {
                this.progress += 10;
                //TODO: Preiodict task.
            }
            else
            {
                this.deferral.Complete();
            }
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.cancelRequested = true;
            this.cancelReason = reason;
        }

        #endregion
    }
}
