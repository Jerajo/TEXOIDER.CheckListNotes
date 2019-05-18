using System;
using System.IO;
using Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Essentials;
using CheckListNotes.Models;
using PortableClasses.Enums;
using PortableClasses.Extensions;
using System.Collections.Generic;
using PortableClasses.Interfaces;
using CheckListNotes.Models.Interfaces;

namespace CheckListNotes
{
    public static class GlobalDataService
    {
        #region Atributes

        private static bool hasLoaded = false;
        private static bool hasChanges = false;

        #endregion

        public static void Init()
        {
            IsLoading = true;
            
            Device.BeginInvokeOnMainThread(async () => // Load Theme
            {
                using (var stream = await FileSystem.OpenAppPackageFileAsync($"{Config.Current.Theme}.json"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var fileContents = await reader.ReadToEndAsync();
                        Config.Current.AppTheme = JsonConvert.DeserializeObject<AppTheme>(fileContents);
                    }
                }
            });

            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            if (IsFirtsInit()) CreateDefoultFiles();

            ListOfList = deviceHerper.GetAllLists(deviceHerper.GetAllLists());
            hasLoaded = !(IsLoading = false);
        }

        #region SETTERS AND GETTERS

        public static List<CheckListTasksModel> ListOfList { get; private set; }

        public static CheckListTasksModel CurrentList { get; private set; }

        public static CheckTaskModel CurrentTask { get; private set; }

        public static bool IsLoading { get; private set; }

        public static string LastCheckListName { get; private set; }

        public static int LastCurrentTaskId { get; private set; }

        public static bool HasChanges
        {
            get => hasChanges;
            private set
            {
                if (value) SaveCahnges();
                hasChanges = value;
            }
        }

        #endregion

        #region public Methods

        #region Get Methods

        // Get all list on LocalFolder/Data/fileName.bin
        public static List<CheckListTasksModel> GetAllLists()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            return ListOfList = deviceHerper.GetAllLists(deviceHerper.GetAllLists());
        }

