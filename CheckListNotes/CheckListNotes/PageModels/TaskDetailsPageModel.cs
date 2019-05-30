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

        private void EditCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            CoreMethods.PushPageModel<TaskPageModel>(InitData, false, true);
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
            await CoreMethods.PushPageModel<OptionsPageModel>(InitData, animate: true);
        }

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            await CoreMethods.PopPageModel(InitData, animate: true);
        }

        #endregion

        #region Overrride Methods

        public override void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);
            InitializeComponent(initData);
            base.Init(initData);
            IsLooked = !(HasLoaded = true);
        }

        public override void ReverseInit(object returnedData)
        {
            IsLooked = !(HasLoaded = false);
            InitializeComponent(returnedData);
            base.ReverseInit(returnedData);
            IsLooked = !(HasLoaded = true);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            Task = null;
            IsLooked = false;
            base.ViewIsDisappearing(sender, e);
        }

        #endregion

        #region Auxiliary Methods

        private void InitializeComponent(object data)
        {
            InitData = data;

            var task = GlobalDataService.CurrentTask;

            Task = new CheckTaskViewModel
            {
                Id = task.Id,
                Name = task.Name,
                NotifyOn = task.NotifyOn,
                ExpirationDate = task.ExpirationDate,
                HasExpiration = task.ExpirationDate != null,
                CompletedDate = task.CompletedDate,
                ReminderTime = task.ReminderTime,
                IsChecked = task.IsChecked,
                IsDaily = task.IsDaily
            };

            if (Task.HasExpiration) Task.Expiration = Task.ExpirationDate.Value.TimeOfDay;

            PageTitle = "Detalles de Tarea";
        }

        public Task RefreshUI() => System.Threading.Tasks.Task.Run(() => Init(null));

        #endregion
    }
}
