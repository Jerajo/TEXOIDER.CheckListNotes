using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using PortableClasses.Extensions;
using PortableClasses.Implementations;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;
using System.Threading;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class CheckListPageModel : BasePageModel, IPageModel
    {
        public CheckListPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        private int? tabIndex;

        public int TabIndex
        {
            get => tabIndex ?? -1;
            set
            {
                if (tabIndex == value && Tasks != null) return;
                tabIndex = value;
                RunTaskSafe();
                async void RunTaskSafe()
                {
                    try
                    {
                        if (loadingTask == null) loadingTask = Task.Run(() => LoadTask(value, 
                            taskHanlder.Token), taskHanlder.Token);
                        else
                        {
                            if (!loadingTask.IsCompleted) taskHanlder.Cancel();
                            await Task.Delay(200);
                            taskHanlder = new CancellationTokenSource();
                            loadingTask = Task.Run(() => LoadTask(value, taskHanlder.Token),
                                taskHanlder.Token);
                        }
                        await loadingTask;
                    }
                    catch (OperationCanceledException) {}
                }
            }
        }

        private Task loadingTask;
        private CancellationTokenSource taskHanlder = new CancellationTokenSource();

        public CheckListViewModel CheckListModel { get; set; }

        public CheckTaskViewModel SelectedTask
        {
            get => null;
            set
            {
                var task = value;
                if (task?.IsAnimating == false && IsLooked == false && IsDisposing == false)
                {
                    IsLooked = true;
                    GlobalDataService.CurrentIndex = task.Id;
                    if (task.IsTaskGroup == true)
                    {
                        ViewIsDisappearing(null, null);
                        Init(TabIndex);
                    }
                    else PushPageModel<TaskDetailsPageModel>(TabIndex).Wait();
                }
            }
        }

        public FulyObservableCollection<CheckTaskViewModel> Tasks { get; set; }

        //public FulyObservableCollection<CheckTaskViewModel> CheckedTasks { get; set; }

        public Random Randomizer { get; private set; }

        #endregion

        #region Commands

        public ICommand AddNewTask
        {
            get => new DelegateCommand(new Action(AddNewTaskCommand));
        }

        public ICommand UpdateOrRemove
        {
            get => new ParamCommand(new Action<object>(UpdateOrRemoveCommand));
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

        private async void AddNewTaskCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            var model = new CheckTaskModel
            {
                NotifyOn = ToastTypesTime.None,
                ExpirationDate = DateTime.Now.AddHours(5),
                IsChecked = (TabIndex > 0) ? true : false
            };
            await PushPageModel<TaskPageModel>(model);
        }

        private async void UpdateOrRemoveCommand(object value)
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true ||
                !(value is CheckTaskViewModel model)) return;
            IsLooked = true;
            if (model.SelectedReason == SelectedFor.Delete)
            {
                var language = AppResourcesLisener.Languages;
                var title = string.Format(language["AlertDeleteTitle"], model.Name);
                var message = language["TaskListDeleteTaskMessage"];
                var resoult = await ShowAlertQuestion(title, message);
                if (resoult)
                {
                    if (TabIndex <= 0) Tasks.Remove(model);
                    //else CheckedTasks.Remove(model);
                    try { GlobalDataService.RemoveTask(model.Id); }
                    catch (Exception ex) { await ShowAlertError(ex.Message); }
                    finally { IsLooked = false; }
                }
                else model.SelectedReason = SelectedFor.Create;
            }
            else if (model.SelectedReason == SelectedFor.Update)
            {
                GlobalDataService.CurrentIndex = model.Id;
                await PushPageModel<TaskPageModel>(TabIndex);
                return;
            }
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            await PushPageModel<OptionsPageModel>(TabIndex);
        }

        private async void GoBackCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            if (CheckListModel.IsTask == true)
            {
                var currentIndex = GlobalDataService.CurrentIndex;
                GlobalDataService.CurrentIndex = currentIndex.RemoveLastSplit('.');
                await RefreshUI();
            }
            else await PopPageModel(TabIndex);
        }

        #endregion

        #region Overrrided Methods

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
            IsDisposing = true;
            CheckListModel = null;
            if (Errors != null)
            {
                Errors.Clear();
                Errors = null;
            }
            Randomizer = null;
            ClearCollection(Tasks);
            tabIndex = null;
            //ClearCollection(CheckedTasks);
            base.ViewIsDisappearing(sender, e);
            GC.Collect();
            IsDisposing = false;
        }

        private void ClearCollection(FulyObservableCollection<CheckTaskViewModel> colecction)
        {
            if (colecction != null)
            {
                colecction.ClearItems();
                colecction.ItemPropertyChanged -= ItemPropertyChanged;
                colecction.Dispose();
                colecction = null;
                GC.Collect();
            }
        }

        #endregion

        #region Auxiliary Methods

        #region Initialize components

        private async Task InitializeComponent(object data)
        {
            if (!string.IsNullOrEmpty(GlobalDataService.CurrentIndex))
            {
                var task = await GlobalDataService.GetCurrentTask();
                CheckListModel = new CheckListViewModel
                {
                    IsTask = task.IsTaskGroup,
                    LastId = task.LastSubId,
                    Name = task.Name
                };
            }
            else
            {
                var list = await GlobalDataService.GetCurrentList();
                CheckListModel = new CheckListViewModel
                {
                    IsTask = false,
                    LastId = list.LastId,
                    Name = list.Name
                };
            }

            PageTitle = CheckListModel.Name;

            Randomizer = new Random();

            TabIndex = (int)data;
            //int index = (int)data;
            //if (index > 0) TabIndex = index;
            //else LoadTask(index);

            RefreshTaskColor();
        }

        #endregion

        #region Tasks Helpers

        private Task LoadTask(int index, CancellationToken token)
        {
            if (token.IsCancellationRequested) return Task.FromCanceled(token);
            ClearCollection(Tasks);
            if (token.IsCancellationRequested) return Task.FromCanceled(token);

            var value = index == 1;
            Tasks = new FulyObservableCollection<CheckTaskViewModel>();
            Tasks.ItemPropertyChanged += ItemPropertyChanged;

            foreach (var task in GetTasks(GetAllTasks(), value))
            {
                if (token.IsCancellationRequested) return Task.FromCanceled(token);
                Tasks.Add(task);
                Task.Delay(100).Wait();
            }

            return Task.CompletedTask;
        }

        private List<CheckTaskModel> GetAllTasks()
        {
            List<CheckTaskModel> tasks;
            if (!string.IsNullOrEmpty(GlobalDataService.CurrentIndex))
                tasks = GlobalDataService.GetCurrentTask().Result.SubTasks;
            else tasks = GlobalDataService.GetCurrentList().Result.CheckListTasks;
            return tasks ?? new List<CheckTaskModel>();
        }

        private List<CheckTaskViewModel> GetTasks(List<CheckTaskModel> tasks, bool value)
        {
            return tasks.Where(m => m.IsChecked == value)
                .Select(task => new CheckTaskViewModel
                {
                    Id = task.Id,
                    Name = task.Name,
                    ToastId = task.ToastId,
                    CompletedDate = task.CompletedDate,
                    ExpirationDate = task.ExpirationDate,
                    HasExpiration = task.ExpirationDate != null,
                    CompletedTasks = task.SubTasks?.Where(m => m.IsChecked == true).Count() ?? 0,
                    Expiration = task.ExpirationDate?.TimeOfDay,
                    TotalTasks = task.SubTasks?.Count ?? 0,
                    ReminderTime = task.ReminderTime,
                    IsTaskGroup = task.IsTaskGroup,
                    IsChecked = task.IsChecked ?? false,
                    NotifyOn = task.NotifyOn,
                    IsDaily = task.IsDaily
                }).ToList();
        }

        #endregion

        #region Interact with Task

        // Save changes for completed or incompleted task status change
        // prevent doble changes and execute only on IsChecked value change
        private async void ItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsDisposing == true || e.PropertyName != "IsChecked") return;
            if (!(sender is CheckTaskViewModel task)) return;
            if (task?.isAnimating == true) return;
            task.isAnimating = true;

            if (task.IsValid)
            {
                try
                {
                    if (IsDisposing == true) return;
                    //if (task.IsChecked == true && IsDisposing == false) Tasks.Remove(task);
                    //else if (IsDisposing == false) CheckedTasks.Remove(task);
                    await SaveChangesAsync(task);
                    Tasks.Remove(task);
                }
                catch (Exception ex)
                {
                    await ShowAlertError(ex.Message);
                }
                finally
                {
                    task = null;
                }
            }
            else await ShowAlertError(task.ErrorMessage);
        }

        // Save all changes to LocalFolder/Data/fileName.bin
        private async Task SaveChangesAsync(CheckTaskViewModel task)
        {
            if (IsDisposing == true) return;
            if (task == null) throw new ArgumentNullException(nameof(task));
            if (task.IsChecked == true && IsDisposing == false)
            {
                if (task.IsDaily == true)
                {
                    task.ExpirationDate = task.ExpirationDate?.AddDays(1);
                    if (task.NotifyOn != ToastTypesTime.None)
                    {
                        task.ReminderTime = task.ReminderTime?.AddDays(1);
                        await UnregisterToast(task.ToastId);
                        if (task.ReminderTime > DateTime.Now) await RegisterToast(task);
                    }
                }
                else if (task.NotifyOn != ToastTypesTime.None)
                    await UnregisterToast(task.ToastId);
                //Tasks.Remove(task);
            }
            else if (IsDisposing == false)
            {
                if (task.IsDaily == true)
                {
                    task.ExpirationDate = task.ExpirationDate?.AddDays(-1);
                    if (task.NotifyOn != ToastTypesTime.None)
                    {
                        task.ReminderTime = task.ReminderTime?.AddDays(-1);
                        await UnregisterToast(task.ToastId);
                        if (task.ReminderTime > DateTime.Now) await RegisterToast(task);
                    }
                }
                else if (task.NotifyOn != ToastTypesTime.None &&
                    task.ReminderTime > DateTime.Now) await RegisterToast(task);
                //CheckedTasks.Remove(task);
            }

            GlobalDataService.UpdateTask(task);
        }

        #endregion

        #region UI

        public override Task RefreshUI() => RefreshPageModel(TabIndex);

        private async void RefreshTaskColor()
        {
            while (IsDisposing == false)
            {
                if (TabIndex == 0 && Tasks != null && Tasks?.Count > 0)
                {
                    foreach (var task in Tasks.Where(m => m.HasExpiration == true))
                        task.RisePropertyChanged(nameof(task.CellBackgroundColor));
                }
                await Task.Delay(TimeSpan.FromMinutes(1));
            }
        }

        #endregion

        #endregion
    }
}
