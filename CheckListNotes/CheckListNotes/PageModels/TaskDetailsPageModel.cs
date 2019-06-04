using System;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.Threading.Tasks;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;
using PortableClasses.Extensions;
using Xamarin.Forms;

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
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            await PushPageModel<TaskPageModel>(InitData);
        }
        private async void RemoveCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var resoult = await ShowAlert("Pregunta!", "¿Estás seguro de que quieres eliminar la tarea?", "Aceptar", "Cancelar");
            if (resoult)
            {
                GlobalDataService.RemoveTask(Task.Id); IsLooked = false;
                GoBackCommand(); return;
            }
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            await PushPageModel<OptionsPageModel>(InitData);
        }

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            if (!GlobalDataService.CurrentIndex.Contains("."))
                GlobalDataService.CurrentIndex = null;
            else GlobalDataService.CurrentIndex = 
                    GlobalDataService.CurrentIndex.RemoveLastSplit('.');
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

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            Task = null;
            IsLooked = false;
            base.ViewIsDisappearing(sender, e);
        }

        #endregion

        #region Auxiliary Methods

        private async Task InitializeComponent(object data)
        {
            InitData = data;

            PageTitle = "Detalles de Tarea";

            var task = await GlobalDataService.GetCurrentTask();

            Device.BeginInvokeOnMainThread(() => { 
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
                    IsChecked = task.IsChecked,
                    IsDaily = task.IsDaily
                };
            });
        }

        #endregion
    }
}
