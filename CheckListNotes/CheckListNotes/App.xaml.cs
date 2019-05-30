using System;
using FreshMvvm;
using System.IO;
using Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using CheckListNotes.Models;
using CheckListNotes.PageModels;
using CheckListNotes.Models.Interfaces;
using Windows.ApplicationModel.Background;
using PortableClasses.Services;
using System.Threading.Tasks;
using PortableClasses.Extensions;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CheckListNotes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // config.AppSettings.Clear();
            LoadTheme(); 
            GlobalDataService.Init();

            var page = FreshPageModelResolver.ResolvePageModel<ListOfCheckListsPageModel>();
            NavigationContainer = new FreshNavigationContainer(page);
            MainPage = NavigationContainer;
        }

        #region Getters And Setters

        public FreshNavigationContainer NavigationContainer { get; set; }

        private Random Randomizer { get; set; } = new Random();

        #endregion

        #region Events

        #region Device Events

        /// <summary>
        /// Handle when your app starts
        /// </summary>
        protected override void OnStart()
        {
            ConfigPlatform();
            LoadState();
        }

        /// <summary>
        /// Handle when your app sleeps
        /// </summary>
        protected override void OnSleep() => SaveState();

        /// <summary>
        /// Handle when your app resumes
        /// </summary>
        protected override void OnResume() => LoadState();

        #endregion

        #region User Events

        private void OnTaskComplete(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            var taskName = sender.Name;

            if (taskName == BackgroundDataTemplate.ResetDaylyTasksTimeTriggerName || taskName == BackgroundDataTemplate.ResetDaylyTasksSystemTriggerName) ConfigPlatform();
            else GlobalDataService.UnregisterBackgroundTask(taskName);

            RefreshUI();
        }

        #endregion

        #endregion

        #region Auxiliary Methods

        #region State

        private void SaveState()
        {
            //TODO: Save aplication state to be resume.

        }

        private void LoadState()
        {
            //TODO: Resume aplication state.
        }

        #endregion

        #region Inilialice Components

        private void ConfigPlatform()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    //TODO: Add Android implementation.
                    break;
                case Device.UWP:
                    RegisterBackgroundTask(
                        BackgroundDataTemplate.ResetDaylyTasksTimeTriggerName,
                        BackgroundDataTemplate.ResetDaylyTasksEntryPoint);
                    RegisterBackgroundTask(
                        BackgroundDataTemplate.ResetDaylyTasksSystemTriggerName,
                        BackgroundDataTemplate.ResetDaylyTasksEntryPoint);
                    break;
                //TODO: Add others plafom implementations.
                //case Device.WPF: 
                //case Device.iOS:
                //case Device.macOS:
                //case Device.GTK:
                //case Device.Tizen:
                default:
                    break;
            }
        }

        #endregion

        #region Background Task

        private void RegisterBackgroundTask(string taskName, string taskEntryPoint)
        {
            using (var fileService = new FileService())
            {
                var localFoldeer = FileSystem.AppDataDirectory;
                var initFilePath = $"{localFoldeer}/init.bin";
                var initFile = Task.Run(() =>
                    fileService.Read<InitFile>(initFilePath)).TryTo().Result;
            }

            if ((uint)(DateTime.Today.AddDays(1) - DateTime.Now).TotalMinutes < 15) return;

            var resetDailyTasks = new BackgroundTaskModel
            {
                Title = taskName,
                EntryPoint = taskEntryPoint,
                Trigger = GetTaskTrigger(taskName),
                OnComplete = new BackgroundTaskCompletedEventHandler(OnTaskComplete)
            };

            GlobalDataService.UnregisterBackgroundTask(taskName);
            GlobalDataService.RegisterBackgroundTask(resetDailyTasks);
        }

        private IBackgroundTrigger GetTaskTrigger(string taskName)
        {
            switch (taskName)
            {
                case BackgroundDataTemplate.ResetDaylyTasksTimeTriggerName:
                    var tomorrowMidnight = DateTime.Today.AddDays(1);
                    var timeUntilMidnight = tomorrowMidnight - DateTime.Now;
                    var minutesUntilMidNight = (uint)timeUntilMidnight.TotalMinutes;
                    return new TimeTrigger(minutesUntilMidNight, false);
                case BackgroundDataTemplate.ResetDaylyTasksSystemTriggerName:
                    return new SystemTrigger(SystemTriggerType.UserPresent, false);
                case BackgroundDataTemplate.CompleteTaskName:
                case BackgroundDataTemplate.ShowTaskDetailsName:
                case BackgroundDataTemplate.SnoozeToastName:
                default:
                    return new ApplicationTrigger();
            }
        }

        #endregion

        #region UI

        private void LoadTheme()
        {
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
        }

        private async void RefreshUI()
        {
            var currentPage = NavigationContainer?.CurrentPage;
            if (currentPage == null) return;
            var pageModel = currentPage.BindingContext as IPageModel;
            await pageModel?.RefreshUI();
        }

        #endregion

        #endregion
    }
}
