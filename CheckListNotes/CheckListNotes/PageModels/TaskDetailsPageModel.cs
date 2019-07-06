using System;
using Xamarin.Forms;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.Threading.Tasks;
using PortableClasses.Extensions;
using PortableClasses.Implementations;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class TaskDetailsPageModel : BasePageModel, IPageModel
    {
        public TaskDetailsPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        public CheckTaskViewModel Task { get; set; }

        #endregion

        #region Commands

        public ICommand Edit
        {
            get => new DelegateCommand(new Action(EditCommand));
        }

        public ICommand Remove
        {
            get => new DelegateCommand(new Action(RemoveCommand));
        }

        public ICommand GoBack
        {
            get => new DelegateCommand(new Action(GoBackCommand));
        }

        public ICommand GoToOptions
        {
            get => new DelegateCommand(new Action(GoToOptionsCommand));
        }

        #endregion

        #endregion

        #region Commands

        private async void EditCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            await PushPageModel<TaskPageModel>(InitData);
        }
        private async void RemoveCommand()
        {
            var language = AppResourcesLisener.Languages;
            var title = string.Format(language["AlertDeleteTitle"], Task.Name);
            var message = language["TaskListDeleteTaskMessage"];
            var resoult = await ShowAlertQuestion(title, message);
            string error = "";
            if (resoult)
            {
                try { GlobalDataService.RemoveTask(Task.Id); }
                catch (Exception ex) { error = ex.Message; }
                finally
                {
                    IsLooked = false;
                    if (string.IsNullOrEmpty(error)) GoBackCommand();
                    else await ShowAlertError(error);
                }
                return;
            }
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            await PushPageModel<OptionsPageModel>(InitData);
        }

        private async void GoBackCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            if (Task.Id.Contains("."))
                GlobalDataService.CurrentIndex = Task.Id.RemoveLastSplit('.');
            else GlobalDataService.CurrentIndex = null;
            await PopPageModel(InitData);
        }

        #endregion

        #region Overrride Methods

        public override async void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);
            await InitializeComponent(initData);
            base.Init(initData);
            IsLooked = !(HasLoaded = true);
        }

        public override async void ReverseInit(object returnedData)
        {
            IsLooked = !(HasLoaded = false);
            await InitializeComponent(returnedData);
            base.ReverseInit(returnedData);
            IsLooked = !(HasLoaded = true);
        }

        protected override void OnDisposing()
        {
            IsDisposing = true;
            Task = null;
            InitData = null;
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        private async Task InitializeComponent(object data)
        {
            if (InitData == null && data is int index) InitData = index;
            if (data is string args) 
            {
                InitData = 0;
                var arguments = QueryParser.Parse(args);
                GlobalDataService.PreviousListName = GlobalDataService.CurrentListName;
                GlobalDataService.PreviousIndex = GlobalDataService.CurrentIndex;
                GlobalDataService.CurrentListName = arguments["listName"];
                GlobalDataService.CurrentIndex = arguments["taskId"];
            }

            var task = await GlobalDataService.GetCurrentTask();

            Task = new CheckTaskViewModel
            {
                Id = task.Id,
                Name = task.Name,
                NotifyOn = task.NotifyOn,
                ExpirationDate = task.ExpirationDate,
                HasExpiration = task.ExpirationDate != null,
                Expiration = task.ExpirationDate?.TimeOfDay,
                CompletedDate = task.CompletedDate,
                ReminderTime = task.ReminderTime,
                IsChecked = task.IsChecked ?? false,
                IsDaily = task.IsDaily
            };
        }

        #endregion
    }
}
