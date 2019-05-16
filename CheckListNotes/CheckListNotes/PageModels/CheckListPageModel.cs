using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using CheckListNotes.Models.Enums;
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

        public CheckTaskVieModel SelectedTask
        {
            get => null;
            set
            {
                if (value?.IsAnimating == false && !IsLooked && !IsDisposing)
                {
                    IsLooked = true;
                    var model = GlobalDataService.GetCurrentTask(value.Id);
                    CoreMethods.PushPageModel<TaskDetailsPageModel>(true);
                }
            }
        }

        public FulyObservableCollection<CheckTaskVieModel> Tasks { get; set; }

        public FulyObservableCollection<CheckTaskVieModel> CheckedTasks { get; set; }

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

        public ICommand CloseApp
        {
            get => new DelegateCommand(new Action(CloseAppCommand));
        }

        #endregion

        #endregion

        #region Commands

        private void CloseAppCommand()
        {
            if (!HasLoaded || IsLooked || IsDisposing) return;
            IsLooked = true;
            var closer = DependencyService.Get<IDeviceHelper>();
            closer?.CloseApp();
        }

        private async void AddNewTaskCommand()
        {
            if (!HasLoaded || IsLooked || IsDisposing) return;
            IsLooked = true;
            var model = new CheckTaskModel
            {
                Id = CheckListModel.LastId + 1,
                NotifyOn = NotificationTime.None,
                ExpirationDate = DateTime.Now.AddHours(5),
                IsChecked = (TabIndex > 0) ? true : false
            };
            await CoreMethods.PushPageModel<TaskPageModel>(model, false, true);
        }

        private async void UpdateOrRemoveCommand(object value)
        {
            if (!HasLoaded || IsLooked || IsDisposing || 
                !(value is CheckTaskVieModel model)) return;
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
                var temp = GlobalDataService.GetCurrentTask(model.Id);
                await CoreMethods.PushPageModel<TaskPageModel>(temp, false, true);
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
            await CoreMethods.PopPageModel(0, animate: true);
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
            base.ViewIsDisappearing(sender, e);
            IsDisposing = false;
        }

        #endregion

        #region Auxiliary Methods

        private void InitializeComponent(object data)
        {
            var result = int.TryParse(data?.ToString(), out int tabIndex);
            TabIndex = result ? tabIndex : 0;

            var model = GlobalDataService.CurrentList;
            CheckListModel = new CheckListViewModel
            {
                //Id = model.Id, //TODO: Implement IIdentity or EntitiFramawork
                LastId = model.LastId,
                Name = model.Name,
                OldName = model.Name,
                CheckListTasks = model.CheckListTasks
            };

            PageTitle = CheckListModel.Name;

            var temporalList = CheckListModel.CheckListTasks.Select(m => new CheckTaskVieModel
            {
                Id = m.Id,
                Name = m.Name,
                NotifyOn = m.NotifyOn,
                ReminderTime = m.ReminderTime,
                ExpirationDate = m.ExpirationDate,
                CompletedDate = m.CompletedDate,
                IsChecked = m.IsChecked,
                IsDaily = m.IsDaily
            }).ToList();
            var tasks = temporalList.Where(m => !m.IsChecked).ToList();
            var checkedTasks = temporalList.Where(m => m.IsChecked).ToList();

            Tasks = new FulyObservableCollection<CheckTaskVieModel>(tasks);
            Tasks.ItemPropertyChanged += ItemPropertyChanged;

            CheckedTasks = new FulyObservableCollection<CheckTaskVieModel>(checkedTasks);
            CheckedTasks.ItemPropertyChanged += ItemPropertyChanged;

            Randomizer = new Random();
        }

        // Save changes for completed or incompleted task status change
        // prevent doble changes and execute only on IsChecked value change
        private async void ItemPropertyChanged(object sender, ItemPropertyChangedEventArgs e)
        {
            if (IsDisposing || e.PropertyName != "IsChecked") return;

            var item = ((IEnumerable<CheckTaskVieModel>)sender).ElementAt(e.CollectionIndex);

            if (item?.isAnimating == true) return;

            item.isAnimating = true;
            item.IsCompleted = item.IsChecked;

            if (item.IsValid)
            {
                try
                {
                    if (item.IsChecked)
                    {
                        if (IsDisposing) return;
                        if (item.NotifyOn != NotificationTime.None)
                            GlobalDataService.UnregisterToast(item.ToastId);

                        var newItem = new CheckTaskVieModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            ToastId = item.ToastId,
                            ReminderTime = item.ReminderTime,
                            ExpirationDate = item.ExpirationDate,
                            CompletedDate = item.CompletedDate,
                            IsCompleted = item.IsCompleted,
                            IsChecked = item.IsChecked,
                            NotifyOn = item.NotifyOn,
                            IsDaily = item.IsDaily
                        };
                        if (item.IsDaily) newItem.ExpirationDate = item.ExpirationDate?.AddDays(1);
                        Tasks.Remove(item);
                        CheckedTasks.Add(newItem);
                    }
                    else
                    {
                        if (IsDisposing) return;
                        if (item.NotifyOn != NotificationTime.None)
                        {
                            item.ToastId = $"{item.Id}-{Randomizer.Next(1000000)}";
                            var toast = new ToastModel
                            {
                                Id = item.ToastId,
                                Title = "Recordatorio de tarea pendiente",
                                Body = item.Name,
                                Time = item.ReminderTime.Value
                            };
                            GlobalDataService.RegisterToast(toast, ToastType.Notification);
                        }
                        var newItem = new CheckTaskVieModel
                        {
                            Id = item.Id,
                            Name = item.Name,
                            ToastId = item.ToastId,
                            ReminderTime = item.ReminderTime,
                            ExpirationDate = item.ExpirationDate,
                            CompletedDate = item.CompletedDate,
                            IsCompleted = item.IsCompleted,
                            IsChecked = item.IsChecked,
                            NotifyOn = item.NotifyOn,
                            IsDaily = item.IsDaily
                        };
                        if (item.IsDaily) newItem.ExpirationDate = item.ExpirationDate?.AddDays(-1);
                        CheckedTasks.Remove(item);
                        Tasks.Add(newItem);
                    }
                }
                catch (Exception ex)
                {
                    await ShowAlert("Error", ex.Message, "Ok");
                }
                finally
                {
                    SaveChanges();
                }
            }
            else await ShowAlert("Error", item.ErrorMessage, "Ok");
        }

        // Save all changes to LocalFolder/Data/fileName.bin
        private async void SaveChanges()
        {
            if (IsDisposing) return;
            try
            {
                var fullList = CheckedTasks.Concat(Tasks);
                CheckListModel.CheckListTasks = fullList.Select(m => new CheckTaskModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    ToastId = m.ToastId,
                    ReminderTime = m.ReminderTime,
                    ExpirationDate = m.ExpirationDate,
                    CompletedDate = m.CompletedDate,
                    IsChecked = m.IsChecked,
                    NotifyOn = m.NotifyOn,
                    IsDaily = m.IsDaily
                }).ToList();
                GlobalDataService.UpdateList(CheckListModel);
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
