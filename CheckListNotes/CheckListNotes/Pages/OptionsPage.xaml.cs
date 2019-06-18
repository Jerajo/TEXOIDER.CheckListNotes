using System;
using System.Threading.Tasks;
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

            var tabGestureReconizer = new TapGestureRecognizer();
            tabGestureReconizer.Tapped += LabelTapped;
            LabelTheme.GestureRecognizers.Add(tabGestureReconizer);
            LabelSound.GestureRecognizers.Add(tabGestureReconizer);
            LabelOthers.GestureRecognizers.Add(tabGestureReconizer);
        }

        #region Events

        // label tapped event
        private void LabelTapped(object sender, EventArgs e)
        {
            if (sender.Equals(LabelTheme)) AnimateContent(StackLayoutTheme);
            else if (sender.Equals(LabelSound)) AnimateContent(StackLayoutSound);
            else AnimateContent(StackLayoutOthers);
        }

        // Color picker event
        private async void Button_Clicked(object sender, System.EventArgs e)
        {
            var buttonColor = (sender as Button).BackgroundColor;
            var color = await ColorPickerDialog.Show(GridMain, "Seleccione un color", buttonColor);
            if (color != null) (sender as Button).BackgroundColor = color;
        }

        #endregion

        #region Auxiliary Methods

        async void AnimateContent(StackLayout toShow)
        {
            if (toShow.IsVisible) return;

            var toHide = GetVisible();

            Parallel.Invoke(
                async () => await toHide.ScaleTo(0, 100),
                async () => await toHide.FadeTo(0, 100),
                async () => await toShow.ScaleTo(0, 100),
                async () => await toShow.FadeTo(0, 100)
            );

            await Task.Delay(100); //Wait until animation ends
            toHide.IsVisible = false;
            toShow.IsVisible = true;

            Parallel.Invoke(
                async () => await toShow.ScaleTo(1, 200),
                async () => await toShow.FadeTo(1, 200)
            );
        }

        private StackLayout GetVisible() => StackLayoutTheme.IsVisible ? StackLayoutTheme : StackLayoutSound.IsVisible ? StackLayoutSound : StackLayoutOthers;

        #endregion

        #region Dispose

        ~OptionsPage()
        {

        }

        #endregion
    }
}