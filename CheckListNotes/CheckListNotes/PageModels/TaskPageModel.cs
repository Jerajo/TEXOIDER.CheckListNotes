using System;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using PortableClasses.Enums;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class TaskPageModel : BasePageModel, IPageModel
    {
        #region Atributes

        private ICommand save;

        #endregion

        public TaskPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        public CheckTaskVieModel Task { get; set; }

        public CheckTaskVieModel OldTask { get; set; }

        public Random Randomizer { get; private set; }

        #endregion

        #region Commands

        public ICommand Save
        {
            get
            {
                return save ?? (save = new DelegateCommand(new Action(SaveCommand), (o) => HasChanges));
            }
        }

        public ICommand Remove
        {
            get => new DelegateCommand(new Action(RemoveCommand));
        }

        public ICommand GoBack
        {
            get => new DelegateCommand(new Action(GoBackCommand));
        }

        #endregion

        #endregion

        #region Commands

        private async void SaveCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            if (Task.IsValid)
            {
                try
                {
                    if (IsEditing && OldTask.NotifyOn != ToastTypesTime.None)
                        GlobalDataService.UnregisterToast(OldTask.ToastId);

                    if (Task.NotifyOn != ToastTypesTime.None)
                    {
                        Task.ToastId = $"{Task.Id}-{Randomizer.Next(1000000)}";
                        var toast = new ToastModel
                        {
                            Id = Task.ToastId,
                            Title = "Recordatorio de tarea pendiente",
                            Body = Task.Name,
                            Time = Task.ReminderTime.Value
                        };
                        GlobalDataService.RegisterToast(toast, (ToastTypes)Config.Current.NotificationType);
                    }

                    if (IsEditing) GlobalDataService.UpdateTask(Task);
                    else GlobalDataService.AddTask(Task);
                }
                catch (Exception ex)
                {
                    Task.Errors.Add($"Error: {ex.Message} \nLinea");
                }
                finally
                {
                    IsLooked = false;
                    if (string.IsNullOrEmpty(Task.ErrorMessage)) GoBackCommand();
                    else await ShowAlert("Error", Task.ErrorMessage, "Ok");
                }
            }
            else
            {
                await ShowAlert("Faltan datos", Task.ErrorMessage, "Ok");
                IsLooked = false;
            }
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

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var tabIndex = Task.IsChecked ? 1 : 0;
            await CoreMethods.PopPageModel(tabIndex, animate: true);
        }

        #endregion

        #region Overrride Methods

        public override void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);

            var model = GlobalDataService.CurrentList;
            var task = initData as CheckTaskModel;

            IsEditing = (!string.IsNullOrEmpty(task.Name)) ? true : false;

            Task = new CheckTaskVieModel
            {
                Id = task.Id,
                Name = task.Name,
                ToastId = task.ToastId,
                ReminderTime = task.ReminderTime,
                ExpirationDate = task.ExpirationDate,
                CompletedDate = task.CompletedDate,
                IsCompleted = task.IsChecked,
                IsChecked = task.IsChecked,
                NotifyOn = task.NotifyOn,
                IsDaily = task.IsDaily
            };

            if (Task.HasExpiration) Task.Expiration = Task.ExpirationDate.Value.TimeOfDay;

            OldTask = new CheckTaskVieModel
            {
                Id = Task.Id,
                Name = Task.Name,
                ToastId = Task.ToastId,
                ReminderTime = Task.ReminderTime,
                ExpirationDate = Task.ExpirationDate,
                CompletedDate = Task.CompletedDate,
                Expiration = Task.Expiration,
                IsCompleted = Task.IsChecked,
                IsChecked = Task.IsChecked,
                NotifyOn = Task.NotifyOn,
                IsDaily = Task.IsDaily
            };

            PageTitle = (IsEditing) ? "Editar Tarea" : "Crear Tarea";

            Randomizer = new Random();

            Task.PropertyChanged += TaskPropertyChanged;

            IsLooked = HasChanges = !(HasLoaded = true);

            base.Init(initData);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            IsDisposing = true;
            Task.PropertyChanged -= TaskPropertyChanged;
            Task = null;
            base.ViewIsDisappearing(sender, e);
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        private async void TaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var name = e.PropertyName;
            if (await UserHasEdited(name) is bool resoult)
            {
                HasChanges = resoult;
                ((DelegateCommand)Save)?.RaiseCanExecuteChanged();
            }
            if (name == "IsChecked") Task.IsCompleted = Task.IsChecked;
        }

        private Task<bool> UserHasEdited(string propertyName)
        {
            return System.Threading.Tasks.Task.Run(() => {
                if (IsDisposing || IsLooked) return false;
                switch(propertyName)
                {
                    case nameof(Task.Name):
                        return (OldTask.Name != Task.Name && !string.IsNullOrEmpty(Task.Name));
                    case nameof(Task.HasExpiration):
                        return (OldTask.HasExpiration != Task.HasExpiration);
                    case nameof(Task.ExpirationDate):
                        return (OldTask.ExpirationDate != Task.ExpirationDate);
                    case nameof(Task.Expiration):
                        return (OldTask.Expiration != Task.Expiration);
                    case nameof(Task.IsCompleted):
                        return (OldTask.IsCompleted != Task.IsCompleted);
                    case nameof(Task.IsChecked):
                        return(OldTask.IsChecked != Task.IsChecked);
                    case nameof(Task.IsDaily):
                        return (OldTask.IsDaily != Task.IsDaily);
                    case nameof(Task.ReminderTime):
                        return (OldTask.ReminderTime != Task.ReminderTime);
                    default: return HasChanges;
                }
            });
        }

        public Task RefreshUI() => System.Threading.Tasks.Task.Run(() => Init(GlobalDataService.CurrentTask));

        #endregion
    }
}
