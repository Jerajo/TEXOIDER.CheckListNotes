using System;
using FreshMvvm;
using System.IO;
using Xamarin.Forms;
using Newtonsoft.Json;
using Xamarin.Forms.Xaml;
using Xamarin.Essentials;
using System.Diagnostics;
using CheckListNotes.Models;
using System.ComponentModel;
using System.Threading.Tasks;
using PortableClasses.Services;
using CheckListNotes.PageModels;
using PortableClasses.Extensions;
using CheckListNotes.Models.Interfaces;
using Windows.ApplicationModel.Background;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace CheckListNotes
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            // config.AppSettings.Clear();
            Randomizer = new Random();
            LoadThemeAndLanguage();

            GlobalDataService.Init();
            //(new TutorialListExample()).Create(); // For debug use.

            var page = FreshPageModelResolver.ResolvePageModel<ListOfCheckListsPageModel>();
            NavigationContainer = new FreshNavigationContainer(page);
            MainPage = NavigationContainer;
        }

        #region Getters And Setters

        public FreshNavigationContainer NavigationContainer { get; set; }

        private Random Randomizer { get; set; }

        #endregion

        #region Events

        #region Device Events

        /// <summary>
        /// App developers override this method to respond when the user initiates an app link request.
        /// </summary>
        /// <param name="uri">Unifor sourse identifier resibed from app link.</param>
        protected override void OnAppLinkRequestReceived(Uri uri)
        {
            var data = uri.ToString().ToLowerInvariant();

            if (!data.Contains("/session/")) return;

            var id = data.Substring(data.LastIndexOf("/", StringComparison.Ordinal) + 1);

            //TODO: implement on app link request recived

            base.OnAppLinkRequestReceived(uri);
        }

        /// <summary>
        /// Handle when your app starts
        /// </summary>
        protected override void OnStart()
        {
            ConfigPlatform();
            //LoadState(); //Activate on mememto
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
            //var taskName = sender.Name;
            //if (taskName == BackgroundDataTemplate.ResetDaylyTasksTimeTriggerName || taskName == BackgroundDataTemplate.ResetDaylyTasksSystemTriggerName) ConfigPlatform();
            //else GlobalDataService.UnregisterBackgroundTask(taskName);
            ConfigPlatform();
            RefreshUI();
        }

        public async void OnLaunchedOrActivated(string arguments)
        {
            if (!(NavigationContainer?.CurrentPage is Page currentPage)) return;
            if (!(currentPage.BindingContext is IPageModel pageModel)) return;
            switch (pageModel)
            {
                case ListOfCheckListsPageModel viewModel:
                    await viewModel.PushPageModel<TaskDetailsPageModel>(arguments);
                    break;
                case CheckListPageModel viewModel:
                    await viewModel.PushPageModel<TaskDetailsPageModel>(arguments);
                    break;
                case TaskDetailsPageModel viewModel:
                    viewModel.Init(arguments);
                    break;
                case TaskPageModel viewModel:
                    await viewModel.PushPageModel<TaskDetailsPageModel>(arguments);
                    break;
                case OptionsPageModel viewModel:
                    await viewModel.PushPageModel<TaskDetailsPageModel>(arguments);
                    break;
            }
        }

        private async void PushPageModel(string pageModelName, object arguments)
        {
            if (!(NavigationContainer?.CurrentPage is Page currentPage)) return;
            if (!(currentPage.BindingContext is IPageModel pageModel)) return;
            switch (pageModelName)
            {
                case nameof(ListOfCheckListsPageModel):
                    await pageModel.PushPageModel<ListOfCheckListsPageModel>(arguments);
                    break;
                case nameof(CheckListPageModel):
                    await pageModel.PushPageModel<CheckListPageModel>(arguments);
                    break;
                case nameof(TaskDetailsPageModel):
                    await pageModel.PushPageModel<TaskDetailsPageModel>(arguments);
                    break;
                case nameof(TaskPageModel):
                    await pageModel.PushPageModel<TaskPageModel>(arguments);
                    break;
                case nameof(OptionsPageModel):
                    await pageModel.PushPageModel<OptionsPageModel>(arguments);
                    break;
            }
        }

        #endregion

        #endregion

        #region Auxiliary Methods

        #region State

        private void SaveState()
        {
            using (var fileService = new FileService())
            {
                if (!(NavigationContainer?.CurrentPage is Page currentPage)) return;
                if (!(currentPage.BindingContext is IPageModel pageModel)) return;

                var localFoldeer = FileSystem.AppDataDirectory;
                var initFilePath = $"{localFoldeer}/init.bin";
                var initFile = Task.Run(() =>
                    fileService.Read<InitFile>(initFilePath)).TryTo().Result;

                initFile.LastPageModelName = pageModel.GetType();
                initFile.LastListName = GlobalDataService.CurrentListName;
                initFile.LastIndex = GlobalDataService.CurrentIndex;

                Task.Run(() =>
                    fileService.Write(initFile, initFilePath)).TryTo().Wait();
            }

        }

        private void LoadState()
        {
            //TODO: Resume aplication state.
            using (var fileService = new FileService())
            {
                var localFoldeer = FileSystem.AppDataDirectory;
                var initFilePath = $"{localFoldeer}/init.bin";
                var initFile = Task.Run(() =>
                    fileService.Read<InitFile>(initFilePath)).TryTo().Result;

                var pageModelName = initFile.LastPageModelName;

                if (pageModelName == null) return;

                GlobalDataService.CurrentListName = initFile.LastListName;
                GlobalDataService.CurrentIndex = initFile.LastIndex;

                if (!(NavigationContainer?.CurrentPage is Page currentPage)) return;
                if (!(currentPage.BindingContext is IPageModel pageModel)) return;

                PushPageModel(pageModelName.Name, 0);
            }
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
                    RegisterBackgroundTask(BackgroundDataTemplate.CompleteTaskName,
                        BackgroundDataTemplate.CompleteTaskEntryPoint);
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
                if (initFile.LastResetTime.TimeOfDay == DateTime.Now.TimeOfDay) return;

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
                    return new ToastNotificationActionTrigger();
            }
        }

        #endregion

        #region UI

        private void LoadThemeAndLanguage()
        {
            try
            {
                using (var stream = FileSystem.OpenAppPackageFileAsync($"{Config.Current.Theme}.json").Result)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        var fileContents = reader.ReadToEnd();
                        Config.Current.AppTheme = 
                            JsonConvert.DeserializeObject<AppTheme>(fileContents);
                    }
                }
                using (var languageService = new LanguageService())
                {
                    languageService.LoadLanguage().Wait();
                }
            }
            catch (Exception ex)
            {
                Application.Current.MainPage.DisplayAlert("Error", ex.Message, "Ok");
            }
        }

        private async void RefreshUI()
        {
            if (!(NavigationContainer?.CurrentPage is Page currentPage)) return;
            if (!(currentPage.BindingContext is IPageModel pageModel)) return;
            await pageModel?.RefreshUI();
        }

        #endregion

        #endregion

        #region Disose

        ~App()
        {
            NavigationContainer = null;
            Randomizer = null;
#if DEBUG
            Debug.WriteLine("Object destroyed: [ Id: {1}, Name: {0} ].", this.GetHashCode(), nameof(App));
#endif
        }

        #endregion
    }
}
