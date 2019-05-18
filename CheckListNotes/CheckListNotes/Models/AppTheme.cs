namespace CheckListNotes.Models
{
    using System;
    using PropertyChanged;
    using CheckListNotes.Models.Interfaces;
    [AddINotifyPropertyChangedInterface]
    class AppTheme : BaseModel, IAppTheme, IDisposable
    {
        #region SETTERS AND GETTERS

        public bool IsDisposing { get; private set; }

        public string TitleFontColor { get; set; }
        public string TitleBackgroundColor { get; set; }

        public string CellFontColor { get; set; }
        public string CellBackgroundColor { get; set; }
        public string CellBorderColor { get; set; }

        public string EditiorFontColor { get; set; }
        public string EditorBackgroundColor { get; set; }
        public string EditorBorderColor { get; set; }

        public string ButtonFontColor { get; set; }
        public string ButtonBackgroundColor { get; set; }

        public string ViewBoxColor { get; set; }
        public string LavelFontColor { get; set; }
        public string PageBackgroundColor { get; set; }
        public string LockerBackgroundColor { get; set; }

        public string DialogFontColor { get; set; }
        public string DialogAceptButtonFontColor { get; set; }
        public string DialogAceptButtonBackgroundColor { get; set; }
        public string DialogCancelButtonFontColor { get; set; }
        public string DialogCancelButtonBackgroundColor { get; set; }
        public string DialogBackgroundColor { get; set; }

        public string FooterFontColor { get; set; }
        public string FooterBackgroundColor { get; set; }

        #endregion

        #region Auxiliary Methods

        public void Dispose()
        {
            IsDisposing = true;
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
            LockerBackgroundColor = null;
            DialogFontColor = null;
            DialogAceptButtonFontColor = null;
            DialogAceptButtonBackgroundColor = null;
            DialogCancelButtonFontColor = null;
            DialogCancelButtonBackgroundColor = null;
            DialogBackgroundColor = null;
            FooterFontColor = null;
            FooterBackgroundColor = null;
            IsDisposing = false;
        }

        #endregion
    }
}
