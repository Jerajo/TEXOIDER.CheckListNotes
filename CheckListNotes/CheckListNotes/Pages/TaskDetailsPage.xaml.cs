using CheckListNotes.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskDetailsPage : ContentPage, IPage
    {
        public TaskDetailsPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);

            DeleteButton.ImageSource = AppResourcesLisener.Images.GetImageSource("Garbage-Closed-Icon", AppResourcesLisener.Themes["Error"], 26);
            EditButton.ImageSource = AppResourcesLisener.Images.GetImageSource("Data-Edit-Icon", AppResourcesLisener.Themes["ContentPrimary"], 26);
        }

        #region Methods

        public Grid GetMainGrid() => MainGrid;

        #endregion
    }
}