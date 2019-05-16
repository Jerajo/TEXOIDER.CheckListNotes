using System;
using System.Collections.Generic;
using System.Text;

namespace CheckListNotes.Models.Interfaces
{
    public interface IAppTheme
    {
        string TitleFontColor { get; set; }
        string TitleBackgroundColor { get; set; }

        string CellFontColor { get; set; }
        string CellBackgroundColor { get; set; }
        string CellBorderColor { get; set; }

        string ButtonFontColor { get; set; }
        string ButtonBackgroundColor { get; set; }

        string EditiorFontColor { get; set; }
        string EditorBackgroundColor { get; set; }

        string ViewBoxColor { get; set; }
        string PageBackgroundColor { get; set; }
    }
}
