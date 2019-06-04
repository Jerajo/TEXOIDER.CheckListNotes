using System;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using PortableClasses.Enums;
using PortableClasses.Extensions;
using System.Collections.Generic;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;
using Xamarin.Forms;

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

        public CheckTaskViewModel Task { get; set; }

        public CheckTaskViewModel OldTask { get; set; }

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
                    if (IsEditing && !string.IsNullOrEmpty(OldTask.ToastId) &&
                        (Task.IsChecked || Task.NotifyOn == OldTask.NotifyOn))
                        await UnregisterToast(OldTask.ToastId);

                    if (Task.NotifyOn != ToastTypesTime.None &&
                        (Task.NotifyOn != OldTask.NotifyOn || 
                        Task.ExpirationDate != OldTask.ExpirationDate || 
                        Task.Expiration != OldTask.Expiration)) await RegisterToast(Task);

                    if (IsEditing) GlobalDataService.UpdateTask(Task);
                    else GlobalDataService.AddTask(Task);
                }
                catch (Exception ex)
                {
                    Task.Errors.Add($"Error: {ex.Message}.\n");
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
                GlobalDataService.RemoveTask(Task.Id); IsLooked = false;
                GoBackCommand(); return;
            }
            IsLooked = false;
        }

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var currentIndex = GlobalDataService.CurrentIndex;
            if (PreviousPageModel is CheckListPageModel && !string.IsNullOrEmpty(currentIndex))
            {
                if (currentIndex.Contains("."))
                    GlobalDataService.CurrentIndex = currentIndex.RemoveLastSplit('.');
                else GlobalDataService.CurrentIndex = null;
            }
            await PopPageModel(InitData);
        }

        #endregion

        #region Overrride Methods

        public override async void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);

            PageTitle = (IsEditing) ? "Editar Tarea" : "Crear Tarea";

            if (!(initData is CheckTaskModel task))
                task = await GlobalDataService.GetCurrentTask();

            if (initData is int index) InitData = index;
            else InitData = task.IsChecked ? 1 : 0;

            IsEditing = (!string.IsNullOrEmpty(task.Name)) ? true : false;

            Device.BeginInvokeOnMainThread(() => { 
                Task = new CheckTaskViewModel
                {
                    Id = task.Id,
                    Name = task.Name,
                    ToastId = task.ToastId,
                    TotalTasks = task.SubTasks?.Count ?? 0,
                    CompletedDate = task.CompletedDate,
                    ExpirationDate = task.ExpirationDate,
                    HasExpiration = task.ExpirationDate != null,
                    Expiration = task.ExpirationDate?.TimeOfDay,
                    ReminderTime = task.ReminderTime,
                    IsTaskGroup = task.IsTaskGroup,
                    IsChecked = task.IsChecked,
                    NotifyOn = task.NotifyOn,
                    IsDaily = task.IsDaily
                };

                OldTask = new CheckTaskViewModel
                {
                    Name = Task.Name,
                    ToastId = Task.ToastId,
                    TotalTasks = Task.TotalTasks,
                    IsTaskGroup = Task.IsTaskGroup,
                    ReminderTime = Task.ReminderTime,
                    CompletedDate = Task.CompletedDate,
                    ExpirationDate = Task.ExpirationDate,
                    HasExpiration = Task.HasExpiration,
                    Expiration = Task.Expiration,
                    IsChecked = Task.IsChecked,
                    NotifyOn = Task.NotifyOn,
                    IsDaily = Task.IsDaily
                };

                Task.PropertyChanged += TaskPropertyChanged;
            });

            Randomizer = new Random();


            IsLooked = HasChanges = !(HasLoaded = true);

            base.Init(initData);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            IsDisposing = true;
            Task.PropertyChanged -= TaskPropertyChanged;
            OldTask = null;
            Task = null;
            base.ViewIsDisappearing(sender, e);
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        private async void TaskPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsLooked || IsDisposing) return;

            var name = e.PropertyName;
            if (HasChanges = !OldTask.Equals(Task))
                ((DelegateCommand)Save)?.RaiseCanExecuteChanged();

            if (e.PropertyName == nameof(Task.HasExpiration))
            {
                if (!Task.HasExpiration)
                {
                    Task.notifyOn = ToastTypesTime.None;
                    Task.expirationDate = null;
                    Task.expiration = null;
                    Task.isDaily = false;
                    Task.ReminderTime = null;
                }
                else
                {
                    Task.notifyOn = OldTask.NotifyOn;
                    Task.expirationDate = OldTask.ExpirationDate ?? DateTime.Now;
                    Task.expiration = OldTask.Expiration ?? DateTime.Now.TimeOfDay;
                    Task.isDaily = OldTask.IsDaily;
                    Task.ReminderTime = OldTask.ReminderTime ?? GetReminderTiem();
                }
            }

            if (e.PropertyName == nameof(Task.ExpirationDate))
            {
                Task.expirationDate = Task.ExpirationDate.ChangeTime(Task.Expiration.Value);
                if (Task.NotifyOn != ToastTypesTime.None) Task.ReminderTime = GetReminderTiem();
            }


            if (e.PropertyName == nameof(Task.Expiration))
            {
                Task.expirationDate = Task.ExpirationDate.ChangeTime(Task.Expiration.Value);
                if (Task.NotifyOn != ToastTypesTime.None) Task.ReminderTime = GetReminderTiem();
            }

            if (e.PropertyName == nameof(Task.IsTaskGroup))
            {
                if (Task.IsTaskGroup) Task.TotalTasks = OldTask.TotalTasks;
                if (!Task.IsTaskGroup && Task.TotalTasks > 0)
                {
                    var resoult = await ShowAlert("Advertencia", "Si continua eliminara toda la lista de subtareas.", "Continuar", "Cancelar");
                    if (resoult) Task.TotalTasks = 0;
                    else Task.IsTaskGroup = true;
                }
            }

            if (e.PropertyName == nameof(Task.NotifyOn)) Task.ReminderTime = GetReminderTiem();

            if (e.PropertyName == nameof(Task.IsChecked) && Task.isDaily)
            {
                if (Task.IsChecked) Task.ReminderTime = GetReminderTiem();
                else if (!Task.IsChecked) Task.ReminderTime = GetReminderTiem();
            }
        }

        private DateTime? GetReminderTiem()
        {
            switch (Task.NotifyOn)
            {
                case ToastTypesTime.AHourBefore:
                    if (Task.IsChecked) return Task.ExpirationDate?.AddDays(1).AddHours(-1);
                    else return Task.ExpirationDate?.AddHours(-1);
                case ToastTypesTime.HalfHourBefore:
                    if (Task.IsChecked) return Task.ExpirationDate?.AddDays(1).AddMinutes(-30);
                    else return Task.ExpirationDate?.AddMinutes(-30);
                case ToastTypesTime.FifteenMinutesBefore:
                    if (Task.IsChecked) return Task.ExpirationDate?.AddDays(1).AddMinutes(-15);
                    else return Task.ExpirationDate?.AddMinutes(-15);
                case ToastTypesTime.FifteenMinutesAfter:
                    if (Task.IsChecked) return Task.ExpirationDate?.AddDays(1).AddMinutes(15);
                    else return Task.ExpirationDate?.AddMinutes(15);
                case ToastTypesTime.HalfHourAfter:
                    if (Task.IsChecked) return Task.ExpirationDate?.AddDays(1).AddMinutes(30);
                    else return Task.ExpirationDate?.AddMinutes(30);
                case ToastTypesTime.AHourAfter:
                    if (Task.IsChecked) return Task.ExpirationDate?.AddDays(1).AddHours(1);
                    else return Task.ExpirationDate?.AddHours(1);
                case ToastTypesTime.None:
                default:
                    return null;
            }
        }

        #endregion
    }
}
