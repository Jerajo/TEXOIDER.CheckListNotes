using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using CheckListNotes.Models;
using PortableClasses.Enums;
using PortableClasses.Services;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ListOfCheckListsPage : ContentPage
    {
        public ListOfCheckListsPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            AddDebugControls();
        }

        [Conditional("DEBUG")]
        private void AddDebugControls()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition {
                Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition {
                Width = new GridLength(1, GridUnitType.Star) });

            var notyfyButton = new Button { Text = "Notificar",
                Command = new Command(() => ShowReminder(null, null)) };
            var AlarmButton = new Button { Text = "Alertar",
                Command = new Command(() => ShowAlarm(null, null)) };

            var colorConverter = this.Resources["StringToColor"] as IValueConverter;
            notyfyButton.SetBinding(Button.TextColorProperty, "AppFontColor", 
                converter: colorConverter);
            notyfyButton.SetBinding(BackgroundColorProperty, "ButtonBackgroundColor", 
                converter: colorConverter);
            AlarmButton.SetBinding(Button.TextColorProperty, "AppFontColor", 
                converter: colorConverter);
            AlarmButton.SetBinding(BackgroundColorProperty, "ButtonBackgroundColor", 
                converter: colorConverter);

            grid.Children.Add(notyfyButton, 0, 0);
            grid.Children.Add(AlarmButton, 1, 0);
            StackLayoutFooter.Children.Add(grid);
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
                    Arguments = $"action={BackgroundDataTemplate.ShowTaskDetailsName}"
                };

                service.ShowToast(toast);
            }
        }

        [Conditional("DEBUG")]
        private void ShowAlarm(object sender, EventArgs e)
        {
            using (var service = new WindowsToastService())
            {
                var toast = new ToastModel
                {
                    Id = "k5j3hf9s1s",
                    Title = "Esto es una notificación",
                    Body = "Esto es una notificación",
                    Type = ToastTypes.Alarm,
                    Arguments = $"action={BackgroundDataTemplate.CompleteTaskName}&listName=Prueba reinicio de tareas diarias&taskId=4"
                    //Arguments = $"action={BackgroundDataTemplate.CompleteTaskName}&listName={GlobalDataService.LastCheckListName}&taskId={GlobalDataService.LastTaskId}"
                };

                service.ShowToast(toast);
            }
        }
    }
}