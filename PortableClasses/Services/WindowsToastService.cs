using System;
using System.Linq;
using Windows.Data.Xml.Dom;
using PortableClasses.Enums;
using Windows.UI.Notifications;
using PortableClasses.Interfaces;
using Microsoft.Toolkit.Uwp.Notifications;

namespace PortableClasses.Services
{
    public class WindowsToastService : IDisposable, IToastService
    {
        #region Atributtes

        private ToastContent content;

        #endregion

        public WindowsToastService() { }
        public WindowsToastService(IToast toast)
        {
            Toast = toast;
        }
        public WindowsToastService(IToast toast, ToastContent content)
        {
            Toast = toast;
            Content = content;
        }

        #region SETTERS AND GETTERS

        public bool IsDisposing { get; private set; }

        public IToast Toast { get; private set; }

        public ScheduledToastNotification ScheduledToast { get; private set; }

        public ToastContent Content
        {
            private set => content = value;
            get
            {
                return content ?? (content = new ToastContent
                {
                    Scenario = (Toast?.Type == ToastTypes.Alarm) ? 
                        ToastScenario.Alarm : ToastScenario.Default,
                    Visual = new ToastVisual
                    {
                        BindingGeneric = new ToastBindingGeneric
                        {
                            Children =
                            {
                                new AdaptiveText
                                {
                                    Text = Toast?.Title,
                                    HintMaxLines = 1
                                },
                                new AdaptiveText
                                {
                                    Text = Toast?.Body,
                                }
                            }
                        }
                    },
                    Audio = new ToastAudio
                    {
                        Src = new Uri((Toast?.Type == ToastTypes.Alarm) ? "ms-winsoundevent:Notification.Looping.Alarm" : "ms-winsoundevent:Notification.Default"),
                        Loop = (Toast?.Type == ToastTypes.Alarm) ? true : false
                    },
                    Actions = (Toast?.Type == ToastTypes.Alarm) ? new ToastActionsCustom
                    {
                        Inputs =
                        {
                            new ToastSelectionBox("snoozeTime")
                            {
                                DefaultSelectionBoxItemId = "15",
                                Items =
                                {
                                    new ToastSelectionBoxItem("5", "5 minutos"),
                                    new ToastSelectionBoxItem("15", "15 minutos"),
                                    new ToastSelectionBoxItem("30", "30 minutos"),
                                    new ToastSelectionBoxItem("60", "1 hora")
                                }
                            }
                        },
                        Buttons = 
                        {
                            new ToastButton("Completar", Toast.Arguments)
                            {
                                ActivationType = ToastActivationType.Background,
                                HintActionId = "CompleteTask",
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            },
                            new ToastButtonSnooze()
                            {
                                SelectionBoxId = "snoozeTime",
                                HintActionId = "SnoozeAlarm",
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            },
                            new ToastButtonDismiss()
                            {
                                HintActionId = "DismissAlarm",
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            }
                        }
                    } :  new ToastActionsCustom
                    {
                        Buttons =
                        {
                            new ToastButton("Ver tarea", Toast.Arguments)
                            {
                                ActivationType = ToastActivationType.Foreground,
                                HintActionId = "ShowTaskDetails",
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            },
                            new ToastButtonDismiss()
                            {
                                HintActionId = "DismissReminder",
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            }
                        }
                    },
                    Launch = Toast.Arguments,
                });
            }
        }

        #endregion

        #region Methods

        #region Toast Exist

        public bool ToastExist(IToast toast)
        {
            Toast = toast ?? throw new ArgumentNullException(nameof(Toast));
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            var toRemove = scheduled.FirstOrDefault(i => i.Id.Equals(toast.Id));
            return (toRemove != null);
        }

        public bool ToastExist(string toastId)
        {
            if (string.IsNullOrEmpty(toastId))
                throw new ArgumentNullException(nameof(toastId));
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            var toRemove = scheduled.FirstOrDefault(i => i.Id.Equals(toastId));
            return (toRemove != null);
        }

        #endregion

        #region Program Toast

        public void ProgramToast(IToast toast, ToastContent content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ProgramToast(toast);
        }

        public void ProgramToast(IToast toast)
        {
            Toast = toast ?? throw new ArgumentNullException(nameof(toast));
            ProgramToast();
        }

        public void ProgramToast()
        {
            if (Toast == null) throw new ArgumentNullException(nameof(Toast));
            if (Content == null) throw new ArgumentNullException(nameof(Content));
            if (Toast.Time < DateTime.Now.AddSeconds(1)) throw new Exception("Tiempo invalido");

            var contentXML = new XmlDocument();
            contentXML.LoadXml(Content.GetContent());

            ScheduledToast = new ScheduledToastNotification(contentXML, Toast.Time);
            ScheduledToast.Id = Toast.Id;

            ToastNotificationManager.CreateToastNotifier().AddToSchedule(ScheduledToast);
        }

        #endregion

        #region Show Toast

        public void ShowToast(IToast toast)
        {
            Toast = toast ?? throw new ArgumentNullException(nameof(toast));
            ShowToast();
        }

        public void ShowToast()
        {
            if (Toast == null) throw new ArgumentNullException(nameof(Toast));
            if (Content == null) throw new ArgumentNullException(nameof(Content));

            var contentXML = new XmlDocument();
            contentXML.LoadXml(Content.GetContent());

            var toast = new ToastNotification(contentXML);
            toast.Tag = Toast.Id;

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        #endregion

        #region Cancel programed toast

        public void CancelProgramedToast()
        {
            if (ScheduledToast == null) throw new ArgumentNullException(nameof(ScheduledToast));
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            var toRemove = scheduled.FirstOrDefault(i => i.Id.Equals(ScheduledToast.Id));
            if (toRemove != null) notifier.RemoveFromSchedule(toRemove);
        }

        public void CancelProgramedToast(string toastId)
        {
            if (string.IsNullOrEmpty(toastId))
                throw new ArgumentNullException(nameof(toastId));
            var notifier = ToastNotificationManager.CreateToastNotifier();
            var scheduled = notifier.GetScheduledToastNotifications();
            var toRemove = scheduled.FirstOrDefault(i => i.Id.Equals(toastId));
            if (toRemove != null) notifier.RemoveFromSchedule(toRemove);
        }

        #endregion

        #region Remove from action center

        public void RemoveToastFromHistory(string toastId)
        {
            if (string.IsNullOrEmpty(toastId))
                throw new ArgumentNullException(nameof(toastId));
            if (ToastExist(toastId))
                ToastNotificationManager.History.Remove(toastId);
        }

        #endregion

        #endregion

        #region Auxiliary Methods

        public void Dispose(bool isDisposing)
        {
            if (isDisposing) Dispose();
        }

        public void Dispose()
        {
            IsDisposing = true;
            Toast = null;
            Content = null;
            ScheduledToast = null;
        }

        #endregion
    }
}
