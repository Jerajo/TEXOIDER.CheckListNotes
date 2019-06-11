using System;
using Xamarin.Forms;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.ComponentModel;
using PortableClasses.Enums;
using PortableClasses.Extensions;
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

        public CheckTaskViewModel Task { get; set; }

        public CheckTaskViewModel OldTask { get; set; }

        public Random Randomizer { get; private set; }

        #endregion

        #region Commands

        public ICommand Save
        {
            get
            {
                return save ?? (save = new DelegateCommand(new Action(SaveCommand), (o) => HasChanges == true));
            }
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
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            if (Task.IsValid)
            {
                try
                {
                    if (IsEditing == true && !string.IsNullOrEmpty(OldTask.ToastId) &&
                    (Task.IsChecked || Task.NotifyOn == OldTask.NotifyOn))
                        await UnregisterToast(OldTask.ToastId);

                    if (Task.NotifyOn != ToastTypesTime.None &&
                        (Task.NotifyOn != OldTask.NotifyOn ||
                        Task.ExpirationDate != OldTask.ExpirationDate ||
                        Task.Expiration != OldTask.Expiration)) await RegisterToast(Task);

                    if (IsEditing == true) GlobalDataService.UpdateTask(Task);
                    else GlobalDataService.AddTask(Task);
                }
                catch (Exception ex) { Task.Errors.Add(ex.Message); }
                finally
                {
                    IsLooked = false;
                    if (string.IsNullOrEmpty(Task.ErrorMessage)) GoBackCommand();
                    else await ShowAlertError(Task.ErrorMessage);
                }
            }
            else
            {
                await ShowAlertError(Task.ErrorMessage);
                IsLooked = false;
            }
        }

        private async void GoBackCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            var currentIndex = GlobalDataService.CurrentIndex;
            if (PreviousPageModel is CheckListPageModel && !string.IsNullOrEmpty(currentIndex)
                && IsEditing == true)
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

            await System.Threading.Tasks.Task.Run(() => 
            { 
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var resources = AppResourcesLisener.Current;

                    if (!(initData is CheckTaskModel task))
                        task = await GlobalDataService.GetCurrentTask();

                    if (initData is int index) InitData = index;
                    else InitData = task.IsChecked == true ? 1 : 0;

                    IsEditing = (!string.IsNullOrEmpty(task.Name)) ? true : false;

                    PageTitle = (IsEditing == true) ? 
                        resources["TaskFormPageTitleOnEdition"] :
                        resources["TaskFormPageTitle"];
            
                    Task = new CheckTaskViewModel
                    {
                        Id = task.Id,
                        Name = task.Name,
                        ToastId = task.ToastId,
                        LastSubId = task.LastSubId,
                        TotalTasks = task.SubTasks?.Count ?? 0,
                        CompletedDate = task.CompletedDate,
                        ExpirationDate = task.ExpirationDate,
                        HasExpiration = task.ExpirationDate != null,
                        Expiration = task.ExpirationDate?.TimeOfDay,
                        ReminderTime = task.ReminderTime,
                        IsTaskGroup = task.IsTaskGroup ?? false,
                        IsChecked = task.IsChecked ?? false,
                        NotifyOn = task.NotifyOn ?? ToastTypesTime.None,
                        IsDaily = task.IsDaily ?? false
                    };

                    OldTask = new CheckTaskViewModel
                    {
                        Name = Task.Name,
                        ToastId = Task.ToastId,
                        LastSubId = Task.LastSubId,
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
            });

            Randomizer = new Random();
            base.Init(initData);
            IsLooked = HasChanges = !(HasLoaded = true);
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
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;

            var name = e.PropertyName;
            HasChanges = !OldTask.Equals(Task);
            ((DelegateCommand)Save)?.RaiseCanExecuteChanged();

            if (e.PropertyName == nameof(Task.HasExpiration))
            {
                if (Task.HasExpiration == false)
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
                if (Task.IsTaskGroup == true)
                {
                    Task.LastSubId = OldTask.LastSubId ?? 0;
                    Task.TotalTasks = OldTask.TotalTasks;
                }
                if (Task.IsTaskGroup == false && Task.TotalTasks > 0)
                {
                    var resourses = AppResourcesLisener.Current;
                    var title = string.Format(resourses["AlertLoseDataTitle"], Task.Name);
                    var message = resourses["TaskFormSwictIsTaskGroupActionMessage"];
                    var resoult = await ShowAlertQuestion(title, message);
                    if (!resoult) Task.IsTaskGroup = true;
                    else 
                    {
                        Task.TotalTasks = 0;
                        Task.LastSubId = null;
                    }
                }
            }

            if (e.PropertyName == nameof(Task.NotifyOn)) Task.ReminderTime = GetReminderTiem();

            if (e.PropertyName == nameof(Task.IsChecked) && Task.isDaily == true)
            {
                if (Task.IsChecked) Task.ReminderTime = GetReminderTiem();
                else if (!Task.IsChecked) Task.ReminderTime = GetReminderTiem();
            }
        }

        private DateTime? GetReminderTiem()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return null;
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
