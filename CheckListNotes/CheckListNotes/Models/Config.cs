﻿using System;
using Plugin.Settings;
using PropertyChanged;
using System.Globalization;
using PortableClasses.Enums;
using Plugin.Settings.Abstractions;
using CheckListNotes.Models.Interfaces;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class Config : BaseModel, IDisposable
    {
        public Config() { }

        #region SETTERS AND GETTERS

        #region Instances

        public static Config Current { get; } = new Config();

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

        #region Theme

        public string Theme
        {
            get => AppSettings.GetValueOrDefault(nameof(Theme), "Dark");
            set => AppSettings.AddOrUpdateValue(nameof(Theme), value);
        }

        public IAppTheme AppTheme { get; set; }

        #endregion

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

        public string UrgentTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(UrgentTaskColor), ""); //TODO:
            set => AppSettings.AddOrUpdateValue(nameof(UrgentTaskColor), value);
        }

        public string LateTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(LateTaskColor), ""); //TODO:
            set => AppSettings.AddOrUpdateValue(nameof(LateTaskColor), value);
        }

        public string CompletedTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(CompletedTaskColor), ""); //TODO:
            set => AppSettings.AddOrUpdateValue(nameof(CompletedTaskColor), value);
        }

        #endregion

        #region Methods

        public void Dispose() { }

        #endregion
    }
}
