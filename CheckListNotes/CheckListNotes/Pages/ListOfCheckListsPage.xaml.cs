using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using CheckListNotes.Models;
using PortableClasses.Enums;
using PortableClasses.Services;
using CheckListNotes.Pages.UserControls;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListOfCheckListsPage : ContentPage, IPage
    {
        public ListOfCheckListsPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            SizeChanged += PageSizeChanged;

            AddDebugControls(); //Debug
        }

        #region Events

        private void PageSizeChanged(object sender, EventArgs e)
        {
            bool isLandSkape = Height < Width;
        }

        protected override void OnDisappearing()
        {
            CardBoard.ClearValues();
            base.OnDisappearing();
        }

        private void Search(object sender, EventArgs e)
        {
            var searchListView = new SearchListView(this);
            var colunms = MainGrid.ColumnDefinitions.Count;
            var rows = MainGrid.RowDefinitions.Count;
            if (colunms > 1) Grid.SetColumnSpan(searchListView, colunms);
            if (rows > 1) Grid.SetRowSpan(searchListView, rows);
            MainGrid.Children.Add(searchListView);
            searchListView.Searching();
        }

        #endregion

        #region Methods

        public Grid GetMainGrid() => MainGrid;

        #endregion

        #region Debug Methods

        [Conditional("DEBUG")]
        private void AddDebugControls()
        {
            var grid = new Grid();

            var notyfyButton = new Button { Text = "Notificar",
                Command = new Command(() => ShowReminder(null, null)) };
            var AlarmButton = new Button { Text = "Alertar",
                Command = new Command(() => ShowAlarm(null, null)) };

            var fontColor = AppResourcesLisener.Themes["FontColor"];
            notyfyButton.SetValue(Button.TextColorProperty, fontColor);
            notyfyButton.SetValue(BackgroundColorProperty, Color.Transparent);
            AlarmButton.SetValue(Button.TextColorProperty, fontColor);
            AlarmButton.SetValue(BackgroundColorProperty, Color.Transparent);


            grid.Children.Add(notyfyButton, 0, 0);
            grid.Children.Add(AlarmButton, 1, 0);
            Footer.RowDefinitions.Add(new RowDefinition());
            Footer.RowDefinitions.Add(new RowDefinition());
            Footer.Children.Add(grid, 0, 1);
        }

        [Conditional("DEBUG")]
        private void ShowReminder(object sender, EventArgs e)
        {
            using (var service = new WindowsToastService())
            {
                var toast = new ToastModel
                {
                    Id = "sd4g63d42",
                    Title = "Esto es una notificación",
                    Body = "Esto es una notificación",
                    Type = ToastTypes.Notification,
                    Arguments = $"action={BackgroundDataTemplate.ShowTaskDetailsName}&listName=Casos de pruebas&taskId=48.58"
                };

                service.ShowToast(toast);
            }
        }

        [Conditional("DEBUG")]
        private void ShowAlarm(object sender, EventArgs e)
        {
            using (var service = new WindowsToastService())
            {
                var id = "k5j3hf9s1s";
                var toast = new ToastModel
                {
                    Id = id,
                    Title = "Esto es una notificación",
                    Body = "Esto es una notificación",
                    Type = ToastTypes.Alarm,
                    Arguments = $"action={BackgroundDataTemplate.CompleteTaskName}&listName=Casos de pruebas&taskId=48.58&toastId={id}"
                };

                service.ShowToast(toast);
            }
        }



        #endregion
    }
}