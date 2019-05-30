using System;
using System.IO;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using PortableClasses.Services;
using Plugin.LocalNotifications;
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
        private static readonly string storageFolder = FileSystem.AppDataDirectory;
        private static Random randomizer = new Random();

        #endregion

        public static void Init()
        {
            IsLoading = true;

            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            if (IsFirtsInit()) CreateDefoultFiles();
            //TODO: reduce ran consumtion
            ListOfList = new List<CheckListTasksModel>(GetAllLists(GetUserDataFileNames()));
            CheckTaskModelStack = new Stack<CheckTaskModel>();

            hasLoaded = !(IsLoading = false);
        }

        #region SETTERS AND GETTERS

        /// <summary>
        /// Indicate whether the data is loading or not.
        /// </summary>
        public static bool IsLoading { get; private set; }

        /// <summary>
        /// Storage basic information of all user lists.
        /// </summary>
        public static List<CheckListTasksModel> ListOfList { get; private set; }

        /// <summary>
        /// Storage the list of sub-task inside a list.
        /// </summary>
        public static Stack<CheckTaskModel> CheckTaskModelStack { get; private set; }

        /// <summary>
        /// Storage the last requested list.
        /// </summary>
        public static CheckListTasksModel CurrentList { get; private set; }

        /// <summary>
        /// Storage the last requested task.
        /// </summary>
        public static CheckTaskModel CurrentTask { get; private set; }

        /// <summary>
        /// Storage the last requested list name.
        /// </summary>
        public static string LastCheckListName { get; private set; }

        /// <summary>
        /// Storage the last requested task id.
        /// </summary>
        public static int LastCurrentTaskId { get; private set; }

        /// <summary>
        /// Indicate whether the data has changes or not. Set to true to save current changes.
        /// </summary>
        public static bool HasChanges
        {
            get => hasChanges;
            set
            {
                hasChanges = value;
                if (value) SaveCahnges();
            }
        }

        #endregion

        #region public Methods

        #region Get Methods

        // Get all list on LocalFolder/Data/fileName.bin
        public static IEnumerable<CheckListTasksModel> GetAllLists()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            return ListOfList = new List<CheckListTasksModel>(GetAllLists(GetUserDataFileNames()));
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
            if (CheckTaskModelStack.Count > 0)
                return CurrentTask = CheckTaskModelStack.Peek().SubTasks.Find(m => m.Id == taskId);
            else return CurrentTask = CurrentList.CheckListTasks.Find(m => m.Id == taskId);
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
        public static void AddTask(CheckTaskViewModel data)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var model = new CheckTaskModel
            {
                Id = data.Id,
                Name = data.Name,
                NotifyOn = data.NotifyOn,
                SubTasks = data.SubTasks,
                ReminderTime = data.ReminderTime,
                ExpirationDate = data.ExpirationDate,
                CompletedDate = data.CompletedDate,
                IsTaskGroup = data.IsTaskGroup,
                IsChecked = data.IsChecked,
                IsDaily = data.IsDaily
            };

            if (CheckTaskModelStack.Count > 0)
            {
                CheckTaskModelStack.Peek().SubTasks.Add(model);
                CheckTaskModelStack.Peek().LastSubId = model.Id;
            }
            else
            {
                CurrentList.CheckListTasks.Add(model);
                CurrentList.LastId = model.Id;
            }
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

            if (!string.IsNullOrEmpty(list.OldName) && list.NameHasChange)
            {
                var oldPath = $"{storageFolder}/Data/{list.OldName}.bin";
                var newPath = $"{storageFolder}/Data/{list.Name}.bin";
                File.Move(oldPath, newPath);
            }

            model.LastId = list.LastId;
            model.Name = list.Name;
            model.CheckListTasks = list.CheckListTasks;
            ListOfList[index] = model;
            CurrentList = model;
            HasChanges = true;
        }

        // Update the task on the current list in LocalFolder/Data/fileName.bin
        public static void UpdateTask(CheckTaskViewModel task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            CurrentTask.Id = task.Id;
            CurrentTask.Name = task.Name;
            CurrentTask.ToastId = task.ToastId;
            CurrentTask.NotifyOn = task.NotifyOn;
            CurrentTask.SubTasks = task.SubTasks;
            CurrentTask.ReminderTime = task.ReminderTime;
            CurrentTask.ExpirationDate = task.ExpirationDate;
            CurrentTask.CompletedDate = task.CompletedDate;
            CurrentTask.IsTaskGroup = task.IsTaskGroup;
            CurrentTask.IsChecked = task.IsChecked;
            CurrentTask.IsDaily = task.IsDaily;
            if (CheckTaskModelStack.Count > 0)
                CheckTaskModelStack.Peek().SubTasks.Update(CurrentTask);
            else CurrentList.CheckListTasks.Update(CurrentTask);
            HasChanges = true;
        }

        public static void UpdateCurrentList()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            HasChanges = true;
        }

        public static void UpdateCurrentTask()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
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
            File.Delete($"{storageFolder}/{listName}.bin");
        }

        // Remove the task on the current list in LocalFolder/Data/fileName.bin
        public static void RemoveTask(CheckTaskViewModel task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            if (CheckTaskModelStack.Count > 0)
            {
                var model = CheckTaskModelStack.Peek().SubTasks.Find(m => m.Id == task.Id);
                CheckTaskModelStack.Peek().SubTasks.Remove(model);
            }
            else
            {
                var model = CurrentList.CheckListTasks.Find(m => m.Id == task.Id);
                CurrentList.CheckListTasks.Remove(model);
            }
            HasChanges = true;
        }

        // Remove the current list on LocalFolder/Data/fileName.bin
        public static void RemoveCurrentList()
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            ListOfList.Remove(CurrentList);
            File.Delete($"{storageFolder}/{CurrentList.Name}.bin");
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

        public static void RegisterToast(CheckTaskViewModel task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            if (task.ReminderTime <= DateTime.Now) throw new Exception($"No puede programar un recordatorio para una fecha anterior a la actual: {DateTime.Now.ToString()}.");

            var toast = new ToastModel
            {
                Id = task.ToastId = $"{task.Id}-{randomizer.Next(1000000)}",
                Title = "Recordatorio de tarea pendiente",
                Body = task.Name,
                Time = task.ReminderTime.Value,
                Type = (ToastTypes)Config.Current.NotificationType
            };

            if (Device.RuntimePlatform == Device.UWP) 
                (new WindowsToastService(toast)).ProgramToast();
            else if (Device.RuntimePlatform == Device.Android)
            {
                var id = $"{randomizer.Next(1000000)}";
                CrossLocalNotifications.Current.Show(toast.Title, toast.Body, int.Parse(id), toast.Time);
            }
        }

        public static void RegisterBackgroundTask(IBackgroundTask task)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            if (Device.RuntimePlatform == Device.UWP)
            {
                var deviceHerper = DependencyService.Get<IDeviceHelper>();
                deviceHerper.RegisterBackgroundTask(task);
            }
            else if (Device.RuntimePlatform == Device.Android)
            {

            }
        }

        public static void UnregisterToast(string toasId)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            if (Device.RuntimePlatform == Device.UWP)
                (new WindowsToastService()).CancelProgramedToast(toasId);
            else if (Device.RuntimePlatform == Device.Android)
                CrossLocalNotifications.Current.Cancel(int.Parse(toasId));
        }

        public static void UnregisterBackgroundTask(string taskName)
        {
            if (!hasLoaded) ThrowNotInitializeExeption();
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            deviceHerper.UnregisterBackgroundTask(taskName);
        }

        #endregion

        #region Stack Methods

        public static void Push(CheckTaskViewModel data)
        {
            CheckTaskModel model;
            if (CheckTaskModelStack.Count == 0) model = CurrentList.CheckListTasks.Find(m => m.Id == data.Id);
            else model = CheckTaskModelStack.Peek().SubTasks.Find(m => m.Id == data.Id);
            CheckTaskModelStack.Push(model);
        }

        public static CheckTaskModel Pop()
        {
            return CheckTaskModelStack.Pop();
        }

        #endregion

        #endregion

        #region Private Methos

        #region Read

        private static string[] GetUserDataFileNames()
        {
            return Directory.GetFiles($"{storageFolder}/Data/", "*.bin");
        }

        private static IEnumerable<CheckListTasksModel> GetAllLists(string[] pathToFiles)
        {
            if (pathToFiles.Length <= 0) yield return default;
            foreach(var filePaht in pathToFiles)
                yield return Read<CheckListTasksModel>(filePaht).Result;
        }

        private static Task<T> Read<T>(string pathToFile)
        {
            using(var fileService = new FileService())
            {
                return Task.Run(() => 
                {
                    return fileService.Read<T>(pathToFile);
                }).TryTo();
            }
        }

        #endregion

        #region Write

        // Save the current list on LocalFolder/Data/fileName.bin
        public static void SaveCahnges()
        {
            if (CurrentList == null) throw new NullReferenceException($"Error can't save the atribute: {nameof(CurrentList)} because his value is null.");
            Write(CurrentList, $"{storageFolder}/Data/{CurrentList.Name}.bin").Wait();
            hasChanges = false;
        }

        private static Task Write(object data, string pathToFile)
        {
            using(var fileService = new FileService())
            {
                return Task.Run(() => { 
                    var document = JToken.FromObject(data);
                    fileService.Write(document, pathToFile);
                }).TryTo();
            }
        }

        #endregion

        #region Utility

        private static void CreateDefoultFiles()
        {
            var deviceHerper = DependencyService.Get<IDeviceHelper>();
            var localFolder = FileSystem.AppDataDirectory;

            var initFile = new InitFile { LastResetTime = DateTime.Now };

            Write(initFile, $"{storageFolder}/init.bin").Wait();

            var mainDir = Directory.CreateDirectory($"{localFolder}/Data");

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

            Write(model, $"{storageFolder}/{model.Name}.bin");
        }

        //TODO: Implement storage folder for android
        private static bool IsFirtsInit() =>
            (Directory.GetDirectories(storageFolder)?.Length <= 0);

        // Throw a not initialized exception if the method Init() hasn't been call
        private static void ThrowNotInitializeExeption()
        {
            throw new TypeInitializationException(
                nameof(GlobalDataService),
                new Exception("Error before using this service it needs to be initialize by calling the Init() method on the Main App class."));
        }

        #endregion

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




