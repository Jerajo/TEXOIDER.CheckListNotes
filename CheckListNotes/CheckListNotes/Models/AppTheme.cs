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
        public string AppFontColor { get; set; }
        public string AppBackgroundColor { get; set; }
        public string HeaderAndHeaderAndFooterBackgroundColor { get; set; }
        public string CellBackgroundColor { get; set; }
        public string CellBorderColor { get; set; }
        public string EditorBorderColor { get; set; }
        public string EditorBackgroundColor { get; set; }
        public string ButtonBackgroundColor { get; set; }
        public string DialogBackgroundColor { get; set; }
        public string AceptButtonFontColor { get; set; }
        public string CancelButtonFontColor { get; set; }
        public string ViewBoxColor { get; set; }

        #endregion

        #region Auxiliary Methods

        public void Dispose()
        {
            IsDisposing = true;
            HeaderAndHeaderAndFooterBackgroundColor = null;
            HeaderAndHeaderAndFooterBackgroundColor = null;
            CellBackgroundColor = null;
            CellBorderColor = null;
            ButtonBackgroundColor = null;
            AppFontColor = null;
            ViewBoxColor = null;
            AppBackgroundColor = null;
            AceptButtonFontColor = null;
            CancelButtonFontColor = null;
            DialogBackgroundColor = null;
            IsDisposing = false;
        }

        #endregion
    }
}
