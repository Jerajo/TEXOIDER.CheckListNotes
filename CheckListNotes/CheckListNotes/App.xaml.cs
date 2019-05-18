using System;
using FreshMvvm;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CheckListNotes.Models;
using CheckListNotes.PageModels;
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


            // Initilalize services
            //Config.Current.AppSettings.Clear();
            GlobalDataService.Init();

            // Set the defoult Page for the aplication
            var page = FreshPageModelResolver.ResolvePageModel<ListOfCheckListsPageModel>();
            NavigationContainer = new FreshNavigationContainer(page);
            MainPage = this.NavigationContainer;
        }

        #region Getters And Setters

        public FreshNavigationContainer NavigationContainer { get; set; }

        #endregion

        #region Device Events

        /// <summary>
        /// Handle when your app starts
        /// </summary>
        protected override void OnStart()
        {
            ConfigPlatform();
            //TODO: Resume aplication state.
        }

        /// <summary>
        /// Handle when your app sleeps
        /// </summary>
        protected override void OnSleep()
        {
            //TODO: Save aplication state to be resume.
        }

        /// <summary>
        /// Handle when your app resumes
        /// </summary>
        protected override void OnResume()
        {
            //TODO: Resume aplication state.
        }

        #endregion

        #region Auxiliary Methods

        private void ConfigPlatform()
        {
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    //TODO: Add Android implementation.
                    break;
                case Device.UWP:
                    var timeTriggerTaskName = "ResetDailyTaskTimeTrigger";
                    var systemTriggerTaskName = "ResetDailyTaskSystemTrigger";
                    GlobalDataService.UnregisterBackgroundTask(timeTriggerTaskName);
                    GlobalDataService.UnregisterBackgroundTask(systemTriggerTaskName);

                    var tomorrowMidnight = DateTime.Today.AddDays(1);
                    var timeUntilMidnight = tomorrowMidnight - DateTime.Now;
                    var minutesUntilMidNight = (uint)timeUntilMidnight.TotalMinutes;
                    var resetDailyTasks = new BackgroundTaskModel
                    {
                        Title = timeTriggerTaskName,
                        EntryPoint = "CheckListNotes.Tasks.ResetDailyTasks",
                        Trigger = new TimeTrigger(15, false),
                        OnComplete = new BackgroundTaskCompletedEventHandler(OnTaskComplete)
                    };
                    GlobalDataService.RegisterBackgroundTask(resetDailyTasks);

                    resetDailyTasks.Title = systemTriggerTaskName;
                    resetDailyTasks.Trigger = new SystemTrigger(SystemTriggerType.UserPresent, false);
                    GlobalDataService.RegisterBackgroundTask(resetDailyTasks);
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

        private void OnTaskComplete(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            var taskName = sender.Name;
            var timeTriggerTaskName = "ResetDailyTaskTimeTrigger";
            var systemTriggerTaskName = "ResetDailyTaskSystemTrigger";
            if (taskName == timeTriggerTaskName || taskName == systemTriggerTaskName)
            {
                GlobalDataService.UnregisterBackgroundTask(timeTriggerTaskName);
                var tomorrowMidnight = DateTime.Today.AddDays(1);
                var timeUntilMidnight = tomorrowMidnight - DateTime.Now;
                var minutesUntilMidNight = (uint)timeUntilMidnight.TotalMinutes;
                var resetDailyTasks = new BackgroundTaskModel
                {
                    Title = "ResetDailyTaskTimeTrigger",
                    EntryPoint = "CheckListNotes.Tasks.ResetDailyTasks",
                    Trigger = new TimeTrigger(minutesUntilMidNight, false),
                    OnComplete = new BackgroundTaskCompletedEventHandler(OnTaskComplete)
                };
                GlobalDataService.RegisterBackgroundTask(resetDailyTasks);

                if (taskName == systemTriggerTaskName)
                {
                    resetDailyTasks.Title = "ResetDailyTaskSystemTrigger";
                    resetDailyTasks.Trigger = new SystemTrigger(SystemTriggerType.UserPresent, false);
                    GlobalDataService.UnregisterBackgroundTask(systemTriggerTaskName);
                    GlobalDataService.RegisterBackgroundTask(resetDailyTasks);
                }
            }
            else GlobalDataService.UnregisterBackgroundTask(taskName);
            RefreshUI();
        }

        private async void RefreshUI()
        {
            var currentPage = NavigationContainer?.CurrentPage;
            if (currentPage == null) return;
            var pageModel = currentPage.BindingContext as IPageModel;
            await pageModel?.RefreshUI();
        }

        #endregion
    }
}
