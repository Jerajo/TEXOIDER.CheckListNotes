using PortableClasses.Enums;
using System.ComponentModel;

namespace CheckListNotes.Models
{
    public interface ICardModel : INotifyPropertyChanged
    {
        string Id { get; set; }
        string Name { get; set; }
        int? Position { get; set; }
        bool? IsAnimating { get; set; }
        string CompletedDateText { get; }
        SelectedFor? SelectedReason { get; set; }
        string ExpirationDateText { get; }
        bool? HasExpiration { get; set; }
        int? CompletedTasks { get; set; }
        bool? IsTaskGroup { get; set; }
        int? TotalTasks { get; set; }
    }
}
