using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using CheckListNotes.Models;
using System.Threading.Tasks;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CustomDialog : ContentView, IDisposable
    {
        #region Atributes

        bool? isDisposing;

        #endregion

        public CustomDialog() { InitializeComponent(); Init(); }

        #region SETTERS AND GETTERS

        public bool? Resoult { get; set; }

        #endregion

        #region Events

        private void OKButton_Clicked(object sender, EventArgs e) => Resoult = true;

        private void CancelButton_Clicked(object sender, EventArgs e) => Resoult = false;

        #endregion

        #region Methods

        private void Init()
        {
            isDisposing = false;
        }

        public static async Task<bool> Show(Grid mainGrid, string title, string message, params ButtonModel[] models)
        {
            var dialog = CreateDialog(mainGrid);
            dialog.Title.Text = title;
            dialog.Message.Text = message;
            CreateButtons(dialog, models);
            while (dialog.Resoult == null) await Task.Delay(300);
            mainGrid.Children.Remove(dialog);
            bool resoult = dialog.Resoult.Value;
            dialog = null;
            GC.Collect();
            return resoult;
        }

        private static CustomDialog CreateDialog(Grid mainGrid)
        {
            var customDialog = new CustomDialog();
            var colunms = mainGrid.ColumnDefinitions.Count;
            var rows = mainGrid.RowDefinitions.Count;
            if (colunms > 1) Grid.SetColumnSpan(customDialog, colunms);
            if (rows > 1) Grid.SetRowSpan(customDialog, rows);
            mainGrid.Children.Add(customDialog);
            return customDialog;
        }

        private static void CreateButtons(CustomDialog control, ButtonModel[] models)
        {
            var colums = 0;
            foreach(var model in models)
            {
                var button = new Button
                {
                    Text = model.Text,
                    ImageSource = model.Icon,
                    TextColor = model.TextColor,
                    BackgroundColor = model.BackgroundColor,
                };
                if (colums == 0) button.Clicked += control.OKButton_Clicked;
                else button.Clicked += control.CancelButton_Clicked;
                control.Footer.Children.Add(button, colums++, 0);
            }
        }

        #endregion

        #region Dispose

        ~CustomDialog()
        {
            if (isDisposing == false) Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            Resoult = null;
            isDisposing = null;
        }

        #endregion
    }
}