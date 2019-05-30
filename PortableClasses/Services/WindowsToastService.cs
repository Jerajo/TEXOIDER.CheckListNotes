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
                        ToastScenario.Alarm : ToastScenario.Reminder,
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
                    Actions = (Toast?.Type == ToastTypes.Alarm) ? new ToastActionsCustom
                    {
                        Buttons = 
                        {
                            new ToastButton("Completo", $"action=complete&amp;id={Toast.Id}")
                            {
                                ActivationType = ToastActivationType.Background,
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            },
                            new ToastButton("Posponer", $"action=snooze&amp;id={Toast.Id}")
                            {
                                ActivationType = ToastActivationType.Background,
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            },
                            new ToastButton("Cancelar", $"action=dismiss&amp;id={Toast.Id}")
                            {
                                ActivationType = ToastActivationType.Background,
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            }
                        }
                    } :  new ToastActionsCustom
                    {
                        Buttons =
                        {
                            new ToastButton("Detalles", $"action=details&amp;id={Toast.Id}")
                            {
                                ActivationType = ToastActivationType.Foreground,
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            },
                            new ToastButton("Aceptar", $"action=acept&amp;id={Toast.Id}")
                            {
                                ActivationType = ToastActivationType.Background,
                                ImageUri = "Assets/ToastButtonIcons/Dismiss.png"
                            }
                        }
                    }
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
