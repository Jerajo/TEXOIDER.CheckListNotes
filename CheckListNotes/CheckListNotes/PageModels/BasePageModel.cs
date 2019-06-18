using System;
using FreshMvvm;
using Xamarin.Forms;
using PropertyChanged;
using CheckListNotes.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class BasePageModel : FreshBasePageModel, IDisposable
    {
        public BasePageModel() : base() { }

        #region SETTERS AND GETTERS

        /// <summary>
        /// The page Header Title.
        /// </summary>
        public string PageTitle { get; protected set; }

        /// <summary>
        /// Indicate whether the page has loaded or not.
        /// </summary>
        public bool? HasLoaded { get; protected set; } = false;

        /// <summary>
        /// Save the initial data setted on push navigation.
        /// </summary>
        public object InitData { get; protected set; }

        /// <summary>
        /// Indicate whether the page has changes or not.
        /// </summary>
        public bool? HasChanges { get; protected set; } = false;

        /// <summary>
        /// Indicate whether the page is loocked or not.
        /// </summary>
        public bool? IsLooked { get; protected set; }

        /// <summary>
        /// Indicate whether the pageModel is disposing or not.
        /// </summary>
        public bool? IsDisposing { get; protected set; } = false;

        /// <summary>
        /// Indicate whether the user is editing or not.
        /// </summary>
        public bool? IsEditing { get; protected set; } = false;

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

        /// <summary>
        /// Indicate whether the app is in debug mode or not.
        /// </summary>
        public bool IsDebug
        {
            get
            {
                var debugin = false;
                #if DEBUG
                debugin = true;
                #endif
                return debugin;
            }
        }

        #endregion

        #region Methods

        #region Navigation

        public Task PushPageModel<T>(object data) where T : FreshBasePageModel => CoreMethods.PushPageModel<T>(data, animate: true);

        public Task PopPageModel(object data) => CoreMethods.PopPageModel(data, animate: true);

        #endregion

        #region Alerts

        public Task<bool> ShowAlertQuestion(string title, string message)
        {
            var language = AppResourcesLisener.Languages;
            var accept = language["ButtonOkText"];
            var cancel = language["ButtonCancelText"];
            return Application.Current.MainPage.DisplayAlert(title, message, accept, cancel);
        }

        public Task ShowAlertError(string message)
        {
            var language = AppResourcesLisener.Languages;
            var title = language["AlertErrorTitle"];
            var cancel = language["ButtonOkText"];
            return Application.Current.MainPage.DisplayAlert(title, message, cancel);
        }

        #endregion

        #region Toast

        public Task RegisterToast(CheckTaskViewModel task) =>
            GlobalDataService.RegisterToast(task);

        public Task UnregisterToast(string toastId) => 
            GlobalDataService.UnregisterToast(toastId);

        #endregion

        #region UI

        public virtual Task RefreshUI() => RefreshPageModel();

        protected virtual async Task RefreshPageModel(object initData = null)
        {
            if (IsEditing == true)
            {
                if (await ShowAlertQuestion("Cambios en los datos!", "Algun proseso de fondo a modificado los datos.")) return;
            }
            var data = initData ?? InitData ?? 0;
            ViewIsDisappearing(null, null); Init(data);
        }

        #endregion
        
        #endregion

        #region Dispose

        ~BasePageModel()
        {
            if (IsDisposing == false) Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            IsDisposing = isDisposing;
            if (IsDisposing == true) Dispose();
        }

        public void Dispose()
        {
            PageTitle = null;
            HasLoaded = null;
            IsLooked = null;
            IsDisposing = null;
            HasChanges = null;
            IsEditing = null;
            InitData = null;
            if (Errors != null)
            {
                Errors.Clear();
                Errors = null;
            }
        }

        #endregion
    }
}
