using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using CheckListNotes.Models;
using PortableClasses.Enums;
using PortableClasses.Services;
using System.Threading.Tasks;

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

            //AddDebugControls();
        }

        #region Debug Methods

        [Conditional("DEBUG")]
        private void AddDebugControls()
        {
            var grid = new Grid();

            var notyfyButton = new Button { Text = "Notificar",
                Command = new Command(() => ShowReminder(null, null)) };
            var AlarmButton = new Button { Text = "Alertar",
                Command = new Command(() => ShowAlarm(null, null)) };

            var colorConverter = this.Resources["StringToColor"] as IValueConverter;
            notyfyButton.SetBinding(Button.TextColorProperty, "FontColor", 
                converter: colorConverter);
            notyfyButton.SetBinding(BackgroundColorProperty, "ButtonBackgroundColor", 
                converter: colorConverter);
            AlarmButton.SetBinding(Button.TextColorProperty, "FontColor", 
                converter: colorConverter);
            AlarmButton.SetBinding(BackgroundColorProperty, "ButtonBackgroundColor", 
                converter: colorConverter);

            grid.Children.Add(notyfyButton, 0, 0);
            grid.Children.Add(AlarmButton, 1, 0);
            //StackLayoutFooter.Children.Add(grid);
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

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (!(sender is Button control)) return;
            if (!(control.Parent is StackLayout parent)) return;

            var index = parent.Children.IndexOf(control);
            parent.Children.Remove(control);
            if (index == 0) parent.Children.Insert(index + 1, control);
            else if (index == 1) parent.Children.Add(control);
            else parent.Children.Insert(0, control);
        }
    }
}