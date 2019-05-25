
namespace CheckListNotes.Models.Interfaces
{
    using System.ComponentModel;
    public interface IAppTheme : INotifyPropertyChanged
    {
        string AppFontColor { get; set; }
        string AppBackgroundColor { get; set; }
        string HeaderAndHeaderAndFooterBackgroundColor { get; set; }
        string CellBackgroundColor { get; set; }
        string CellBorderColor { get; set; }
        string EditorBorderColor { get; set; }
        string EditorBackgroundColor { get; set; }
        string ButtonBackgroundColor { get; set; }
        string DialogBackgroundColor { get; set; }
        string AceptButtonFontColor { get; set; }
        string CancelButtonFontColor { get; set; }
        string ViewBoxColor { get; set; }
    }
}
