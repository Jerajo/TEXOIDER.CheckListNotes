using System;
using System.Linq;
using Xamarin.Forms;
using PropertyChanged;
using System.Diagnostics;
using System.Windows.Input;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

        public bool? IsNewListFormVisible { get => IsEditing; set => IsEditing = value; }

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

        public string ExampleListName { get; private set; }

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
            var oldName = temporalList?.OldName;
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
                    if (!string.IsNullOrEmpty(oldName))
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
                        IsNewListFormVisible = false;
                        IsLooked = false;
                        // Navigate to check list page model
                        SelectedCheckList = new CheckListViewModel { Name = CheckListName }; 
                        return;
                    }
                }
                catch (Exception ex) { await ShowAlertError(ex.Message); }
            }
            else
            {
                //TODO: Implemnt validation error adaptive text control.
                await ShowAlertError(ErrorMessage);
            }
            IsLooked = false;
        }

        private async void UpdateOrRemoveCommand(object value)
        {
            if (HasLoaded == false || IsLooked == true || IsDisposing == true || 
                !(value is CheckListViewModel model)) return;
            IsLooked = true;
            if (model.SelectedReason == SelectedFor.Delete)
            {
                var language = AppResourcesLisener.Languages;
                var title = string.Format(language["AlertDeleteTitle"], model.Name);
                var message = language["ListOfListDeleteListMessage"];
                var resoult = await ShowAlertQuestion(title, message);
                if (resoult)
                {
                    ListOfCheckLists.Remove(model);
                    try { GlobalDataService.RemoveList(model.Name); }
                    catch (Exception ex) { await ShowAlertError(ex.Message); }
                    finally { IsLooked = false; }
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

                    var exampleListName = AppResourcesLisener.Languages["ExampleListName"];
                    if (ListOfCheckLists.Any(m => m.Name == exampleListName))
                    {
                        AppResourcesLisener.Current.PropertyChanged += OnLanguageChanged;
                        ExampleListName = exampleListName;
                    }

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
            var resourses = Application.Current.Resources;
            if (temporalList != null && temporalList.OldName == name) return true;
            if ((new Regex("[^A-Za-z0-9 ]")).IsMatch(name))
                Errors.Add(resourses["ListOfListErrorMessageInvalidCharacters"].ToString());
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                Errors.Add(resourses["ListOfListErrorMessageNullName"].ToString());
            if (ListOfCheckLists.Any(m => m.Name == name))
                Errors.Add(resourses["ListOfListErrorMessageListRepeated"].ToString());
            if (!string.IsNullOrEmpty(name) && name.Length > 50)
                Errors.Add(resourses["ListOfListErrorMessageNameTooLong"].ToString());
            return (Errors?.Count <= 0);
        }

        private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != AppResourcesLisener.Language) return;
            try
            {
                if (!string.IsNullOrEmpty(ExampleListName))
                {
                    GlobalDataService.RemoveList(ExampleListName);
                    (new TutorialListExample()).Create();
                }
            }
            catch (Exception) { }
        }

        #endregion

        #region Dispose

        ~ListOfCheckListsPageModel()
        {
            if (!string.IsNullOrEmpty(ExampleListName))
                AppResourcesLisener.Current.PropertyChanged -= OnLanguageChanged;
            ViewIsDisappearing(null, null);
#if DEBUG
            Debug.WriteLine("Object destroyed: [ Id: {1}, Name: {0} ].", this.GetHashCode(), nameof(ListOfCheckListsPageModel));
#endif
        }

        #endregion
    }
}
