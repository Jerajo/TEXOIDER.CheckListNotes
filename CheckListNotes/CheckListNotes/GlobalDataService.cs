using System;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using CheckListNotes.Models;
using PortableClasses.Enums;
using System.Threading.Tasks;
using PortableClasses.Helpers;
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

        private static readonly string storageFolder = FileSystem.AppDataDirectory;
        private static bool hasLoaded = false;
        private static Random randomizer = new Random();

        #endregion

        public static void Init()
        {
            IsLoading = true;
            randomizer = new Random();
            if (IsFirtsInit()) CreateDefoultFiles();
            hasLoaded = !(IsLoading = false);
        }

        #region SETTERS AND GETTERS

        /// <summary>
        /// Indicate whether the data is loading or not.
        /// </summary>
        public static bool IsLoading { get; private set; }

        /// <summary>
        /// Storage the last requested list name.
        /// </summary>
        public static string CurrentListName { get; set; }

        /// <summary>
        /// Storage the previous requested list name.
        /// </summary>
        public static string PreviousListName { get; set; }

        /// <summary>
        /// Storage the last requested task id.
        /// </summary>
        public static string CurrentIndex { get; set; }

        /// <summary>
        /// Storage the previous requested task id.
        /// </summary>
        public static string PreviousIndex { get; set; }

        #endregion

        #region public Methods

        #region Get Methods

        // Get all list on LocalFolder/Data/fileName.bin
        public static IEnumerable<CheckListTasksModel> GetAllLists()
        {
            using (var cleaner = new ExplicitGarbageCollector())
            {
                if (!hasLoaded) ThrowNotInitializeExeption();
                foreach(var pathToFile in GetUserDataFileNames())
                    yield return Read<CheckListTasksModel>(pathToFile).Result;
            }
        }

        // Get current list by name
        public static Task<CheckListTasksModel> GetCurrentList(string listName = null)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                if (string.IsNullOrEmpty(CurrentListName))
                    CurrentListName = listName ?? throw new ArgumentNullException(nameof(listName));
                var pathToFile = $"{storageFolder}/Data/{CurrentListName}.bin";
                return Read<CheckListTasksModel>(pathToFile);
            }
        }

        // Get current task by id
        public static async Task<CheckTaskModel> GetCurrentTask(string taskId = null)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                string index = taskId ?? CurrentIndex; // Get correct index
                if (string.IsNullOrEmpty(index))
                    throw new ArgumentNullException(nameof(CurrentIndex));
                var model = await GetCurrentList();
                var value = model.CheckListTasks.Find(m => m.Id == index);
                return IndexNavigation(model.CheckListTasks, index);
            }
        }

        #endregion

        #region Add Methods

        // Add a new list on LocalFolder/Data/fileName.bin
        public static async Task AddList(string listName)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                var model = new CheckListTasksModel
                {
                    LastId = 0,
                    Name = listName,
                    CheckListTasks = new List<CheckTaskModel>()
                };
                CurrentListName = listName;
                await Write(model, $"{storageFolder}/Data/{listName}.bin");
            }
        }

        // Add a new task on the current list in LocalFolder/Data/fileName.bin
        public static async void AddTask(CheckTaskViewModel data)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                var model = await GetCurrentList();
                var task = new CheckTaskModel
                {
                    Id = string.IsNullOrEmpty(CurrentIndex) ? $"{++model.LastId}" : 
                        $"{CurrentIndex}.{++model.LastId}",
                    Name = data.Name,
                    NotifyOn = data.NotifyOn,
                    SubTasks = new List<CheckTaskModel>(),
                    ReminderTime = data.ReminderTime,
                    ExpirationDate = data.ExpirationDate,
                    CompletedDate = data.CompletedDate,
                    IsTaskGroup = data.IsTaskGroup,
                    IsChecked = data.IsChecked,
                    IsDaily = data.IsDaily
                };

                if (task.Id.Contains("."))
                {
                    var parentTask = IndexNavigation(model.CheckListTasks,
                        task.Id.RemoveLastSplit('.'));
                    parentTask.SubTasks.Add(task);
                    parentTask.IsChecked = !parentTask.SubTasks.Any(m => m.IsChecked == false);
                }
                else model.CheckListTasks.Add(task);
                await Write(model, $"{storageFolder}/Data/{model.Name}.bin");
            }
        }

        #endregion

        #region Update Methods

        // Update the list on LocalFolder/Data/fileName.bin
        public static async Task RenameList(string oldName, string newName)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                var model = await GetCurrentList();
                var oldPath = $"{storageFolder}/Data/{oldName}.bin";
                var newPath = $"{storageFolder}/Data/{newName}.bin";
                await Task.Run(() => File.Move(oldPath, newPath)).TryTo();
            }
        }

        // Update the task on the current list in LocalFolder/Data/fileName.bin
        public static async void UpdateTask(CheckTaskViewModel data)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                var index = data.Id.Contains(".") ? data.Id : CurrentIndex;
                var model = await GetCurrentList();

                CheckTaskModel task;
                if (index?.Contains(".") == true)
                {
                    var parentTask = IndexNavigation(model.CheckListTasks, 
                        index.RemoveLastSplit('.'));
                    task = parentTask.SubTasks.Find(m => m.Id == index);
                    task.IsChecked = data.IsChecked;
                    parentTask.IsChecked = !parentTask.SubTasks.Any(m => m.IsChecked == false);
                }
                else task = model.CheckListTasks.Find(m => m.Id == data.Id);

                task.Name = data.Name;
                task.ToastId = data.ToastId;
                task.NotifyOn = data.NotifyOn;
                task.ReminderTime = data.ReminderTime;
                task.ExpirationDate = data.ExpirationDate;
                task.CompletedDate = data.CompletedDate;
                task.IsTaskGroup = data.IsTaskGroup;
                task.IsChecked = data.IsChecked;
                task.IsDaily = data.IsDaily;
                await Write(model, $"{storageFolder}/Data/{model.Name}.bin");
            }
        }

        #endregion

        #region Remove Methods

        // Remove the list on LocalFolder/Data/fileName.bin
        public static async void RemoveList(string listName)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                await Task.Run(() => File.Delete($"{storageFolder}/Data/{listName}.bin"))
                    .TryTo();
            }
        }

        // Remove the task on the current list in LocalFolder/Data/fileName.bin
        public static async void RemoveTask(string taskId)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                var model = await GetCurrentList();

                if (taskId?.Contains(".") == true)
                {
                    var parentTask = IndexNavigation(model.CheckListTasks,
                        taskId.RemoveLastSplit('.'));
                    var task = parentTask.SubTasks.Find(m => m.Id == taskId);
                    parentTask.SubTasks.Remove(task);
                    parentTask.IsChecked = !parentTask.SubTasks.Any(m => m.IsChecked == false);
                }
                else
                {
                    var task = model.CheckListTasks.Find(m => m.Id == taskId);
                    model.CheckListTasks.Remove(task);
                }
                await Write(model, $"{storageFolder}/Data/{model.Name}.bin");
            }
        }

        #endregion

        #region Toasts

        public static Task RegisterToast(CheckTaskViewModel task, string listName = null)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                if (task.ReminderTime <= DateTime.Now) throw new Exception($"No puede programar un recordatorio para una fecha anterior a la actual: {DateTime.Now.ToString()}.");

                task.ToastId = $"{task.Id}-{randomizer.Next(1000000)}";
                var toasType = (ToastTypes)Config.Current.NotificationType;
                var listIndex = listName ?? CurrentListName;
                var arguments = $"listName={listIndex}&";
                arguments += $"toastId={task.ToastId}&";
                arguments += toasType == ToastTypes.Alarm ? ToastDataTemplate.AlarmToastArguments : ToastDataTemplate.NotificationToastArguments;

                var toast = new ToastModel
                {
                    Id = task.ToastId,
                    Title = "Recordatorio de tarea pendiente",
                    Body = task.Name,
                    Time = task.ReminderTime.Value,
                    Type = toasType,
                    Arguments = arguments + task.Id
                };

                if (Device.RuntimePlatform == Device.UWP) 
                    (new WindowsToastService(toast)).ProgramToast();
                else if (Device.RuntimePlatform == Device.Android)
                {
                    var id = $"{randomizer.Next(1000000)}";
                    CrossLocalNotifications.Current.Show(toast.Title, toast.Body, int.Parse(id), toast.Time);
                }
                return Task.CompletedTask;
            }
        }

        public static Task UnregisterToast(string toasId)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                if (Device.RuntimePlatform == Device.UWP)
                    (new WindowsToastService()).CancelProgramedToast(toasId);
                else if (Device.RuntimePlatform == Device.Android)
                    CrossLocalNotifications.Current.Cancel(int.Parse(toasId));
                return Task.CompletedTask;
            }
        }

        #endregion

        #region Background Tasks

        public static void RegisterBackgroundTask(IBackgroundTask task)
        {
            using (var cleaner = new ExplicitGarbageCollector())
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
        }

        public static void UnregisterBackgroundTask(string taskName)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (!hasLoaded) ThrowNotInitializeExeption();
                var deviceHerper = DependencyService.Get<IDeviceHelper>();
                deviceHerper.UnregisterBackgroundTask(taskName);
            }
        }

        #endregion

        #endregion

        #region Private Methos

        #region Navigation

        public static CheckTaskModel IndexNavigation(List<CheckTaskModel> list, string uri, int deep = 1)
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                if (uri.Contains(".") && deep < uri.Split('.').Length)
                {
                    var taskId = uri.GetSplitRange(".", endValue: deep);
                    var model = list.FirstOrDefault(m => m.Id == taskId);
                    return IndexNavigation(model.SubTasks,
                        uri, ++deep);
                }
                else return list.FirstOrDefault(m => m.Id == uri);
            }
        }

        #endregion

        #region Read

        private static List<string> GetUserDataFileNames()
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                return Directory.GetFiles($"{storageFolder}/Data/", "*.bin").ToList();
            }
        }
            

        private static Task<T> Read<T>(string pathToFile)
        {
            using (var fileService = new FileService())
            {
                return Task.Run(() => fileService.Read<T>(pathToFile)).TryTo();
            }
        }

        private static Task<object> Read(string pathToFile)
        {
            using (var fileService = new FileService())
            {
                return Task.Run(() => fileService.Read(pathToFile)).TryTo();
            }
        }

        #endregion

        #region Write

        private static Task Write(object data, string pathToFile)
        {
            using (var fileService = new FileService())
            {
                var document = (data is string value) ? JToken.Parse(value) : 
                    JToken.FromObject(data);
                return Task.Run(() => { 
                    fileService.Write(document, pathToFile);
                }).TryTo();
            }
        }

        #endregion

        #region Utility

        private static void CreateDefoultFiles()
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                var initFile = new InitFile { LastResetTime = DateTime.Now };
                Write(initFile, $"{storageFolder}/init.bin").Wait();

                var IdCount = 0;
                var model = new CheckListTasksModel
                {
                    Name = "Lista de ejemplo",
                    CheckListTasks = new List<CheckTaskModel>
                    {
                        new CheckTaskModel
                        {
                            Id=((IdCount++).ToString()).ToString(), Name="Tarea atrasada", ExpirationDate=DateTime.Now.AddHours(-1),
                            CompletedDate=null, IsChecked=false, IsDaily=false
                        },
                        new CheckTaskModel
                        {
                            Id=((IdCount++).ToString()).ToString(), Name="Tarea urgente", ExpirationDate=DateTime.Now.AddHours(1), CompletedDate=null, IsChecked=false, IsDaily=false
                        },
                        new CheckTaskModel
                        {
                            Id=((IdCount++).ToString()).ToString(), Name="Tarea diaria", ExpirationDate=DateTime.Now.AddHours(5), CompletedDate=null, IsChecked=false, IsDaily=true
                        },
                        new CheckTaskModel
                        {
                            Id=((IdCount++).ToString()).ToString(), Name="Tarea sin expiracíon", ExpirationDate=null, CompletedDate=null, IsChecked=false, IsDaily=false
                        },
                        new CheckTaskModel
                        {
                            Id=(IdCount++).ToString(), Name="Tarea completada a tiempo", ExpirationDate=DateTime.Now.AddHours(2), CompletedDate=DateTime.Now, IsChecked=true, IsDaily=false
                        },
                        new CheckTaskModel
                        {
                            Id=(IdCount).ToString(), Name="Tarea completada atrasada", ExpirationDate=DateTime.Now.AddHours(-1), CompletedDate=DateTime.Now, IsChecked=true, IsDaily=false
                        }
                    },
                    LastId = IdCount
                };

                Directory.CreateDirectory($"{storageFolder}/Data");
                Write(model, $"{storageFolder}/Data/{model.Name}.bin");
            }
        }

        //TODO: Implement storage folder for android
        private static bool IsFirtsInit()
        {
            using (var cleaner = new ExplicitGarbageCollector())
            {
                return (Directory.GetDirectories(storageFolder)?.Length <= 0);
            }
        }
            

        // Throw a not initialized exception if the method Init() hasn't been call
        private static void ThrowNotInitializeExeption()
        {
            using (var cleaner = new ExplicitGarbageCollector())
            { 
                throw new TypeInitializationException(
                    nameof(GlobalDataService),
                    new Exception("Error before using this service it needs to be initialize by calling the Init() method on the Main App class."));
            }
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

#region Lists

/* /-------------------------/ Get /-------------------------/
 *  2.1) Get all lists.
 *  var allList = GlobalDataService.GetAllLists();
 *  
 *  2.2) Get a list.
 *  var listName = "List Example"
 *  // Needs to be call at least ones per list
 *  var list = GlobalDataService.GetCurrentList(listName);
 *  
 *  2.3) Get the current list.
 *  GlobalDataService.CurrentListName = "List Example";
 *  //Get the current list if CurrentListName isn't null and the list exist 
 *  //otherwise throw an Exeption
 *  var list = GlobalDataService.GetCurrentList();
 *  
 *  2.4) Get the previous list.
 *  var listName = GlobalDataService.PreviousListName;
 *  //Get the current list if PreviousListName isn't null and the list exist 
 *  //otherwise throw an Exeption
 *  var list = GlobalDataService.GetCurrentList(listName);
 */

/*  /-------------------------/ Add /-------------------------/
 *  2.3) Add a list 
 *  //NOTE: The name can't be null or repeated.
 *  var listName = "List Example 2";
 *  GlobalDataService.AddList(listName);
 */

/*  /-------------------------/ Rename /-------------------------/
 *  2.4) Rename a list.
 *  var model = CheckListModel;
 *  //Note: The old name has to be a existing file on app storage.
 *  GlobalDataService.RenameList(model.OldName, model.Name);
 */

/*  /-------------------------/ Remove /-------------------------/
 *  2.5) Remove a list
 *  var listName = "List Example 2";
 *  //NOTE: The name can't be null and the file need to exists in app storage.
 *  GlobalDataService.RemoveList(listName);
 */

#endregion

#region Tasks

/* /-------------------------/ Get /-------------------------/
 *  3.1) Get a task by a string indexer. "1.23.5.26..."
 *  var taskIndex = $"{GlobalDataService.CurrentIndex}.{Task.Id}";
 *  var task = GlobalDataService.GetCurrentTask(taskIndex);
 *  
 *  3.2) Get the current thask.
 *  //Get the current task if GlobalDataService.CurrentIndex isn't null 
 *  //otherwise throw an Exeption
 *  var task = GlobalDataService.GetCurrentTask();
 *  
 *  3.3) Get the previous taks by previuos index.
 *  var taskIndex = GlobalDataService.PreviousIndex;
 *  var task = GlobalDataService.GetCurrentTask(taskIndex);
 *  
 *  3.4) Get the parent taks by RemoveLastSplit method.
 *  var parentTask = GlobalDataService.CurrentIndex.RemoveLastSplit('.');
 *  var task = GlobalDataService.GetCurrentTask(parentTask);
 */

/* /-------------------------/ Add /-------------------------/
 * 3.5) Add a task 
 *  var task = new CheckTaskVieModel
 *  {
 *      Id = Task.Id,
 *      Name = Task.Name,
 *      // ...
 *  };
 *  GlobalDataService.AddTask(task);
 */

/*  /-------------------------/ Update /-------------------------/
 *  3.6) Update a task.
 *  var Task = GlobalDataService.GetCurrentTask();
 *  var task = new CheckTaskVieModel
 *  {
 *      Id = Task.Id,
 *      Name = Task.Name,
 *      // ...
 *  };
 *  GlobalDataService.UpdateTask(model);
 */

/*  /-------------------------/ Remove /-------------------------/
 *  3.7) Remove a task by string indexer. "1.23.5.26..."
 *  var taskIndex = $"{GlobalDataService.CurrentIndex}.{Task.Id}";
 *  Note: If task do not exist will throw a resource not found exection.
 *  GlobalDataService.RemoveTask(taskIndex);
 */

#endregion

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




