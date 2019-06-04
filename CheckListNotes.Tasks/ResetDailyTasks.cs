using System;
using System.IO;
using Windows.Storage;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using PortableClasses.Services;
using PortableClasses.Extensions;
using System.Collections.Generic;
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
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            this.deferral = taskInstance.GetDeferral();
            this.currentTaskInstance = taskInstance;

            try
            {
                using (var fileService = new FileService())
                {
                    var localFoldeer = ApplicationData.Current.LocalFolder;
                    var initFilePath = $"{localFoldeer.Path}/init.bin";
                    var initFile = await Task.Run(() =>
                        fileService.Read<InitFile>(initFilePath)).TryTo();

                    if (initFile == null) CancelTask();
                    else if (initFile.LastResetTime.DayOfYear == DateTime.Now.DayOfYear)
                        CancelTask();
                    else
                    {
                        initFile.LastResetTime = DateTime.Now;
                        var document = JToken.FromObject(initFile);
                        await Task.Run(() => fileService.Write(document, initFilePath)).TryTo();
                    }

                    var userDataFilePath = $"{localFoldeer.Path}/Data/";
                    var pathToFiles = Directory.GetFiles(userDataFilePath, "*.bin");
                    if (pathToFiles.Length <= 0) CancelTask();

                    foreach (var filePath in pathToFiles)
                    {
                        if (cancelRequested) CancelTask();

                        var list = await Task.Run(() =>
                            fileService.Read<CheckListTasksModel>(filePath)).TryTo();

                        if (cancelRequested) CancelTask();

                        if (list == null) CancelTask();
                        else if (list.CheckListTasks.Count <= 0) continue;

                        if (cancelRequested) CancelTask();

                        var hasChanges = await ResetAllDailyTasks(list.CheckListTasks);

                        if (!hasChanges) continue;

                        if (cancelRequested) CancelTask();

                        var document = JToken.FromObject(list);
                        await Task.Run(() => fileService.Write(document, filePath)).TryTo();
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

        #region Reset Daily Tasks

        private Task<bool> ResetAllDailyTasks(List<CheckTaskModel> taskList,
            bool taskGroupIsDaily = false)
        {
            var hasChanges = false;
            foreach (var task in taskList)
            {
                if (cancelRequested) CancelTask();
                if (task.IsTaskGroup && task.SubTasks != null && task.SubTasks.Count > 0)
                {
                    var resoult = ResetAllDailyTasks(task.SubTasks, task.IsDaily).Result;
                    if (task.IsChecked && resoult) hasChanges = !(task.IsChecked = false);
                }
                else if (task.IsChecked && (taskGroupIsDaily | task.IsDaily))
                {
                    hasChanges = !(task.IsChecked = false);
                    if (task.NotifyOn != ToastTypesTime.None)
                        RegisterUnregistedDailyToast(task);
                }
            }
            return Task.FromResult(hasChanges);
        }

        #endregion

        #region Register Toast

        private void RegisterUnregistedDailyToast(CheckTaskModel task)
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
