using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using CheckListNotes.Models;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckListPage : ContentPage, IPage
    {
        public CheckListPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            NewTaskButton.ImageSource = AppResourcesLisener.Images.GetImageSource("Add-New-Icon", AppResourcesLisener.Themes["ContentPrimary"], 26);
        }

        protected override void OnDisappearing()
        {
            CardBoard.ClearValues();
            base.OnDisappearing();
        }

        #region Methods

        public Grid GetMainGrid() => MainGrid;

        #endregion

        #region Dispose

        ~CheckListPage()
        {
#if DEBUG
            Debug.WriteLine($"Object destroyect: [Name: {nameof(CheckListPage)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion
    }
}
