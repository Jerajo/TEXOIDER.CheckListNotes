using FreshMvvm;
using Xamarin.Forms;
using PropertyChanged;
using PortableClasses.Enums;
using CheckListNotes.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using PortableClasses.Interfaces;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class BasePageModel : FreshBasePageModel
    {
        public BasePageModel() : base()
        {
            Config.Current.PropertyChanged += ConfigChanged;
        }

        #region SETTERS AND GETTERS

        #region Theme

        public string AppFontColor { get => Config.Current.AppTheme?.AppFontColor; }
        public string AppBackgroundColor
        { get => Config.Current.AppTheme?.AppBackgroundColor; }
        public string HeaderAndHeaderAndFooterBackgroundColor
        { get => Config.Current.AppTheme?.HeaderAndHeaderAndFooterBackgroundColor; }
        public string CellBackgroundColor
        { get => Config.Current.AppTheme?.CellBackgroundColor; }
        public string CellBorderColor { get => Config.Current.AppTheme?.CellBorderColor; }
        public string EditorBorderColor
        { get => Config.Current.AppTheme?.EditorBorderColor; }
        public string EditorBackgroundColor { get => Config.Current.AppTheme?.EditorBackgroundColor; }
        public string ButtonBackgroundColor
        { get => Config.Current.AppTheme?.ButtonBackgroundColor; }
        public string DialogBackgroundColor
        { get => Config.Current.AppTheme?.DialogBackgroundColor; }
        public string AceptButtonFontColor
        { get => Config.Current.AppTheme?.AceptButtonFontColor; }
        public string CancelButtonFontColor
        { get => Config.Current.AppTheme?.CancelButtonFontColor; }
        public string ViewBoxColor { get => Config.Current.AppTheme?.ViewBoxColor; }

        #endregion

        /// <summary>
        /// The page Header Title.
        /// </summary>
        public string PageTitle { get; protected set; }

        /// <summary>
        /// Indicate whether the page has loaded or not.
        /// </summary>
        public bool HasLoaded { get; protected set; } = false;

        /// <summary>
        /// Save the initial data setted on push navigation.
        /// </summary>
        public object InitData { get; protected set; }

        /// <summary>
        /// Indicate whether the page has changes or not.
        /// </summary>
        public bool HasChanges { get; protected set; } = false;

        /// <summary>
        /// Indicate whether the page is loocked or not.
        /// </summary>
        public bool IsLooked { get; protected set; }

        /// <summary>
        /// Indicate whether the pageModel is disposing or not.
        /// </summary>
        public bool IsDisposing { get; protected set; } = false;

        /// <summary>
        /// Indicate whether the user is editing or not.
        /// </summary>
        public bool IsEditing { get; protected set; } = false;

        /// <summary>
        /// Store a list of errors if exists.
        /// </summary>
        public List<string> Errors { get; protected set; } = new List<string>();

        /// <summary>
        /// Store all the errors in a string message to be displayed or audited.
        /// </summary>
        public string ErrorMessage
        {
            get
            {
                string errorMessage = "";
                foreach (var error in Errors) errorMessage += error;
                return errorMessage;
            }
        }

        #endregion

        #region Methods

        public Task<bool> ShowAlert(string title, string message, string accept, string cancel) => Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);

        public Task ShowAlert(string title, string message, string cancel) => Application.Current.MainPage.DisplayAlert(title, message, cancel);

        public Task<string> RegisterToast(IToast toast, ToastTypes type)
        {
            return Task.Run(() => 
            {
                return GlobalDataService.RegisterToast(toast, type);
            });
        }

        public Task UnregisterToast(string toastId)
        {
            return Task.Run(() =>
            {
                GlobalDataService.UnregisterToast(toastId);
            });
        }

        private void ConfigChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Config.Current.AppTheme)) return;

            RaisePropertyChanged(nameof(AppFontColor));
            RaisePropertyChanged(nameof(AppBackgroundColor));
            RaisePropertyChanged(nameof(HeaderAndHeaderAndFooterBackgroundColor));
            RaisePropertyChanged(nameof(CellBackgroundColor));
            RaisePropertyChanged(nameof(CellBorderColor));
            RaisePropertyChanged(nameof(EditorBorderColor));
            RaisePropertyChanged(nameof(EditorBackgroundColor));
            RaisePropertyChanged(nameof(ButtonBackgroundColor));
            RaisePropertyChanged(nameof(DialogBackgroundColor));
            RaisePropertyChanged(nameof(AceptButtonFontColor));
            RaisePropertyChanged(nameof(CancelButtonFontColor));
            RaisePropertyChanged(nameof(ViewBoxColor));
        }

        #endregion
    }
}
