using System;
using Plugin.Settings;
using PropertyChanged;
using System.Diagnostics;
using System.Globalization;
using PortableClasses.Enums;
using Plugin.Settings.Abstractions;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class Config : BaseModel, IDisposable
    {
        #region Atributes

        private static Config current;
        private bool? isDisposing;

        #endregion

        public Config() { isDisposing = false; }

        #region SETTERS AND GETTERS

        #region Instances

        public static Config Current { get => current ?? (current = new Config()); }

        public ISettings AppSettings
        {
            get
            {
                if (CrossSettings.IsSupported)
                    return CrossSettings.Current;
                else return ConfigHandler.Config;
            }
        }

        #endregion

        public string Theme
        {
            get => AppSettings.GetValueOrDefault(nameof(Theme), "Dark");
            set => AppSettings.AddOrUpdateValue(nameof(Theme), value);
        }

        public string Language
        {
            get => AppSettings.GetValueOrDefault(nameof(Language), (CultureInfo.CurrentCulture.TwoLetterISOLanguageName));
            set => AppSettings.AddOrUpdateValue(nameof(Language), value);
        }

        public int NotificationType
        {
            get => AppSettings.GetValueOrDefault(nameof(NotificationType), (int)ToastTypes.Notification);
            set => AppSettings.AddOrUpdateValue(nameof(NotificationType), value);
        }

        public string NotificationSound
        {
            get => AppSettings.GetValueOrDefault(nameof(NotificationSound), "Bib");
            set => AppSettings.AddOrUpdateValue(nameof(NotificationSound), value);
        }

        public string TouchSound
        {
            get => AppSettings.GetValueOrDefault(nameof(TouchSound), "Clic");
            set => AppSettings.AddOrUpdateValue(nameof(TouchSound), value);
        }

        public string LateTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(LateTaskColor), "#FFCD0000");
            set => AppSettings.AddOrUpdateValue(nameof(LateTaskColor), value);
        }

        public string UrgentTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(UrgentTaskColor), "#FFCD6700");
            set => AppSettings.AddOrUpdateValue(nameof(UrgentTaskColor), value);
        }

        public string CompletedTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(CompletedTaskColor), "#FF00CD00");
            set => AppSettings.AddOrUpdateValue(nameof(CompletedTaskColor), value);
        }

        #endregion

        #region Disposing

        ~Config()
        {
            if (isDisposing == false) Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            isDisposing = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Id: {0}, Name: {1} ].",
                this.GetHashCode(), nameof(LanguageService));
#endif
            current = null;
        }

        #endregion
    }
}