        // Get current list by name
        public static CheckListTasksModel GetCurrentList(string listName)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            LastCheckListName = listName;
            return CurrentList = ListOfList.Find(m => m.Name == listName);
        }

        // Get current task by id
        public static CheckTaskModel GetCurrentTask(int taskId)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            LastCurrentTaskId = taskId;
            return CurrentTask = CurrentList.CheckListTasks.Find(m => m.Id == taskId);
        }

        #endregion

        #region Add Methods

        // Add a new list on LocalFolder/Data/fileName.bin
        public static void AddList(string listName)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var model = new CheckListTasksModel
            {
                LastId = 0,
                Name = listName,
                CheckListTasks = new List<CheckTaskModel>()
            };
            ListOfList.Add(model);
            CurrentList = model;
            HasChanges = true;
        }

        // Add a new task on the current list in LocalFolder/Data/fileName.bin
        public static void AddTask(CheckTaskVieModel data)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var model = new CheckTaskModel
            {
                Id = data.Id,
                Name = data.Name,
                NotifyOn = data.NotifyOn,
                ReminderTime = data.ReminderTime,
                ExpirationDate = data.ExpirationDate,
                CompletedDate = data.CompletedDate,
                IsChecked = data.IsChecked,
                IsDaily = data.IsDaily
            };
            CurrentList.CheckListTasks.Add(model);
            CurrentList.LastId = model.Id;
            HasChanges = true;
        }

        #endregion

        #region Update Methods

        // Update the list on LocalFolder/Data/fileName.bin
        public static void UpdateList(CheckListViewModel list)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            //TODO: Inplement IIdentity on CeckListTasksModel or Using IdentityFramework
            //ListOfList.Update(model);
            var model = ListOfList.Find(m => m.Name == list.OldName);
            var index = ListOfList.IndexOf(model);

            if (!string.IsNullOrEmpty(list.OldName) && list.NameHasChange) RenameList(list);
            model.LastId = list.LastId;
            model.Name = list.Name;
            model.CheckListTasks = list.CheckListTasks;
            ListOfList[index] = model;
            CurrentList = model;
            HasChanges = true;
        }

        // Update the task on the current list in LocalFolder/Data/fileName.bin
        public static void UpdateTask(CheckTaskVieModel task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            CurrentTask.Id = task.Id;
            CurrentTask.Name = task.Name;
            CurrentTask.NotifyOn = task.NotifyOn;
            CurrentTask.ReminderTime = task.ReminderTime;
            CurrentTask.ExpirationDate = task.ExpirationDate;
            CurrentTask.CompletedDate = task.CompletedDate;
            CurrentTask.IsChecked = task.IsChecked;
            CurrentTask.IsDaily = task.IsDaily;
            CurrentList.CheckListTasks.Update(CurrentTask);
            HasChanges = true;
        }

        #endregion

        #region Remove Methods

        // Remove the list on LocalFolder/Data/fileName.bin
        public static void RemoveList(string listName)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var list = ListOfList.Find(m => m.Name == listName);
            ListOfList.Remove(list);
            Remove(listName);
        }

        // Remove the task on the current list in LocalFolder/Data/fileName.bin
        public static void RemoveTask(CheckTaskVieModel task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var model = CurrentList.CheckListTasks.Find(m => m.Id == task.Id);
            CurrentList.CheckListTasks.Remove(model);
            HasChanges = true;
        }

        // Remove the current list on LocalFolder/Data/fileName.bin
        public static void RemoveCurrentList()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            ListOfList.Remove(CurrentList);
            Remove(CurrentList.Name);
            CurrentList.CheckListTasks.Clear();
            CurrentList = null;
        }

        // Remove the current task on the current list in LocalFolder/Data/fileName.bin
        public static void RemoveCurrentTask()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            CurrentList.CheckListTasks.Remove(CurrentTask);
            CurrentTask = null;
            HasChanges = true;
        }

        #endregion

        #region Background Tasks

        public static string RegisterToast(IToast toast, ToastTypes type)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            if (type == ToastTypes.Alarm) return deviceHerper.RegisterAlarm(toast); 
            else return deviceHerper.RegisterNotification(toast);
        }

        public static void RegisterBackgroundTask(IBackgroundTask task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.RegisterBackgroundTask(task);
        }

        public static void UnregisterToast(string toasId)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.UnregisterToast(toasId);
        }

        public static void UnregisterBackgroundTask(string taskName)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.UnregisterBackgroundTask(taskName);
        }

        #endregion

        #endregion

        #region Private Methos

        // Save the current list on LocalFolder/Data/fileName.bin
        private static void SaveCahnges()
        {
            if (CurrentList == null) throw new NullReferenceException($"Error can't save the atribute: {nameof(CurrentList)} because his value is null.");
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.Write(CurrentList);
            HasChanges = false;
        }

        // Remove the current list on LocalFolder/Data/fileName.bin
        private static void Remove(string listName)
        {
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.Delete(listName);
        }

        // Rename the current list on LocalFolder/Data/fileName.bin
        private static void RenameList(CheckListViewModel list)
        {
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.Rename(list.OldName, list.Name);
        }

        private static void CreateDefoultFiles()
        {
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            var localFolder = FileSystem.AppDataDirectory;

            var initFileName = new { initFileName = "Lista de ejemplo" };
            deviceHerper.Write(initFileName, "/init.bin");

            var mainDir = Directory.CreateDirectory("/Data");
            
            var IdCount = 1;
            var model = new CheckListTasksModel
            {
                Name = "Lista de ejemplo",
                CheckListTasks = new List<CheckTaskModel>
                {
                    new CheckTaskModel
                    {
                        Id=IdCount++, Name="Tarea atrasada", ExpirationDate=DateTime.Now.AddHours(-1),
                        CompletedDate=null, IsChecked=false, IsDaily=false
                    },
                    new CheckTaskModel
                    {
                        Id=IdCount++, Name="Tarea urgente", ExpirationDate=DateTime.Now.AddHours(1), CompletedDate=null, IsChecked=false, IsDaily=false
                    },
                    new CheckTaskModel
                    {
                        Id=IdCount++, Name="Tarea diaria", ExpirationDate=DateTime.Now.AddHours(5), CompletedDate=null, IsChecked=false, IsDaily=true
                    },
                    new CheckTaskModel
                    {
                        Id=IdCount++, Name="Tarea sin expiracíon", ExpirationDate=null, CompletedDate=null, IsChecked=false, IsDaily=false
                    },
                    new CheckTaskModel
                    {
                        Id=IdCount++, Name="Tarea completada a tiempo", ExpirationDate=DateTime.Now.AddHours(2), CompletedDate=DateTime.Now, IsChecked=true, IsDaily=false
                    },
                    new CheckTaskModel
                    {
                        Id=IdCount, Name="Tarea completada atrasada", ExpirationDate=DateTime.Now.AddHours(-1), CompletedDate=DateTime.Now, IsChecked=true, IsDaily=false
                    }
                },
                LastId = IdCount
            };

            deviceHerper.Write(model);
        }

        //TODO: Implement validation for android
        private static bool IsFirtsInit()
        {
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            var dir = Directory.GetDirectories(FileSystem.AppDataDirectory);
            return (dir?.Length <= 0);
        }

        // Throw a not initialized exception if the method Init() hasn't been call
        private static void ThrowNotInitializeExeption()
        {
            throw new TypeInitializationException(
                nameof(GlobalDataService), 
                new Exception("Error before using this service it needs to be initialize by calling the Init() method on the Main App class."));
        }

        #endregion
    }
}

#region Implementation

/* /-------------------------/ Initialize /-----------------------------/
 * //Initialize the class on the App Main class
 * GlobalDataService.Init();
 */

