using System;
using Windows.Storage;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using PortableClasses.Services;
using PortableClasses.Extensions;
using Microsoft.QueryStringDotNET;
using Windows.ApplicationModel.Background;

namespace CheckListNotes.Tasks
{
    public sealed class CompleteTask : IBackgroundTask
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
        // Use to generate randoms Toast Id.
        private Random randomizer = new Random();

        #endregion

        #region SETTERS AND GETTERS

        public bool CancelRequested
        {
            get => cancelRequested;
            set
            {
                cancelRequested = value;
            }
        }

        public BackgroundTaskCancellationReason CancelReason
        {
            get => cancelReason;
        }

        #endregion

        #region Main Method

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            //TODO: Runs background task from toast.
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            this.deferral = taskInstance.GetDeferral();
            this.currentTaskInstance = taskInstance;

            try
            {
                var details = taskInstance.TriggerDetails as 
                    ToastNotificationActionTriggerDetail;

                if (details == null) CancelTask();

                var args = QueryString.Parse(details.Argument);

                if (!AppCommandsDataTemplate.Contains(args["action"]) &&
                    args["action"] != AppCommandsDataTemplate.CompleteTask) CancelTask();


                using (var fileService = new FileService())
                {
                    var listName = args["listName"];
                    var localFoldeer = ApplicationData.Current.LocalFolder;
                    var pathToFile = $"{localFoldeer.Path}/Data/{listName}.bin";
                    var taskId = args["taskId"];

                    if (cancelRequested) CancelTask();

                    var list = await Task.Run(() => 
                        fileService.Read<CheckListTasksModel>(pathToFile)).TryTo();

                    if (list == null) CancelTask();

                    var task = list.CheckListTasks.Find(m => m.Id == taskId);

                    if (!task.IsChecked) 
                    {
                        task.IsChecked = true; // complete uncompleted task
                        AddOrUpdateOrRemoveToast(task);

                        var document = JToken.FromObject(list);
                        await Task.Run(() => fileService.Write(document, pathToFile)).TryTo();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                cancelReason = BackgroundTaskCancellationReason.Abort;
            }
            finally
            {
                this.deferral.Complete();
            }
        }

        #endregion

        #region Private Methods

        #region Register Toast

        private void AddOrUpdateOrRemoveToast(CheckTaskModel task)
        {
            using (var service = new WindowsToastService())
            {
                if (service.ToastExist(task.ToastId))
                    service.CancelProgramedToast(task.ToastId);

                if (task.ReminderTime?.DayOfYear > DateTime.Now.DayOfYear)
                {
                    var diferens = (uint)DateTime.Now.DayOfYear - task.ReminderTime?.DayOfYear;
                    task.ReminderTime = task.ReminderTime?.AddDays(diferens.Value);
                }

                if (task.ReminderTime > DateTime.Now)
                {
                    var toast = new ToastModel
                    {
                        Id = task.ToastId = $"{task.Id}-{randomizer.Next(1000000)}",
                        Title = "Recordatorio de tarea pendiente",
                        Body = task.Name,
                        Time = task.ReminderTime.Value,
                        Type = (ToastTypes)Config.Current.NotificationType
                    };
                    service.ProgramToast(toast);
                }
            }
        }

        #endregion

        #region Cancel Task

        private void CancelTask()
        {
            this.cancelRequested = true;
            throw new TaskCanceledException();
        }

        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.cancelRequested = true;
            this.cancelReason = reason;
        }

        #endregion

        #endregion
    }
}
