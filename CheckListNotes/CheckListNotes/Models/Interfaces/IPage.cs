using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace CheckListNotes.Models
{
    public interface IPage : IPageController
    {
        Grid GetMainGrid();
    }
}
