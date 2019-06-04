using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PropertyChanged;
using Xamarin.Essentials;
using System.Windows.Input;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    class OptionsPageModel : BasePageModel, IPageModel
    {
        public OptionsPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        #region Theme

        private Models.Enums.Themes SelectedTheme
        {
            get
            {
                switch (ConfigTheme)
                {
                    case "Light": return Models.Enums.Themes.Light;
                    case "Dark": default: return Models.Enums.Themes.Dark;
                }
            }
            set
            {
                switch (value)
                {
                    case Models.Enums.Themes.Light: ConfigTheme = "Light"; break;
                    case Models.Enums.Themes.Dark: default: ConfigTheme = "Dark"; break;
                }
            }
        }
        private string ConfigTheme
        {
            get => Config.Current.Theme;
            set
            {
                if (!string.IsNullOrEmpty(value)) ThemeChanged(Config.Current.Theme = value);
            }
        }

        public List<string> Themes { get; set; }
        public string ThemeSelected
        {
            get => Themes?.ElementAtOrDefault((int)SelectedTheme);
            set
            {
                if (!string.IsNullOrEmpty(value))
                    SelectedTheme = (Models.Enums.Themes)Themes.IndexOf(value);
            }
        }

        #endregion

        #region Language

        private Models.Enums.Languages SelectedLanguage
        {
            get
            {
                switch (ConfigLanguage)
                {
                    case "En": return Models.Enums.Languages.English;
                    case "Fr": return Models.Enums.Languages.English;
                    case "Es": default: return Models.Enums.Languages.Español;
                }
            }
            set
            {
                switch (value)
                {
                    case Models.Enums.Languages.English: ConfigLanguage = "En"; break;
                    case Models.Enums.Languages.French: ConfigLanguage = "Fr"; break;
                    case Models.Enums.Languages.Español: default: ConfigLanguage = "Es"; break;
                }
            }
        }
        private string ConfigLanguage
        {
            get => Config.Current.Language;
            set
            {
                if (!string.IsNullOrEmpty(value)) Config.Current.Language = value;
            }
        }

        public List<string> Languages { get; set; }
        public string Language
        {
            get => Languages?.ElementAtOrDefault((int)SelectedLanguage);
            set
            {
                if (!string.IsNullOrEmpty(value))
                    SelectedLanguage = (Models.Enums.Languages)Languages.IndexOf(value);
            }
        }

        #endregion

        #region NotificationType

        private ToastTypes SelectedNotificationType
        {
            get
            {
                switch (ConfigNotificationType)
                {
                    case 0: return ToastTypes.Alarm;
                    case 1: default: return ToastTypes.Notification;
                }
            }
            set
            {
                switch (value)
                {
                    case ToastTypes.Alarm: ConfigNotificationType = 0; break;
                    case ToastTypes.Notification:
                    default: ConfigNotificationType = 1; break;
                }
            }
        }
        private int? ConfigNotificationType
        {
            get => Config.Current.NotificationType;
            set
            {
                if (value != null) Config.Current.NotificationType = value.Value;
            }
        }

        public List<string> NotificationTypes { get; set; }
        public string NotificationType
        {
            get => NotificationTypes?.ElementAtOrDefault((int)SelectedNotificationType);
            set
            {
                if (!string.IsNullOrEmpty(value))
                    SelectedNotificationType = (ToastTypes)NotificationTypes.IndexOf(value);
            }
        }

        #endregion

        #region NotificationSound

        private Models.Enums.NotificationSounds SelectedNotificationSound
        {
            get
            {
                switch (ConfigNotificationSound)
                {
                    case "None": return Models.Enums.NotificationSounds.None;
                    case "Bib": return Models.Enums.NotificationSounds.Bib;
                    case "Ring": return Models.Enums.NotificationSounds.Ring;
                    case "Custom": return Models.Enums.NotificationSounds.Custom;
                    case "Thone": default: return Models.Enums.NotificationSounds.Thone;
                }
            }
            set
            {
                switch (value)
                {
                    case Models.Enums.NotificationSounds.None:
                        ConfigNotificationSound = "None"; break;
                    case Models.Enums.NotificationSounds.Bib:
                        ConfigNotificationSound = "Bib"; break;
                    case Models.Enums.NotificationSounds.Ring:
                        ConfigNotificationSound = "Ring"; break;
                    case Models.Enums.NotificationSounds.Custom:
                        ConfigNotificationSound = "Custom"; break;
                    case Models.Enums.NotificationSounds.Thone:
                    default: ConfigNotificationSound = "Thone"; break;
                }
            }
        }
        private string ConfigNotificationSound
        {
            get => Config.Current.NotificationSound;
            set
            {
                if (!string.IsNullOrEmpty(value)) Config.Current.NotificationSound = value;
            }
        }

        public List<string> NotificationSounds { get; set; }
        public string NotificationSound
        {
            get => NotificationSounds?.ElementAtOrDefault((int)SelectedNotificationSound);
            set
            {
                if (!string.IsNullOrEmpty(value))
                    SelectedNotificationSound = (Models.Enums.NotificationSounds)NotificationSounds.IndexOf(value);
            }
        }

        #endregion

        #region TouchSounds

        private Models.Enums.TouchSounds SelectedTouchSound
        {
            get
            {
                switch (ConfigTouchSound)
                {
                    case "None": return Models.Enums.TouchSounds.None;
                    case "Clic": return Models.Enums.TouchSounds.Clic;
                    case "Custom": return Models.Enums.TouchSounds.Custom;
                    case "Touch": default: return Models.Enums.TouchSounds.Touch;
                }
            }
            set
            {
                switch (value)
                {
                    case Models.Enums.TouchSounds.None:
                        ConfigNotificationSound = "None"; break;
                    case Models.Enums.TouchSounds.Clic:
                        ConfigNotificationSound = "Clic"; break;
                    case Models.Enums.TouchSounds.Custom:
                        ConfigNotificationSound = "Custom"; break;
                    case Models.Enums.TouchSounds.Touch:
                    default: ConfigNotificationSound = "Touch"; break;
                }
            }
        }
        private string ConfigTouchSound
        {
            get => Config.Current.TouchSound;
            set
            {
                if (!string.IsNullOrEmpty(value)) Config.Current.TouchSound = value;
            }
        }

        public List<string> TouchSounds { get; set; }
        public string TouchSound
        {
            get => TouchSounds?.ElementAtOrDefault((int)SelectedNotificationSound);
            set
            {
                if (!string.IsNullOrEmpty(value))
                    SelectedNotificationSound = (Models.Enums.NotificationSounds)TouchSounds.IndexOf(value);
            }
        }

        #endregion

        #region UrgentTaskColors

        public string UrgentTaskColor
        {
            get => Config.Current.UrgentTaskColor;
            set
            {
                if (!string.IsNullOrEmpty(value)) Config.Current.UrgentTaskColor = value;
            }
        }

        #endregion

        #region LateTaskColors

        public string LateTaskColor
        {
            get => Config.Current.LateTaskColor;
            set
            {
                if (!string.IsNullOrEmpty(value)) Config.Current.LateTaskColor = value;
            }
        }

        #endregion

        #region UrgentTaskColors

        public string CompletedTaskColor
        {
            get => Config.Current.CompletedTaskColor;
            set
            {
                if (!string.IsNullOrEmpty(value)) Config.Current.CompletedTaskColor = value;
            }
        }

        #endregion

        #endregion

        #region Commands

        public ICommand GoBack
        {
            get => new DelegateCommand(new Action(GoBackCommand));
        }

        #endregion

        #endregion

        #region Commands

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked || IsDisposing) return;
            IsLooked = true;
            await PopPageModel(InitData);
        }

        #endregion

        #region Overrided Methods

        public override void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);

            InitData = initData;

            //TODO: Create with language file
            Themes = new List<string> { "Oscuro", "Claro" };
            Languages = new List<string> { "Español", "English", "French" };
            NotificationTypes = new List<string> { "Alarma", "Notificación" };
            NotificationSounds = new List<string> { "Desactivado", "Ring", "Thone", "Bib", "Selecciona uno" };
            TouchSounds = new List<string> { "Desactivado", "Clic", "Touch", "Selecciona uno" };

            PageTitle = "Opciones";

            base.Init(initData);
            IsLooked = !(HasLoaded = true);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            IsDisposing = true;
            if (Themes != null)
            {
                Themes.Clear();
                Themes = null;
            }
            if (Themes != null)
            {
                Languages.Clear();
                Languages = null;
            }
            if (Themes != null)
            {
                NotificationTypes.Clear();
                NotificationTypes = null;
            }
            if (Themes != null)
            {
                NotificationSounds.Clear();
                NotificationSounds = null;
            }
            if (Themes != null)
            {
                TouchSounds.Clear();
                TouchSounds = null;
            }
            base.ViewIsAppearing(sender, e);
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        private async void ThemeChanged(string themeName)
        {
            if (IsLooked || IsDisposing) return;

            using (var stream = await FileSystem.OpenAppPackageFileAsync($"{themeName}.json"))
            {
                using (var reader = new StreamReader(stream))
                {
                    var fileContents = await reader.ReadToEndAsync();
                    Config.Current.AppTheme = JsonConvert.DeserializeObject<AppTheme>(fileContents);
                }
            }
        }

        #endregion
    }
}
