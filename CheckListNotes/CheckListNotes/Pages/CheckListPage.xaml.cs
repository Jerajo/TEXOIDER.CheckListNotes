using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CheckListNotes.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CheckListPage : ContentPage
    {
        public CheckListPage()
        {
            InitializeComponent();

            NavigationPage.SetHasBackButton(this, false);
            NavigationPage.SetHasNavigationBar(this, false);
        }

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
