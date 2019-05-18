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

        public string TitleFontColor { get => Config.Current.AppTheme?.TitleFontColor; }
        public string TitleBackgroundColor
        { get => Config.Current.AppTheme?.TitleBackgroundColor; }

        public string CellFontColor { get => Config.Current.AppTheme?.CellFontColor; }
        public string CellBackgroundColor
        { get => Config.Current.AppTheme?.CellBackgroundColor; }
        public string CellBorderColor { get => Config.Current.AppTheme?.CellBorderColor; }

        public string EditiorFontColor { get => Config.Current.AppTheme?.EditiorFontColor; }
        public string EditorBackgroundColor
        { get => Config.Current.AppTheme?.EditorBackgroundColor; }
        public string EditorBorderColor { get => Config.Current.AppTheme?.EditorBorderColor; }

        public string ButtonFontColor { get => Config.Current.AppTheme?.ButtonFontColor; }
        public string ButtonBackgroundColor
        { get => Config.Current.AppTheme?.ButtonBackgroundColor; }

        public string ViewBoxColor { get => Config.Current.AppTheme?.ViewBoxColor; }
        public string LavelFontColor { get => Config.Current.AppTheme?.LavelFontColor; }
        public string PageBackgroundColor
        { get => Config.Current.AppTheme?.PageBackgroundColor; }
        public string LockerBackgroundColor
        { get => Config.Current.AppTheme?.LockerBackgroundColor; }

        public string DialogFontColor { get => Config.Current.AppTheme?.DialogFontColor; }
        public string DialogAceptButtonFontColor
        { get => Config.Current.AppTheme?.DialogAceptButtonFontColor; }
        public string DialogAceptButtonBackgroundColor
        { get => Config.Current.AppTheme?.DialogAceptButtonBackgroundColor; }
        public string DialogCancelButtonFontColor
        { get => Config.Current.AppTheme?.DialogCancelButtonFontColor; }
        public string DialogCancelButtonBackgroundColor
        { get => Config.Current.AppTheme?.DialogCancelButtonBackgroundColor; }
        public string DialogBackgroundColor
        { get => Config.Current.AppTheme?.DialogBackgroundColor; }

        public string FooterFontColor { get => Config.Current.AppTheme?.FooterFontColor; }
        public string FooterBackgroundColor
        { get => Config.Current.AppTheme?.FooterBackgroundColor; }

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

            RaisePropertyChanged(nameof(TitleFontColor));
            RaisePropertyChanged(nameof(TitleBackgroundColor));
            RaisePropertyChanged(nameof(CellFontColor));
            RaisePropertyChanged(nameof(CellBackgroundColor));
            RaisePropertyChanged(nameof(CellBorderColor));
            RaisePropertyChanged(nameof(EditiorFontColor));
            RaisePropertyChanged(nameof(EditorBackgroundColor));
            RaisePropertyChanged(nameof(EditorBorderColor));
            RaisePropertyChanged(nameof(ButtonFontColor));
            RaisePropertyChanged(nameof(ButtonBackgroundColor));
            RaisePropertyChanged(nameof(ViewBoxColor));
            RaisePropertyChanged(nameof(LavelFontColor));
            RaisePropertyChanged(nameof(PageBackgroundColor));
            RaisePropertyChanged(nameof(LockerBackgroundColor));
            RaisePropertyChanged(nameof(DialogFontColor));
            RaisePropertyChanged(nameof(DialogAceptButtonFontColor));
            RaisePropertyChanged(nameof(DialogCancelButtonFontColor));
            RaisePropertyChanged(nameof(DialogAceptButtonBackgroundColor));
            RaisePropertyChanged(nameof(DialogBackgroundColor));
            RaisePropertyChanged(nameof(DialogCancelButtonBackgroundColor));
            RaisePropertyChanged(nameof(FooterFontColor));
            RaisePropertyChanged(nameof(FooterBackgroundColor));
        }

        #endregion
    }
}
