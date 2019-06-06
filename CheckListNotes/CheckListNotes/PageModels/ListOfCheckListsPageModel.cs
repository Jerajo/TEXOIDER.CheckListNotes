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
    public class ListOfCheckListsPageModel : BasePageModel, IPageModel
    {
        private CheckListViewModel temporalList;

        public ListOfCheckListsPageModel() : base() { }

        #region SETTERS AND GETTERS

        #region Atributes

        public string CheckListName { get; set; }

        public bool IsNewListFormVisible { get; set; }

        public CheckListViewModel SelectedCheckList
        {
            get => null;
            set
            {
                if (value?.IsAnimating == false && IsLooked == false && IsDisposing == false)
                {
                    IsLooked = true;
                    GlobalDataService.CurrentListName = value.Name;
                    GlobalDataService.CurrentIndex = null;
                    PushPageModel<CheckListPageModel>(InitData);
                }
            }
        }

        public FulyObservableCollection<CheckListViewModel> ListOfCheckLists { get; set; }

        #endregion

        #region Commands

        public ICommand Save
        {
            get => new DelegateCommand(new Action(SaveCommand));
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

        private async void SaveCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            var oldName = temporalList.OldName;
            if (ValidateNewCheckListName())
            {
                try
                {
                    if (CheckListName == oldName)
                    {
                        IsLooked = false;
                        CancelCommand();
                        return;
                    }
                    if (!string.IsNullOrEmpty(CheckListName))
                    {
                        await GlobalDataService.UpdateList(oldName, CheckListName);
                        temporalList.OldName = temporalList.Name = CheckListName.Substring(0);
                        CheckListName = "";
                        IsLooked = false;
                        CancelCommand();
                        return;
                    }
                    else
                    {
                        await GlobalDataService.AddList(CheckListName);
                        IsLooked = false;
                        // Navigate to check list page model
                        SelectedCheckList = new CheckListViewModel { Name = CheckListName }; 
                        return;
                    }
                }
                catch (Exception ex)
                {
                    await ShowAlert("Fallo la operación!", ex.Message, "Ok");
                }
            }
            else await ShowAlert("Fallo la operación!", ErrorMessage, "Ok");
            IsLooked = false;
        }

        private async void UpdateOrRemoveCommand(object value)
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true || 
                !(value is CheckListViewModel model)) return;
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
                temporalList = model;
                CheckListName = model.Name.Substring(0);
            }
            IsLooked = false;
        }

        private void OpenFormCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            IsNewListFormVisible = true;
            CheckListName = "";
            IsLooked = false;
        }

        private void CancelCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            IsNewListFormVisible = false;
            if (temporalList != null)
                temporalList.SelectedReason = SelectedFor.Create;
            temporalList = null;
            IsLooked = false;
        }

        private async void GoToOptionsCommand()
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true) return;
            IsLooked = true;
            await PushPageModel<OptionsPageModel>(InitData);
        }

        #endregion

        #region Overrride Methods

        public override async void Init(object initData)
        {
            IsLooked = !(HasLoaded = false);
            await InitializeComponet(initData);
            base.Init(initData);
            IsLooked = !(HasLoaded = true);
        }

        public override async void ReverseInit(object returndData)
        {
            IsLooked = !(HasLoaded = false);
            await InitializeComponet(returndData);
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

        private Task InitializeComponet(object data)
        {
            return Task.Run(() => 
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    InitData = data ?? 0;

                    ListOfCheckLists = new FulyObservableCollection<CheckListViewModel>();
            
                    foreach (var item in GlobalDataService.GetAllLists())
                    {
                        ListOfCheckLists.Add(new CheckListViewModel
                        {
                            LastId = item.LastId,
                            Name = item.Name,
                            OldName = item.Name,
                            CompletedTasks = item.CheckListTasks.Where(m => 
                                m.IsChecked == true).Count(),
                            TotalTasks = item.CheckListTasks.Count()
                        });
                    }

                    Errors = new List<string>();

                    //TODO: Implement business logic for licence: [free, premium].
                    //var deviceHelper = DependencyService.Get<IDeviceHelper>();
                    //And so on...
                });
            });
        }

        private bool ValidateNewCheckListName()
        {
            Errors.Clear();
            var name = CheckListName;
            if (temporalList != null && temporalList.OldName == name) return true;
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name)) Errors.Add("Debe ingresar el nobre de la nueba lista de tareas. ");
            if (ListOfCheckLists.Where(m => m.Name == name).FirstOrDefault() != null) Errors.Add("\nYa existe una lista con este nombre. ");
            if (!string.IsNullOrEmpty(name) && name.Length > 50) Errors.Add("\nEl nombre de la lista no puede ser mayor de 50 caractetes. ");
            return (Errors?.Count <= 0);
        }

        #endregion
    }
}
