using System;
using System.IO;
using Newtonsoft.Json;
using PropertyChanged;
using Xamarin.Essentials;
using System.Windows.Input;
using CheckListNotes.Models;
using System.ComponentModel;
using System.Threading.Tasks;
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

        public Config AppSettings
        {
            get => Config.Current;
        }

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
            await CoreMethods.PopPageModel(InitData, animate: true);
        }

        #endregion

        #region Overrided Methods

        public override void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);

            InitData = initData;

            AppSettings.PropertyChanged += SettingsPropertyChanged;

            PageTitle = "Opciones";

            base.Init(initData);
            IsLooked = !(HasLoaded = true);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            IsDisposing = true;
            if (AppSettings != null)
                AppSettings.PropertyChanged -= SettingsPropertyChanged;
            base.ViewIsAppearing(sender, e);
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        public Task RefreshUI() => Task.Run(() => Init(null));

        private async void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsLooked || IsDisposing) return;

            if (sender is Config config)
            {
                if (e.PropertyName == nameof(AppSettings.Theme))
                {
                    using (var stream = await FileSystem.OpenAppPackageFileAsync($"{config.ThemeSelected}.json"))
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            var fileContents = await reader.ReadToEndAsync();
                            base.Theme = JsonConvert.DeserializeObject<AppTheme>(fileContents);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
