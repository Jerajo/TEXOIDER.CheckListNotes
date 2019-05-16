using System;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.Threading.Tasks;
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

        public CheckTaskVieModel Task { get; set; }

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

        private void EditCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var model = GlobalDataService.GetCurrentTask(Task.Id);
            CoreMethods.PushPageModel<TaskPageModel>(model, false, true);
        }
        private async void RemoveCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var resoult = await ShowAlert("Pregunta!", "¿Estás seguro de que quieres eliminar la tarea?", "Aceptar", "Cancelar");
            if (resoult)
            {
                GlobalDataService.RemoveTask(Task);
                IsLooked = false;
                GoBackCommand();
                return;
            }
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            await CoreMethods.PushPageModel<OptionsPageModel>(0, animate: true);
        }

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var tabIndex = Task.IsChecked ? 1 : 0;
            await CoreMethods.PopPageModel(0, animate: true);
        }

        #endregion

        #region Overrride Methods

        public override void Init(object initData)
        {
            InitializeComponent();
            base.Init(initData);
        }

        public override void ReverseInit(object returnedData)
        {
            InitializeComponent();
            base.ReverseInit(returnedData);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            Task = null;
            IsLooked = false;
            base.ViewIsDisappearing(sender, e);
        }

        #endregion

        #region Auxiliary Methods

        private void InitializeComponent()
        {
            HasLoaded = false;

            var model = GlobalDataService.CurrentList;
            var task = GlobalDataService.CurrentTask;

            Task = new CheckTaskVieModel
            {
                Id = task.Id,
                Name = task.Name,
                ExpirationDate = task.ExpirationDate,
                CompletedDate = task.CompletedDate,
                NotifyOn = task.NotifyOn,
                ReminderTime = task.ReminderTime,
                IsChecked = task.IsChecked,
                IsCompleted = task.IsChecked,
                IsDaily = task.IsDaily
            };

            if (Task.HasExpiration) Task.Expiration = Task.ExpirationDate.Value.TimeOfDay;

            PageTitle = "Detalles de Tarea";

            HasLoaded = true;
        }

        public Task RefreshUI() => System.Threading.Tasks.Task.Run(() => Init(null));

        #endregion
    }
}
