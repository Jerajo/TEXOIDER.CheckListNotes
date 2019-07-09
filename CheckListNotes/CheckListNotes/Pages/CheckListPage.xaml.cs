using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using CheckListNotes.Models;
using CheckListNotes.Pages.UserControls;

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

        #region Events

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
