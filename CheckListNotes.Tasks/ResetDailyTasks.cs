using System;
using CheckListNotes;
using System.Diagnostics;
using CheckListNotes.Models;
using Windows.ApplicationModel.Background;

namespace CheckListNotes.Tasks
{
    public sealed class ResetDailyTasks : IBackgroundTask
    {
        #region Atributes

        // Indicate wether the task has beem completed or not.
        private BackgroundTaskDeferral deferral;
        // Indicate what was the resaon for cancelation.
        private BackgroundTaskCancellationReason cancelReason = BackgroundTaskCancellationReason.Abort;
        // Indicate wether the task has beem cancel or not.
        private volatile bool cancelRequested = false;
        // Indicarte the runing execution time.
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
                GlobalDataService.Init();
                var allList = GlobalDataService.GetAllLists();

                foreach (var list in allList)
                {
                    if (cancelRequested) break;
                    GlobalDataService.GetCurrentList(list.Name);
                    for (var i = 0; i > list.CheckListTasks.Count; i++)
                    {
                        if (cancelRequested) break;
                        if (list.CheckListTasks[i].IsDaily)
                        {
                            list.CheckListTasks[i].IsChecked = false;
                        }

                    }
                    var model = new CheckListViewModel
                    {
                        //Id = m.Id TODO: Implement entity Framewor
                        LastId = list.LastId,
                        Name = list.Name,
                        CheckListTasks = list.CheckListTasks
                    };
                    GlobalDataService.UpdateList(model);
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
            }
            finally
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