/* /-------------------------/ Lists /-------------------------/
 *  2.1) Get a list.
 *  var listName = "List Example"
 *  // Needs to be call at least ones per list
 *  var list = GlobalDataService.GetList(listName);
 *  
 *  2.2) Get the current list.
 *  //Get the current list if isn't null otherwise throw an Exeption
 *  var list = GlobalDataService.CurrentList;
 *  
 *  2.3) Add a list 
 *  //NOTE: The name can't be null or repeated.
 *  var listName = "List Example 2";
 *  GlobalDataService.AddList(listName);
 *  
 *  2.4) Update a list.
 *  var list = GlobalDataService.CurrentList;
 *  var model = new CheckListViewModel
 *  {
 *      //Id = m.Id TODO: Implement entity Framewor
 *      LastId = m.LastId,
 *      Name = m.Name,
 *      CheckListTasks = m.CheckListTasks
 *  };
 *  GlobalDataService.UpdateList(model);
 *  
 *  2.5) Remove a list
 *  var list = GlobalDataService.CurrentList;
 *  GlobalDataService.RemoveList(list);
 *  
 *  2.6) Remove the current list
 *  //Remove the current list if isn't null otherwise throw an Exeption
 *  GlobalDataService.RemoveCurrentList();
 */

/* /-------------------------/ Tasks /-------------------------/
*  3.1) Get a task.
*  var taskName = "List Example"
*  // Needs to be call at least ones per task
*  var task = GlobalDataService.GetTask(taskName);
*  
*  3.2) Get the current thask.
*  //Get the current task if isn't null otherwise throw an Exeption
*  var taskCopy = GlobalDataService.CurrentTask;
*  
*  3.3) Update a task.
*  var task = GlobalDataService.CurrentTask;
*  var model = new CheckTaskVieModel
*  {
*      Id = Task.Id,
*      Name = Task.Name,
*      ExpirationDate = Task.ExpirationDate,
*      CompletedDate = Task.CompletedDate,
*      IsChecked = Task.IsChecked,
*      IsDaily = Task.IsDaily
*  };
*  GlobalDataService.UpdateTask(model);
*  
*  3.4) Add a task 
*  var task = new CheckTaskVieModel
*  {
*      Id = Task.Id,
*      Name = Task.Name,
*      ExpirationDate = Task.ExpirationDate,
*      CompletedDate = Task.CompletedDate,
*      IsChecked = Task.IsChecked,
*      IsDaily = Task.IsDaily
*  };
*  GlobalDataService.AddTask(task);
*  
*  3.5) Remove a task
*  var task = GlobalDataService.CurrentTask;
*  GlobalDataService.RemoveTask(task);
*  
*  3.6) Remove the current task
*  //Remove the current task if isn't null otherwise throw an Exeption
*  GlobalDataService.RemoveCurrentTask();*  
*/

/* /-------------------------/ Background Tasks /-----------------------------/
 * 4.1) Register a toast
 * var task = new CheckTaskVieModel
*  {
*      Id = Task.Id,
*      Name = Task.Name,
*      ExpirationDate = Task.ExpirationDate,
*      CompletedDate = Task.CompletedDate,
*      IsChecked = Task.IsChecked,
*      IsDaily = Task.IsDaily
*  };
*  var toast = new IToast 
*  {
*      Id = Task.Id,
*      Title = Resources.Language.ToastInformationTitle,
*      Body = Task.Name.
*      ButtonDetails = Resources.Language.ToastButtonDetalis,
*      ButtonAcept = Resources.Language.ButtonAcept,
*      ButtonCancel = Resources.Language.ButtonCancel
*  };
 * task.ToastId = GlobalDataService.RegisterToast(IToast toast, ToastType.Information);
*  GlobalDataService.AddTask(task);
*  
*  4.2) Unregister a toast
*  var task = GlobalDataService.CurrentTask;
*  GlobalDataService.UnregisterToast(task.ToastId);
*  
*  4.3) Register a background task
*  var task = GlobalDataService.CurrentTask;
*  var model = new CheckTaskVieModel
*  {
*      Id = Task.Id,
*      Name = Task.Name,
*      ExpirationDate = Task.ExpirationDate,
*      CompletedDate = Task.CompletedDate,
*      IsChecked = Task.IsChecked,
*      IsDaily = Task.IsDaily
*  };
*  var toast = new IToast 
*  {
*      Id = Task.Id,
*      Title = Resources.Language.ToastInformationTitle,
*      Body = Task.Name.
*      ButtonDetails = Resources.Language.ToastButtonDetalis,
*      ButtonAcept = Resources.Language.ButtonAcept,
*      ButtonCancel = Resources.Language.ButtonCancel
*  };
*  GlobalDataService.UnregisterToast(model.ToastId);
*  model.ToastId = GlobalDataService.RegisterToast(IToast toast, ToastType.Information);
*  
*  var task = new IBackgroundTask
*  {
*      Title = model.Name.Extrat(0, 10) + DateTime.Now.ToString(),
*  };
*  GlobalDataService.UnregisterBackgroundTask(model.Name);
*  GlobalDataService.RegisterBackgroundTask(task);
*  
*  GlobalDataService.UpdateTask(model);
*  
*  4.4) Unregister a background task
*  var task = GlobalDataService.CurrentTask;
*  GlobalDataService.UnregisterBackgroundTask(task.Name);
 */

#endregion




