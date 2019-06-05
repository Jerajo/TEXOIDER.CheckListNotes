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

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class CheckListPageModel : BasePageModel, IPageModel
    {
        public CheckListPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        private int tabIndex = 0;

        public int TabIndex
        {
            get => tabIndex;
            set
            {
                tabIndex = value;
                Device.BeginInvokeOnMainThread(() => 
                {
                    Task.Run(() => 
                    {
                        if (tabIndex > 0)
                        {
                            Tasks.Clear();
                            var taskList = GetAllTasks();
                            foreach(var task in GetTasks(taskList, true))
                                CheckedTasks.Add(task);
                        }
                        else
                        {
                            CheckedTasks.Clear();
                            var taskList = GetAllTasks();
                            foreach (var task in GetTasks(taskList, false))
                                Tasks.Add(task);
                        }
                    });
                });
            }
        }

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

        public FulyObservableCollection<CheckTaskViewModel> CheckedTasks { get; set; }

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
                var resoult = await ShowAlert("Pregunta!", "¿Estás seguro de que quieres eliminar esta tarea?", "Aceptar", "Cancelar");
                if (resoult)
                {
                    if (TabIndex <= 0) Tasks.Remove(model);
                    else CheckedTasks.Remove(model);
                    GlobalDataService.RemoveTask(model.Id);
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
            else await PopPageModel(0);
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
            if (Tasks != null)
            {
                Tasks.Clear();
                Tasks.ItemPropertyChanged -= ItemPropertyChanged;
                Tasks = null;
            }
            if (CheckedTasks != null)
            {
                CheckedTasks.Clear();
                CheckedTasks.ItemPropertyChanged -= ItemPropertyChanged;
                CheckedTasks = null;
            }
            if (Errors != null)
            {
                Errors.Clear();
                Errors = null;
            }
            Randomizer = null;
            base.ViewIsDisappearing(sender, e);
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        #region Initialize components

        private Task InitializeComponent(object data)
        {
            return Task.Run(() =>
            {
                Device.BeginInvokeOnMainThread(async () => {
                    TabIndex = (int)data;
                    List<CheckTaskModel> taskList;
                    if (!string.IsNullOrEmpty(GlobalDataService.CurrentIndex))
                    {
                        var task = await GlobalDataService.GetCurrentTask();
                        CheckListModel = new CheckListViewModel
                        {
                            IsTask = task.IsTaskGroup,
                            LastId = task.LastSubId,
                            Name = task.Name
                        };
                        taskList = task.SubTasks ?? new List<CheckTaskModel>();
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
                        taskList = list.CheckListTasks ?? new List<CheckTaskModel>();
                    }

                    PageTitle = CheckListModel.Name;

                    Tasks = new FulyObservableCollection<CheckTaskViewModel>();
                    Tasks.ItemPropertyChanged += ItemPropertyChanged;

                    CheckedTasks = new FulyObservableCollection<CheckTaskViewModel>();
                    CheckedTasks.ItemPropertyChanged += ItemPropertyChanged;

                    if (TabIndex > 0 && taskList.Count > 0)
                        CheckedTasks.AddRange(GetTasks(taskList, true));
                    else if (taskList.Count > 0) Tasks.AddRange(GetTasks(taskList, false));

                    Randomizer = new Random();

                    RefreshTaskColor();
                });
            });
        }

        #endregion

        #region Tasks Helpers

        private List<CheckTaskModel> GetAllTasks()
        {
            List<CheckTaskModel> tasks;
            if (!string.IsNullOrEmpty(GlobalDataService.CurrentIndex))
                tasks = GlobalDataService.GetCurrentTask().Result.SubTasks;
            else tasks = GlobalDataService.GetCurrentList().Result.CheckListTasks;
            return tasks ?? new List<CheckTaskModel>();
        }

        private IEnumerable<CheckTaskViewModel> GetTasks(List<CheckTaskModel> tasks, bool value)
        {
            if (tasks.Count <= 0) yield break;
            else
            {
                foreach (var task in tasks.Where(m => m.IsChecked == value))
                    yield return new CheckTaskViewModel
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
                    };
            }
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
                    if (task.IsChecked == true && IsDisposing == false) Tasks.Remove(task);
                    else if (IsDisposing == false) CheckedTasks.Remove(task);
                    await SaveChangesAsync(task);
                }
                catch (Exception ex)
                {
                    await ShowAlert("Fallo en la acción.", ex.Message, "Ok");
                }
                finally
                {
                    task = null;
                }
            }
            else await ShowAlert("Fallo en la acción.", task.ErrorMessage, "Ok");
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
                Tasks.Remove(task);
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
                CheckedTasks.Remove(task);
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
