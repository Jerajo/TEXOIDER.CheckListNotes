
namespace CheckListNotes.Models.Interfaces
{
    using System.ComponentModel;
    public interface IAppTheme : INotifyPropertyChanged
    {
        string TitleFontColor { get; set; }
        string TitleBackgroundColor { get; set; }

        string CellFontColor { get; set; }
        string CellBackgroundColor { get; set; }
        string CellBorderColor { get; set; }

        string EditiorFontColor { get; set; }
        string EditorBackgroundColor { get; set; }
        string EditorBorderColor { get; set; }

        string ButtonFontColor { get; set; }
        string ButtonBackgroundColor { get; set; }

        string DialogFontColor { get; set; }
        string DialogAceptButtonFontColor { get; set; }
        string DialogAceptButtonBackgroundColor { get; set; }
        string DialogCancelButtonFontColor { get; set; }
        string DialogCancelButtonBackgroundColor { get; set; }
        string DialogBackgroundColor { get; set; }

        string ViewBoxColor { get; set; }
        string LavelFontColor { get; set; }
        string PageBackgroundColor { get; set; }
        string LockerBackgroundColor { get; set; }

        string FooterFontColor { get; set; }
        string FooterBackgroundColor { get; set; }
    }
}
