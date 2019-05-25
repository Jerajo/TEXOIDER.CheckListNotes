﻿using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
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

        public int TabIndex { get; set; }

        public CheckListViewModel CheckListModel { get; set; }

        public CheckTaskViewModel SelectedTask
        {
            get => null;
            set
            {
                var task = value;
                if (task?.IsAnimating == false && !IsLooked && !IsDisposing)
                {
                    IsLooked = true;
                    if (task.IsTaskGroup)
                    {
                        GlobalDataService.Push(task);
                        InitializeComponent(TabIndex);
                        IsLooked = false;
                    }
                    else
                    {
                        var model = GlobalDataService.GetCurrentTask(task.Id);
                        CoreMethods.PushPageModel<TaskDetailsPageModel>(TabIndex, animate: true);
                    }
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
            if (!HasLoaded || IsLooked || IsDisposing) return;
            IsLooked = true;
            var model = new CheckTaskModel
            {
                Id = CheckListModel.LastId + 1,
                NotifyOn = ToastTypesTime.None,
                ExpirationDate = DateTime.Now.AddHours(5),
                IsChecked = (TabIndex > 0) ? true : false
            };
            await CoreMethods.PushPageModel<TaskPageModel>(model, animate: true);
        }

        private async void UpdateOrRemoveCommand(object value)
        {
            if (!HasLoaded || IsLooked || IsDisposing || 
                !(value is CheckTaskViewModel model)) return;
            IsLooked = true;
            if (model.SelectedReason == SelectedFor.Delete)
            {
                var resoult = await ShowAlert("Pregunta!", "¿Estás seguro de que quieres eliminar esta tarea?", "Aceptar", "Cancelar");
                if (resoult)
                {
                    var temp = CheckListModel.CheckListTasks.Find(m => m.Id == model.Id);
                    CheckListModel.CheckListTasks.Remove(temp);
                    if (TabIndex <= 0) Tasks.Remove(model);
                    else CheckedTasks.Remove(model);
                    GlobalDataService.RemoveTask(model);
                }
                else model.SelectedReason = SelectedFor.Create;
            }
            else if (model.SelectedReason == SelectedFor.Update)
            {
                var task = GlobalDataService.GetCurrentTask(model.Id);
                await CoreMethods.PushPageModel<TaskPageModel>(TabIndex, false, true);
                return;
            }
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (!HasLoaded || IsLooked || IsDisposing) return;
            IsLooked = true;
            await CoreMethods.PushPageModel<OptionsPageModel>(TabIndex, animate: true);
        }

        private async void GoBackCommand()
        {
            if (!HasLoaded || IsLooked || IsDisposing) return;
            IsLooked = true;
            if (CheckListModel.IsTask)
            {
                GlobalDataService.Pop();
                InitializeComponent(TabIndex);
                IsLooked = false;
            }
            else await CoreMethods.PopPageModel(0, animate: true);
        }

        #endregion

        #region Overrrided Methods

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
            IsDisposing = true;
            CheckListModel = null;
            if (Tasks != null)
            {
                Tasks.ItemPropertyChanged -= ItemPropertyChanged;
                Tasks.Clear();
                Tasks = null;
            }
            if (CheckedTasks != null)
            {
                CheckedTasks.ItemPropertyChanged -= ItemPropertyChanged;
                CheckedTasks.Clear();
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

        private void InitializeComponent(object data)
        {
            TabIndex = (int)data;

            if (GlobalDataService.CheckTaskModelStack.Count > 0)
            {
                var task = GlobalDataService.CheckTaskModelStack.Peek();
                CheckListModel = new CheckListViewModel
                {
                    //Id = model.Id, //TODO: Implement IIdentity or EntitiFramawork
                    IsTask = task.IsTaskGroup,
                    LastId = task.LastSubId,
                    Name = task.Name,
                    OldName = task.Name,
                    CheckListTasks = task.SubTasks
                };
            }
            else
            {
                var list = GlobalDataService.CurrentList;
                CheckListModel = new CheckListViewModel
                {
                    //Id = model.Id, //TODO: Implement IIdentity or EntitiFramawork
                    IsTask = false,
                    LastId = list.LastId,
                    Name = list.Name,
                    OldName = list.Name,
                    CheckListTasks = list.CheckListTasks
                };
            }

            PageTitle = CheckListModel.Name;

            var temporalList = CheckListModel.CheckListTasks.Select(m => new CheckTaskViewModel
            {
                Id = m.Id,
                Name = m.Name,
                ToastId = m.ToastId,
                SubTasks = m.SubTasks,
                ExpirationDate = m.ExpirationDate,
                Expiration = m.ExpirationDate?.TimeOfDay,
                CompletedDate = m.CompletedDate,
                ReminderTime = m.ReminderTime,
                IsTaskGroup = m.IsTaskGroup,
                IsChecked = m.IsChecked,
                IsDaily = m.IsDaily
            }).ToList();

            var tasks = temporalList.Where(m => !m.IsChecked).ToList();
            Tasks = new FulyObservableCollection<CheckTaskViewModel>(tasks);
            Tasks.ItemPropertyChanged += ItemPropertyChanged;

            var checkedTasks = temporalList.Where(m => m.IsChecked).ToList();
            CheckedTasks = new FulyObservableCollection<CheckTaskViewModel>(checkedTasks);
            CheckedTasks.ItemPropertyChanged += ItemPropertyChanged;

            Randomizer = new Random();
        }

        // Save changes for completed or incompleted task status change
        // prevent doble changes and execute only on IsChecked value change
        private async void ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            if (IsDisposing || e.PropertyName != "IsChecked") return;

            var task = ((IEnumerable<CheckTaskViewModel>)sender).ElementAt(e.CollectionIndex);

            if (task?.isAnimating == true) return;

            task.isAnimating = true;

            if (task.IsValid)
            {
                try
                {
                    if (task.IsChecked)
                    {
                        if (IsDisposing) return;
                        if (task.NotifyOn != ToastTypesTime.None)
                            GlobalDataService.UnregisterToast(task.ToastId);
                        task.ToastId = string.Empty;
                    }
                    else
                    {
                        if (IsDisposing) return;
                        if (task.NotifyOn != ToastTypesTime.None)
                        {
                            task.ToastId = $"{task.Id}-{Randomizer.Next(1000000)}";
                            var toast = new ToastModel
                            {
                                Id = task.ToastId,
                                Title = "Recordatorio de tarea pendiente",
                                Body = task.Name,
                                Time = task.ReminderTime.Value
                            };
                            GlobalDataService.RegisterToast(toast,
                                (ToastTypes)Config.Current.NotificationType);
                        }
                    }

                    var newItem = new CheckTaskViewModel
                    {
                        Id = task.Id,
                        Name = task.Name,
                        ToastId = task.ToastId,
                        SubTasks = task.SubTasks,
                        ExpirationDate = task.ExpirationDate,
                        CompletedDate = task.CompletedDate,
                        ReminderTime = task.ReminderTime,
                        IsTaskGroup = task.IsTaskGroup,
                        Expiration = task.Expiration,
                        IsChecked = task.IsChecked,
                        IsDaily = task.IsDaily
                    };

                    if (task.IsChecked)
                    { 
                        if (task.IsDaily) newItem.ExpirationDate = task.ExpirationDate?.AddDays(1);
                        Tasks.Remove(task);
                        CheckedTasks.Add(newItem);
                    }
                    else
                    {
                        if (task.IsDaily) newItem.ExpirationDate = task.ExpirationDate?.AddDays(-1);
                        CheckedTasks.Remove(task);
                        Tasks.Add(newItem);
                    }
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", ex.Message, "Ok");
                }
                finally
                {
                    SaveChanges(task);
                }
            }
            else await ShowAlert("Error", task.ErrorMessage, "Ok");
        }

        // Save all changes to LocalFolder/Data/fileName.bin
        private async void SaveChanges(CheckTaskViewModel data)
        {
            if (IsDisposing || data == null) return;
            try
            {
                var task = GlobalDataService.GetCurrentTask(data.Id);
                task.ToastId = data.ToastId;
                task.IsChecked = data.IsChecked;

                if (GlobalDataService.CheckTaskModelStack.Count > 0)
                {
                    var container = GlobalDataService.CheckTaskModelStack.Peek();
                    if (container.IsTaskGroup)
                        container.IsChecked = !container.SubTasks.Any(m => !m.IsChecked);
                }

                GlobalDataService.UpdateCurrentTask();
            }
            catch (Exception ex)
            {
                await ShowAlert("Error", ex.Message, "Ok");
            }
        }

        public Task RefreshUI() => Task.Run(() => Init(TabIndex));

        #endregion
    }
}
