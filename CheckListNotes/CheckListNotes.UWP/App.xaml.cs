using System;
using Windows.UI.Xaml;
using System.Reflection;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;
using System.Collections.Generic;
using Windows.UI.Xaml.Navigation;
using CarouselView.FormsPlugin.UWP;
using Windows.ApplicationModel.Activation;

namespace CheckListNotes.UWP
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
            Current.HighContrastAdjustment = ApplicationHighContrastAdjustment.Auto;
        }

        #region UWP Events

        /// <summary>
        /// Invoked when the application is activated by some means other than normal launching.
        /// </summary>
        /// <param name="e">Event data for the event.</param>
        protected override void OnActivated(IActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
                OnLaunchedOrActivated(out rootFrame, e);
            if (e is ToastNotificationActivatedEventArgs toastEvent)
            {
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), toastEvent?.Argument);
                else if (Xamarin.Forms.Application.Current is CheckListNotes.App library)
                    library.OnLaunchedOrActivated(toastEvent?.Argument);
            }
            if (rootFrame.Content != null) Window.Current.Activate();
            else Current.Exit();

            base.OnActivated(e);
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            if (!(Window.Current.Content is Frame rootFrame))
                OnLaunchedOrActivated(out rootFrame, e);
            if (rootFrame.Content == null)
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            Window.Current.Activate();
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        async void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            var errorMessage = $"Failed to load Page " + e.SourcePageType.FullName;
            var subscribeDialog = new ContentDialog
            {
                Content = errorMessage,
                CloseButtonText = "Ok",
            };
            await subscribeDialog.ShowAsync();
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            // TODO: Save application state and stop any background activity
            deferral.Complete();
        }

        /// <summary>
        /// Invoked when the application is activated in the background.
        /// </summary>
        /// <param name="e">Makes all data avalible from the IBackgroundTask.Run method avalible to your event when your app is activated by a background trigger.</param>
        protected override void OnBackgroundActivated(BackgroundActivatedEventArgs e)
        {
            var deferral = e.TaskInstance.GetDeferral();
            // Todo: Handle in-process background tasks.
            deferral.Complete();
        }

        #endregion

        #region Method

        private void OnLaunchedOrActivated(out Frame rootFrame, IActivatedEventArgs e)
        {
            rootFrame = new Frame();
            rootFrame.NavigationFailed += OnNavigationFailed;

            List<Assembly> assembliesToInclude = new List<Assembly>();
            assembliesToInclude.Add(typeof(CarouselViewRenderer).GetTypeInfo().Assembly);

            Xamarin.Forms.Forms.Init(e, assembliesToInclude);

            Xamarin.Forms.DependencyService.Register<Dependencies>();

            Window.Current.Content = rootFrame;
        }

        #endregion
    }
}
