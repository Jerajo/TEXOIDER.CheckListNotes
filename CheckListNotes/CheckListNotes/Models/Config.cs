using System;
using System.Linq;
using Plugin.Settings;
using PropertyChanged;
using System.Collections.Generic;
using Plugin.Settings.Abstractions;
using CheckListNotes.Models.Interfaces;

namespace CheckListNotes.Models
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class Config : BaseModel, IDisposable
    {

        #region Atributes

        private string _theme;

        #endregion

        #region SETTERS AND GETTERS

        #region Instances

        public static Config Current { get; } = new Config();

        private ISettings AppSettings
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

        public IAppTheme AppTheme { get; set; }

        public Enums.Themes SelectedTheme
        {
            get
            {
                var theme = ThemeSelected;
                switch (theme)
                {
                    case "Light": return Enums.Themes.Light;
                    case "Dark": default: return Enums.Themes.Dark;
                }
            }
            set
            {
                switch (value)
                {
                    case Enums.Themes.Light: ThemeSelected = "Light"; break;
                    case Enums.Themes.Dark: default: ThemeSelected = "Dark"; break;
                }
            }
        }

        public List<string> Themes { get; set; }
        public string Theme
        {
            get => _theme ?? (_theme = Themes.ElementAtOrDefault((int)SelectedTheme));
            set
            {
                _theme = value;
                SelectedTheme = (Enums.Themes)Themes.IndexOf(_theme);
            }
        }

        public string ThemeSelected
        {
            get => AppSettings.GetValueOrDefault(nameof(Theme), "Dark");
            set => AppSettings.AddOrUpdateValue(nameof(Theme), value);
        }

        #endregion

        public List<string> Languages { get; set; }
        public string Language
        {
            get => AppSettings.GetValueOrDefault(nameof(Language), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(Language), value);
            }
        }

        public List<string> FontSizes { get; set; }
        public string FontSize
        {
            get => AppSettings.GetValueOrDefault(nameof(FontSize), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(FontSize), value);
            }
        }

        public List<string> NotificationTypes { get; set; }
        public string NotificationType
        {
            get => AppSettings.GetValueOrDefault(nameof(NotificationType), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(NotificationType), value);
            }
        }

        public List<string> NotificationSounds { get; set; }
        public string NotificationSound
        {
            get => AppSettings.GetValueOrDefault(nameof(NotificationSound), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(NotificationSound), value);
            }
        }

        public List<string> TouchSounds { get; set; }
        public string TouchSound
        {
            get => AppSettings.GetValueOrDefault(nameof(TouchSound), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(TouchSound), value);
            }
        }

        public List<string> UrgentTaskColors { get; set; }
        public string UrgentTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(UrgentTaskColor), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(UrgentTaskColor), value);
            }
        }

        public List<string> LateTaskColors { get; set; }
        public string LateTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(LateTaskColor), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(LateTaskColor), value);
            }
        }

        public List<string> CompletedTaskColors { get; set; }
        public string CompletedTaskColor
        {
            get => AppSettings.GetValueOrDefault(nameof(CompletedTaskColor), string.Empty);
            set
            {
                if (value != null) AppSettings.AddOrUpdateValue(nameof(CompletedTaskColor), value);
            }
        }

        #endregion

        #region Methods

        public Config()
        {
            Themes = new List<string> { "Oscuro", "Claro" };
            Languages = new List<string> { "Español", "English", "French" };
            FontSizes = new List<string> { "Grande", "Mediano", "Pequeño" };
            NotificationTypes = new List<string> { "Notificaón", "Alarma" };
            NotificationSounds = new List<string> { "Beeb", "Ring" };
            TouchSounds = new List<string> { "Tab", "Clic" };
            UrgentTaskColors = new List<string> { "Rojo", "Naranja", "Verde" };
            LateTaskColors = new List<string> { "Naranja", "Verde", "Rojo" };
            CompletedTaskColors = new List<string> { "Verde", "Rojo", "Naranja" };
        }

        public void Dispose()
        {
            if (Themes != null)
            {
                Themes.Clear();
                Themes = null;
            }
            if (Languages != null)
            {
                Languages.Clear();
                Languages = null;
            }
            if (FontSizes != null)
            {
                FontSizes.Clear();
                FontSizes = null;
            }
            if (NotificationTypes != null)
            {
                NotificationTypes.Clear();
                NotificationTypes = null;
            }
            if (NotificationSounds != null)
            {
                NotificationSounds.Clear();
                NotificationSounds = null;
            }
            if (TouchSounds != null)
            {
                TouchSounds.Clear();
                TouchSounds = null;
            }
            if (UrgentTaskColors != null)
            {
                UrgentTaskColors.Clear();
                UrgentTaskColors = null;
            }
            if (LateTaskColors != null)
            {
                LateTaskColors.Clear();
                LateTaskColors = null;
            }
            if (CompletedTaskColors != null)
            {
                CompletedTaskColors.Clear();
                CompletedTaskColors = null;
            }
        }

        #endregion
    }
}
