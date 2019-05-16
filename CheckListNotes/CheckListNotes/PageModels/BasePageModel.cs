using FreshMvvm;
using Xamarin.Forms;
using PropertyChanged;
using PortableClasses.Enums;
using CheckListNotes.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using PortableClasses.Interfaces;
using CheckListNotes.Models.Interfaces;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class BasePageModel : FreshBasePageModel, IAppTheme
    {
        public BasePageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Theme

        /// <summary>
        /// The page Header Title.
        /// </summary>
        public IAppTheme Theme
        {
            get => Config.Current.AppTheme;
            set
            {
                Config.Current.AppTheme = value;
                Task.Run(() => ReloadUI());
            }
        }

        public string TitleFontColor { get; set; }

        public string TitleBackgroundColor { get; set; }

        public string CellFontColor { get; set; }

        public string CellBackgroundColor { get; set; }

        public string CellBorderColor { get; set; }

        public string ButtonFontColor { get; set; }

        public string ButtonBackgroundColor { get; set; }

        public string EditiorFontColor { get; set; }

        public string EditorBackgroundColor { get; set; }

        public string ViewBoxColor { get; set; }

        public string PageBackgroundColor { get; set; }

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

        public Task<string> RegisterToast(IToast toast, ToastType type)
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

        protected Task ReloadUI()
        {
            var theme = Config.Current.AppTheme;
            if (theme == null) return Task.CompletedTask;

            TitleFontColor = theme.TitleFontColor;
            TitleBackgroundColor = theme.TitleBackgroundColor;
            CellFontColor = theme.CellFontColor;
            CellBackgroundColor = theme.CellBackgroundColor;
            CellBorderColor = theme.CellBorderColor;
            ButtonFontColor = theme.ButtonFontColor;
            ButtonBackgroundColor = theme.ButtonBackgroundColor;
            EditiorFontColor = theme.EditiorFontColor;
            EditorBackgroundColor = theme.EditorBackgroundColor;
            ViewBoxColor = theme.ViewBoxColor;
            PageBackgroundColor = theme.PageBackgroundColor;

            return Task.CompletedTask;
        }

        #endregion
    }
}
