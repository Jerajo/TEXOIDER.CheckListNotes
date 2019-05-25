using Amporis.Xamarin.Forms.ColorPicker;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class OptionsPage : ContentPage
    {
        public OptionsPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            var buttonColor = (sender as Button).BackgroundColor;
            var color = await ColorPickerDialog.Show(GridMain, "Seleccione un color", buttonColor);
            if (color != null) (sender as Button).BackgroundColor = color;
        }
    }
}