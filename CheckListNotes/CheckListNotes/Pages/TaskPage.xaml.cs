using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using CheckListNotes.PageModels.Converters;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskPage : ContentPage
    {
        public TaskPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            AddDebugControls();
        }

        #region Debug Controls

        [Conditional("DEBUG")]
        private void AddDebugControls()
        {
            var label = new Label
            {
                Text = "Posición",
                TextColor = (Color)App.Current.Resources["FontColor"]
            };

            var entry = new Entry
            {
                Placeholder = "Posicion",
                Keyboard = Keyboard.Numeric,
                TextColor = (Color)App.Current.Resources["FontColor"]
            };

            Container.Children.Add(label);
            Container.Children.Add(entry);

            entry.SetBinding(Entry.TextProperty, "Position", converter: new IntToStringConverter());
        }

        #endregion
    }
}