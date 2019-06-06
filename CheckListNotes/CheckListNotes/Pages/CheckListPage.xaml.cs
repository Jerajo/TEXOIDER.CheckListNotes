using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.ComponentModel;

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

            AppResourcesLisener.Current.PropertyChanged += OnLanguageChanged;
            OnLanguageChanged(null, new PropertyChangedEventArgs(AppResourcesLisener.Language));
        }

        #region Methods

        private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != AppResourcesLisener.Language) return;
            var resourses = Application.Current.Resources;
            TabItemPendientTask.HeaderText = resourses["TaskListPendientTabTitle"].ToString();
            TabItemCompletedTask.HeaderText = resourses["TaskListCompletedTabTitle"].ToString();
        }

        #endregion

        #region Dispose

        ~CheckListPage()
        {
            AppResourcesLisener.Current.PropertyChanged -= OnLanguageChanged;
#if DEBUG
            Debug.WriteLine($"Object destroyect: [Name: {nameof(CheckListPage)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion
    }
}
