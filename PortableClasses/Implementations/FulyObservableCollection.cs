using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace PortableClasses.Implementations
{
    public class FulyObservableCollection<T> : ObservableCollection<T>, IDisposable
        where T : INotifyPropertyChanged
    {
        private bool? isDisposing = false;

        public FulyObservableCollection() : base() { }

        public FulyObservableCollection(IEnumerable<T> enumerable) :
            base(enumerable) => ObserveAll();

        #region Events


        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is
        /// refreshed.
        /// </summary>
        public event PropertyChangedEventHandler ItemPropertyChanged;

        /// <summary>
        /// Notify that a value has changed in the collection.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        public void RaisePropertyChanged(string propertyName) =>
            ItemPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Method called when an item is added, removed, changed, moved, or the entire list is
        /// refreshed.
        /// </summary>
        /// <param name="sender">Obcjet that called the event.</param>
        /// <param name="e">Event handler.</param>
        protected virtual void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) 
            => ItemPropertyChanged?.Invoke(sender, e);

        #endregion

        #region Override

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;
            }

            if (e.Action == NotifyCollectionChangedAction.Add ||
                e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (T item in e.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;
            }

            base.OnCollectionChanged(e);
        }

        public new void ClearItems()
        {
            UnobserveAll();
            base.ClearItems();
        }

        #endregion

        #region Methods

        public void AddRange(IEnumerable<T> enumerator)
        {
            if (enumerator == null) throw new ArgumentNullException(nameof(enumerator));
            foreach (T item in enumerator) Add(item);
        }

        public void ObserveAll()
        {
            foreach (T item in Items)
                item.PropertyChanged += OnItemPropertyChanged;
        }

        public void UnobserveAll()
        {
            foreach (T item in Items)
                item.PropertyChanged -= OnItemPropertyChanged;
        }

        #endregion

        #region Dispose

        ~FulyObservableCollection()
        {
            if (isDisposing == false) Dispose(true);
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(FulyObservableCollection<T>)}, Id: {this.GetHashCode()}].");
#endif
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            if (Items != null) ClearItems();
            if (ItemPropertyChanged != null) ItemPropertyChanged = null;
            isDisposing = null;
        }

        #endregion
    }

    #region Auxiliary Class

    /// <summary>
    /// Provides data for the <see cref="FullyObservableCollection{T}.ItemPropertyChanged"/> event.
    /// </summary>
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs, IDisposable
    {

        /// <summary>
        /// Gets the index in the collection for which the property change has occurred.
        /// </summary>
        /// <value>
        /// Index in parent collection.
        /// </value>
        public int? CollectionIndex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index in the collection of changed item.</param>
        /// <param name="name">The name of the property that changed.</param>
        public ItemPropertyChangedEventArgs(int index, string name) : base(name)
        {
            CollectionIndex = index;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemPropertyChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="args">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        public ItemPropertyChangedEventArgs(int index, PropertyChangedEventArgs args) : 
            this(index, args.PropertyName) { }

        public void Dispose()
        {
            CollectionIndex = null;
            throw new NotImplementedException();
        }

        ~ItemPropertyChangedEventArgs()
        {

        }
    }

    #endregion
}
