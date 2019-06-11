using System;
using System.Linq;
using Windows.Storage;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using Windows.UI.Notifications;
using PortableClasses.Services;
using PortableClasses.Extensions;
using System.Collections.Generic;
using PortableClasses.Implementations;
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

                var args = QueryParser.Parse(details.Argument);

                if (!AppCommandsDataTemplate.Contains(args["action"]) &&
                    args["action"] != AppCommandsDataTemplate.CompleteTask) CancelTask();

                using (var service = new WindowsToastService())
                {
                    service.RemoveToastFromHistory(args["toastId"]);
                }

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

                    var task = IndexNavigation(list.CheckListTasks, taskId);

                    if (task != null && task?.IsChecked == false) 
                    {
                        task.IsChecked = true; // complete uncompleted task
                        if (task.IsDaily == true) AddOrUpdateToast(task);

                        if (task.Id.Contains("."))
                        {
                            var parentTask = IndexNavigation(list.CheckListTasks,
                                task.Id.RemoveLastSplit('.'));
                            parentTask.IsChecked = !parentTask.SubTasks.Any(m => m.IsChecked == false);
                        }

                        await Task.Run(() => fileService.Write(list, pathToFile)).TryTo();
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

        private void AddOrUpdateToast(CheckTaskModel task)
        {
            using (var service = new WindowsToastService())
            {
                if (task.ReminderTime?.DayOfYear <= DateTime.Now.DayOfYear)
                    task.ReminderTime = DateTime.Now.AddDays(1)
                        .ChangeTime((TimeSpan)task.ReminderTime?.TimeOfDay);

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

        #region Navigation

        private CheckTaskModel IndexNavigation(List<CheckTaskModel> list, string uri, int deep = 1)
        {
            if (uri.Contains(".") && deep < uri.Split('.').Length)
            {
                var taskId = uri.GetSplitRange(".", endValue: deep);
                var model = list.FirstOrDefault(m => m.Id == taskId);
                return IndexNavigation(model.SubTasks,
                    uri, ++deep);
            }
            else return list.FirstOrDefault(m => m.Id == uri);
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
