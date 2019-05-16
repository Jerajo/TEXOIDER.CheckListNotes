using System;
using System.IO;
using System.Linq;
using Windows.Storage;
using Newtonsoft.Json.Linq;
using CheckListNotes.Models;
using Windows.System.Profile;
using PortableClasses.Services;
using Windows.UI.Notifications;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using CheckListNotes.Models.Interfaces;
using Microsoft.Toolkit.Uwp.Notifications;
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

        #region File Management

        public string GetAppFolder() => Windows.Application­Model.Package.Current.Installed­Location.Path;

        public StorageFolder GetLocalFolder() => ApplicationData.Current.LocalFolder;

        public string GetStorageFolder() => GetLocalFolder().Path;

        public List<string> GetAllLists()
        {
            var files = Directory.GetFiles($"{GetStorageFolder()}/Data/", "*.bin");
            var fileNames = new List<string>();
            foreach (var file in files) fileNames.Add(Path.GetFileNameWithoutExtension(file));
            return fileNames;
        }

        public List<CheckListTasksModel> GetAllLists(List<string> listNames)
        {
            var listOfCheckList = new List<CheckListTasksModel>();
            foreach (var name in listNames)
            {
                listOfCheckList.Add(Read($"/Data/{name}.bin"));
            }
            return listOfCheckList;
        }

        public CheckListTasksModel Read(string path = null)
        {
            using (var fileService = new FileService())
            {
                if (string.IsNullOrEmpty(path))
                {
                    var json = ((JObject)fileService.Read(GetStorageFolder() + "/init.bin"));
                    return ((JObject)fileService.Read(GetStorageFolder() + $"/Data/{json["initFileName"]}.bin")).ToObject<CheckListTasksModel>();
                }
                return ((JObject)fileService.Read(GetStorageFolder() + path)).ToObject<CheckListTasksModel>();
            }
        }

        public void Write(object model, string path = null)
        {
            using (var fileService = new FileService())
            {
                var document = JToken.FromObject(model);
                if (string.IsNullOrEmpty(path))
                {
                    fileService.Write(document,
                        $"{GetStorageFolder()}/Data/{((CheckListTasksModel)model).Name}.bin");
                }
                else fileService.Write(document, GetStorageFolder() + path);
            }
        }

        public void Rename(string oldName, string newName)
        {
            var oldPath = GetStorageFolder() + $"/Data/{oldName}.bin";
            var newPath = GetStorageFolder() + $"/Data/{newName}.bin";
            File.Move(oldPath, newPath);
        }

        public void Delete(string name)
        {
            var fullPath = GetStorageFolder() + $"/Data/{name}.bin";
            File.Delete(fullPath);
        }

        #endregion

        #region Background Task Management

        public string RegisterNotification(PortableClasses.Interfaces.IToast toast)
        {
            if (toast.Time < DateTime.Now.AddSeconds(1)) throw new Exception("Tiempo invalido");

            var content = new ToastContent
            {
                Scenario = ToastScenario.Reminder,
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = toast.Title,
                                HintMaxLines = 1
                            },
                            new AdaptiveText
                            {
                                Text = toast.Body,
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom
                {
                    Buttons =
                    {
                        new ToastButton("Detalles", $"action=details&amp;id={toast.Id}")
                        {
                            ActivationType = ToastActivationType.Foreground,
                            ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                        },
                        new ToastButton("Aceptar", $"action=acept&amp;id={toast.Id}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                        }
                    }
                }
            };

            // Create the scheduled notification
            var scheduledNotif = new ScheduledToastNotification(
            content.GetXml(), // Content of the toast
            toast.Time // Time we want the toast to appear at
            );
            scheduledNotif.Id = toast.Id;
            // And add it to the schedule
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotif);
            return scheduledNotif.Id;
        }

        public string RegisterAlarm(PortableClasses.Interfaces.IToast toast)
        {
            if (toast.Time < DateTime.Now.AddSeconds(1)) throw new Exception("Tiempo invalido");

            var content = new ToastContent
            {
                Scenario = ToastScenario.Alarm,
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = toast.Title,
                                HintMaxLines = 1
                            },
                            new AdaptiveText
                            {
                                Text = toast.Body,
                            }
                        }
                    }
                },
                Actions = new ToastActionsCustom
                {
                    Buttons =
                    {
                        new ToastButton("Completo", $"action=complete&amp;id={toast.Id}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                        },
                        new ToastButton("Posponer", $"action=snooze&amp;id={toast.Id}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                        },
                        new ToastButton("Cancelar", $"action=dismiss&amp;id={toast.Id}")
                        {
                            ActivationType = ToastActivationType.Background,
                            ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                        }
                    }
                }
            };
            var scheduledNotif = new ScheduledToastNotification(content.GetXml(), toast.Time, TimeSpan.FromMinutes(5), 3);
            scheduledNotif.Id = toast.Id;
            ToastNotificationManager.CreateToastNotifier().AddToSchedule(scheduledNotif);
            return scheduledNotif.Id;
        }

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
        
        public void UnregisterToast(string alarmId)
        {
            var notifier = ToastNotificationManager.CreateToastNotifier();
            // Get all scheduled notifications
            var scheduled = notifier.GetScheduledToastNotifications();
            // Find the one we want to remove
            var toRemove = scheduled.FirstOrDefault(i => i.Id.Equals(alarmId));
            // And remove it
            if (toRemove != null) notifier.RemoveFromSchedule(toRemove);
        }

        public void UnregisterBackgroundTask(string taskName)
        {
            var service = new BackgroundTaskService();
            service.UnregisterBackgroundTask(taskName);
        }

        #endregion
    }
}
