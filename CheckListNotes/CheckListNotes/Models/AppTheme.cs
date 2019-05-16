namespace CheckListNotes.Models
{
    using System;
    using PropertyChanged;
    using CheckListNotes.Models.Interfaces;
    [AddINotifyPropertyChangedInterfaceAttribute]
    class AppTheme : IAppTheme, IDisposable
    {
        #region SETTERS AND GETTERS

        public bool IsDisposin { get; private set; }

        public string TitleFontColor { get; set; }

        public string TitleBackgroundColor { get; set; }

        public string CellFontColor { get; set; }

        public string CellBackgroundColor { get; set; }

        public string CellBorderColor { get; set; }

        public string ButtonFontColor { get; set; }

        public string ButtonBackgroundColor { get; set; }

        public string EditiorFontColor { get; set; }

        public string EditorBackgroundColor { get; set; }

        public string ViewBoxColor { get; set; }

        public string PageBackgroundColor { get; set; }

        #endregion

        #region Auxiliary Methods

        public void Dispose()
        {
            IsDisposin = true;
            TitleFontColor = null;
            TitleBackgroundColor = null;
            CellFontColor = null;
            CellBackgroundColor = null;
            CellBorderColor = null;
            ButtonFontColor = null;
            ButtonBackgroundColor = null;
            EditiorFontColor = null;
            EditorBackgroundColor = null;
            ViewBoxColor = null;
            PageBackgroundColor = null;
            IsDisposin = false;
        }

        #endregion
    }
}
