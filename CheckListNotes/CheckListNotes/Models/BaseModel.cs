namespace CheckListNotes.Models
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class BaseModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is
        /// refreshed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Notify that a value has changed in the collection.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        public void RisePropertyChanged(string propertyName) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
