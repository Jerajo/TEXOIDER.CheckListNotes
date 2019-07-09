using System;
using FreshMvvm;
using PropertyChanged;
using CheckListNotes.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using PortableClasses.Extensions;
using CheckListNotes.Pages.UserControls;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class BasePageModel : FreshBasePageModel, IDisposable
    {
        public BasePageModel() : base()
        {
            this.PageWasPopped += OnPageWasPopped;
        }

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
        public bool? IsLooked
        {
            get => GlobalDataService.IsProcesing;
            set => GlobalDataService.IsProcesing = value ?? false;
        }

        /// <summary>
        /// Indicate whether the pageModel is disposing or not.
        /// </summary>
        public bool? IsDisposing { get; protected set; } = false;

        /// <summary>
        /// Indicate whether the user is editing or not.
        /// </summary>
        public bool? IsEditing { get; protected set; } = false;

        /// <summary>
        /// Indicate whether the user is searching or not.
        /// </summary>
        public bool? IsSearching { get; set; } = false;

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

        #region Virtuals

        protected virtual void OnDisposing() { }

        #endregion

        #region Navigation

        public Task PushPageModel<T>(object data) where T : FreshBasePageModel
        {
            IsDisposing = true;
            Task.Run(() => OnDisposing());
            return CoreMethods.PushPageModel<T>(data, animate: true);
        }

        public Task PopPageModel(object data)
        {
            IsDisposing = true;
            Task.Run(() => OnDisposing());
            return CoreMethods.PopPageModel(data, animate: true);
        }

        protected virtual void OnPageWasPopped(object sender, EventArgs e)
        {
            if (IsDisposing != true)
            {
                IsDisposing = true;
                Task.Run(() => OnDisposing());
                if (PreviousPageModel is CheckListPageModel && (sender is TaskDetailsPageModel || (sender is TaskPageModel model && model.IsEditing == true)))
                {
                    var index = GlobalDataService.CurrentIndex;
                    if (index.Contains("."))
                        GlobalDataService.CurrentIndex = index.RemoveLastSplit('.');
                    else GlobalDataService.CurrentIndex = null;
                }
                PreviousPageModel.ReverseInit(InitData);
            }
        }

        #endregion

        #region Alerts

        public Task<bool> ShowAlertQuestion(string title, string message, params ButtonModel[] models)
        {
            var page = CurrentPage as IPage;
            var mainGrid = page.GetMainGrid();
            return CustomDialog.Show(mainGrid, title, message, models);
        }

        public Task ShowAlertError(string message)
        {
            var page = CurrentPage as IPage;
            var mainGrid = page.GetMainGrid();
            var title = AppResourcesLisener.Languages["AlertErrorTitle"];
            return CustomDialog.Show(mainGrid, title, message, ButtonModel.ButtonOk);
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
            this.PageWasPopped -= OnPageWasPopped;
            PageTitle = null;
            HasLoaded = null;
            HasChanges = null;
            IsEditing = null;
            IsSearching = null;
            InitData = null;
            if (Errors != null)
            {
                Errors.Clear();
                Errors = null;
            }
            IsDisposing = null;
        }

        #endregion
    }
}
