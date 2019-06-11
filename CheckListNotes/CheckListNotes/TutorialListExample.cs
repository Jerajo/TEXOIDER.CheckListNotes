using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Diagnostics;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using PortableClasses.Services;
using Plugin.LocalNotifications;
using System.Collections.Generic;
using PortableClasses.Extensions;

namespace CheckListNotes
{
    public class TutorialListExample
    {
        #region Atributes

        private Random randomizer;

        #endregion

        public TutorialListExample() { randomizer = new Random(); }

        #region Methods

        #region Create Totorial List

        public void Create()
        {
            var IdCount = 0;
            var resources = AppResourcesLisener.Current;
            var model = new CheckListTasksModel
            {
                Name = resources["ExampleListName"],
                CheckListTasks = new List<CheckTaskModel>
                {
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListPendientTask"],
                        ExpirationDate = null, NotifyOn = ToastTypesTime.None,
                        CompletedDate = null, IsChecked = false, IsDaily = false
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListCompletedTask"],
                        ExpirationDate = null, NotifyOn = ToastTypesTime.None,
                        CompletedDate = DateTime.Now, IsChecked = true, IsDaily = false
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListLateTask"],
                        ExpirationDate = DateTime.Now.AddHours(-1),
                        NotifyOn = ToastTypesTime.None,
                        CompletedDate = null, IsChecked = false, IsDaily = false
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListCompletedLateTask"],
                        ExpirationDate = DateTime.Now.AddHours(-1),
                        NotifyOn = ToastTypesTime.None,
                        CompletedDate = DateTime.Now, IsChecked = true, IsDaily = false
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListUrgentTask"],
                        ExpirationDate = DateTime.Now.AddHours(1),
                        NotifyOn = ToastTypesTime.None,
                        CompletedDate = null, IsChecked = false, IsDaily = false
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListCompletedTaskOnTime"],
                        ExpirationDate = DateTime.Now.AddHours(2),
                        NotifyOn = ToastTypesTime.None,
                        CompletedDate = DateTime.Now, IsChecked = true, IsDaily = false
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListDailyTask"],
                        ExpirationDate = DateTime.Now.AddHours(2),
                        NotifyOn = ToastTypesTime.None,
                        CompletedDate = DateTime.Now, IsChecked = false, IsDaily = true
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount++}", IsTaskGroup = false,
                        Name = resources["ExampleListCompletedDailyTask"],
                        ExpirationDate = DateTime.Now.AddHours(2),
                        NotifyOn = ToastTypesTime.None,
                        CompletedDate = DateTime.Now, IsChecked = true, IsDaily = true
                    },
                    new CheckTaskModel
                    {
                        Id = $"{IdCount}", IsTaskGroup = true,
                        Name =resources["ExampleListTaskGroup"],
                        ExpirationDate = null, CompletedDate = null,
                        NotifyOn = ToastTypesTime.None,
                        IsChecked = false, IsDaily = false,
                        SubTasks = new List<CheckTaskModel>
                        {
                            new CheckTaskModel
                            {
                                Id = $"{IdCount}.{IdCount+1}", IsTaskGroup = false,
                                Name = resources["ExampleListSubTaskAlarmExampleMessage"],
                                ExpirationDate = DateTime.Now.AddHours(1).AddMinutes(1),
                                CompletedDate = null, IsChecked = false, IsDaily = true,
                                NotifyOn = ToastTypesTime.AHourBefore,
                                ToastId = RegisterToast(new ToastModel
                                {
                                    Title = resources["ExampleListAlarmExampleTitle"],
                                    Body = resources["ExampleListSubTaskAlarmExampleMessage"],
                                    Time = DateTime.Now.AddMinutes(1),
                                    Type = ToastTypes.Alarm,
                                }, 
                                resources["ExampleListName"], $"{IdCount}.{IdCount+1}")
                            },
                            new CheckTaskModel
                            {
                                Id = $"{IdCount}.{IdCount+2}", IsTaskGroup = false,
                                Name = resources["ExampleListSubTaskCompleted"],
                                NotifyOn = ToastTypesTime.None,
                                ExpirationDate = DateTime.Now.AddHours(1),
                                CompletedDate = DateTime.Now, IsChecked = true, IsDaily = false
                            }
                        }
                    },
                    new CheckTaskModel
                    {
                        Id = $"{++IdCount}", IsTaskGroup = false,
                        Name = resources["ExampleListTaskToastExampleMessage"],
                        ExpirationDate = DateTime.Now.AddHours(2),
                        CompletedDate = DateTime.Now, IsChecked = false, IsDaily = true,
                        NotifyOn = ToastTypesTime.AHourBefore,
                        ToastId = RegisterToast(new ToastModel
                        {
                            Title = resources["ExampleListToastExampleTitle"],
                            Body = resources["ExampleListTaskToastExampleMessage"],
                            Time = DateTime.Now.AddSeconds(10),
                            Type = ToastTypes.Notification,
                        },
                        resources["ExampleListName"], $"{IdCount}")
                    },
                },
                LastId = IdCount
            };

            var storageFolder = FileSystem.AppDataDirectory;
            Write(model, $"{storageFolder}/Data/{model.Name}.bin");
        }

        #endregion

        #region Register Toast

        private string RegisterToast(ToastModel model, string listName, string taskId)
        {
            var toastId = model.Id = $"{randomizer.Next(1000000)}";
            var toasType = (ToastTypes)Config.Current.NotificationType;

            var arguments = $"listName={listName}&";
            arguments += $"toastId={taskId}&";
            arguments += toasType == ToastTypes.Alarm ? ToastDataTemplate.AlarmToastArguments : ToastDataTemplate.NotificationToastArguments;

            model.Arguments = arguments + taskId;

            if (Device.RuntimePlatform == Device.UWP)
                (new WindowsToastService(model)).ProgramToast();
            else if (Device.RuntimePlatform == Device.Android)
                CrossLocalNotifications.Current.Show(model.Title, model.Body, 
                    int.Parse(toastId), model.Time);

            return toastId;
        }

        #endregion

        #region Write

        private Task Write(object data, string pathToFile)
        {
            return Task.Run(() => (new FileService()).Write(data, pathToFile)).TryTo();
        }

        #endregion

        #endregion

        #region Dispose

        ~TutorialListExample()
        {
            randomizer = null;
#if DEBUG
            Debug.WriteLine("Object destroyed: [ Id: {1}, Name: {0} ].", this.GetHashCode(), nameof(TutorialListExample));
#endif
        }

        #endregion
    }
}
