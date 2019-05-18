using System;
using System.Linq;
using PropertyChanged;
using System.Windows.Input;
using CheckListNotes.Models;
using System.ComponentModel;
using PortableClasses.Enums;
using System.Threading.Tasks;
using System.Collections.Generic;
using PortableClasses.Implementations;
using CheckListNotes.Models.Interfaces;
using CheckListNotes.PageModels.Commands;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.IO;
using Newtonsoft.Json;

namespace CheckListNotes.PageModels
{
    [AddINotifyPropertyChangedInterface]
    public class ListOfCheckListsPageModel : BasePageModel, IPageModel
    {
        #region Atributes

        private bool isNewListFormVisible = false;
        private CheckListViewModel lastCeckListSelected;

        #endregion

        public ListOfCheckListsPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        public string CheckListName { get; set; }

        public bool IsNewListFormVisible
        {
            get => isNewListFormVisible;
            set
            {
                IsLooked = isNewListFormVisible = value;
            }
        }

        public CheckListViewModel SelectedCheckList
        {
            get => null;
            set
            {
                if (value?.IsAnimating == false && !IsLooked && !IsDisposing)
                {
                    IsLooked = true;
                    GlobalDataService.GetCurrentList(value.Name);
                    CoreMethods.PushPageModel<CheckListPageModel>(true);
                }
            }
        }

        public FulyObservableCollection<CheckListViewModel> ListOfCheckLists { get; set; }

        #endregion

        #region Commands

        public ICommand Save
        {
            get => new ParamCommand(new Action<object>(SaveCommand));
        }

        public ICommand UpdateOrRemove
        {
            get => new ParamCommand(new Action<object>(UpdateOrRemoveCommand));
        }

        public ICommand GoToOptions
        {
            get => new DelegateCommand(new Action(GoToOptionsCommand));
        }

        public ICommand OpenForm
        {
            get => new DelegateCommand(new Action(OpenFormCommand));
        }

        public ICommand Cancel
        {
            get => new DelegateCommand(new Action(CancelCommand));
        }

        #endregion

        #endregion

        #region Commands

        private async void SaveCommand(object value)
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            var name = value as string;
            if (ValidateNewCheckListName(name))
            {
                IsNewListFormVisible = false;
                if (CheckListName == name)
                {
                    IsLooked = false;
                    CancelCommand();
                    return;
                }
                if (!string.IsNullOrEmpty(CheckListName))
                {
                    ListOfCheckLists.Remove(lastCeckListSelected);
                    lastCeckListSelected.Name = name;
                    GlobalDataService.UpdateList(lastCeckListSelected);
                    ListOfCheckLists.Add(lastCeckListSelected);
                    lastCeckListSelected = null;
                    CheckListName = "";
                }
                else
                {
                    var checkListModel = new CheckListViewModel { LastId=0, Name=name, CheckListTasks = new List<CheckTaskModel>() };
                    GlobalDataService.AddList(name);
                    IsLooked = false;
                    SelectedCheckList = checkListModel; // Navigate to next page
                    return;
                }
            }
            else await ShowAlert("Faltan datos", ErrorMessage, "Ok");
            IsLooked = false;
        }

        private async void UpdateOrRemoveCommand(object value)
        {
            if (!HasLoaded || IsLooked || !(value is CheckListViewModel model)) return;
            IsLooked = true;
            if (model.SelectedReason == SelectedFor.Delete)
            {
                var resoult = await ShowAlert("Pregunta!", "¿Estás seguro de que quieres eliminar esta lista?", "Aceptar", "Cancelar");
                if (resoult)
                {
                    ListOfCheckLists.Remove(model);
                    GlobalDataService.RemoveList(model.Name);
                }
                else model.SelectedReason = SelectedFor.Create;
            }
            else if (model.SelectedReason == SelectedFor.Update)
            {
                IsNewListFormVisible = true;
                lastCeckListSelected = model;
                lastCeckListSelected.OldName = CheckListName = model.Name;
            }
            IsLooked = false;
        }

        private void OpenFormCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            IsNewListFormVisible = true;
            CheckListName = "";
            IsLooked = false;
        }

        private void CancelCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            IsNewListFormVisible = false;
            if (lastCeckListSelected != null)
                lastCeckListSelected.SelectedReason = SelectedFor.Create;
            lastCeckListSelected = null;
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (!HasLoaded || IsLooked) return;
            IsLooked = true;
            await CoreMethods.PushPageModel<OptionsPageModel>(0, animate: true);
        }

        #endregion

        #region Overrride Methods

        public override void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);
            InitializeComponet();
            base.Init(initData);
            IsLooked = !(HasLoaded = true);
        }

        public override void ReverseInit(object returndData)
        {
            IsLooked = !(HasLoaded = false);
            InitializeComponet();
            base.ReverseInit(returndData);
            IsLooked = !(HasLoaded = true);
        }

        protected override void ViewIsDisappearing(object sender, EventArgs e)
        {
            IsDisposing = true;
            if (ListOfCheckLists != null)
            {
                ListOfCheckLists.Clear();
                ListOfCheckLists = null;
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

        private void InitializeComponet()
        {
            var tempList = GlobalDataService.ListOfList;
            var viewModelList = tempList.Select(m => new CheckListViewModel
            {
                LastId = m.LastId,
                Name = m.Name,
                CheckListTasks = m.CheckListTasks
            });
            ListOfCheckLists = new FulyObservableCollection<CheckListViewModel>(viewModelList);

            //TODO: Implement business logic for licence: [free, premium].
            //var deviceHelper = DependencyService.Get<IDeviceHelper>();
            //And so on...

            PageTitle = "Grupo de listas";
        }

        private bool ValidateNewCheckListName(string name)
        {
            Errors.Clear();
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) Errors.Add("Debe ingresar el nobre de la nueba lista de tareas. ");
            if (ListOfCheckLists.Where(m => m.Name == name).FirstOrDefault() != null) Errors.Add("\nYa existe una lista con este nombre. ");
            if (!string.IsNullOrEmpty(name) && name.Length > 50) Errors.Add("\nEl nombre de la lista no puede ser mayor de 50 caractetes. ");
            if (!string.IsNullOrEmpty(name) && name == CheckListName) return true;
            return (Errors?.Count <= 0);

        }

        public Task RefreshUI() => Task.Run(() => Init(null));

        #endregion
    }
}
