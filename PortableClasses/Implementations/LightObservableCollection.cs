using System;
using System.Linq;
using System.Collections;
using System.ComponentModel;
using System.Collections.Generic;

namespace PortableClasses.Implementations
{
    public class LightObservableCollection<T> : IEnumerable<T>, ICollection<T>, INotifyPropertyChanged, IDisposable where T : INotifyPropertyChanged
    {
        #region Atributes

        private ICollection<T> items;
        private bool? isDisposing = false;

        #endregion

        #region Constructors and Indexer

        public LightObservableCollection() { items = new List<T>(); }

        public LightObservableCollection(IEnumerable<T> items)
        {
            this.items = new List<T>();
            AddRange(items);
        }
        public T this[int index]
        {
            get => items.ToArray()[index];
            set
            {
                var indexer = items.ToArray()[index];
                var item = items.FirstOrDefault(m => m.Equals(indexer));
                item = value;
            }
        }

        #endregion

        #region SETTERS AND GETTERS

        /// <summary>
        /// Gets the numbre of elements contained on the ICollection<T>.
        /// </summary>
        public int Count { get => items.Count(); }

        /// <summary>
        /// Gets a value indicating whether the ICollection<T> is read only-only.
        /// </summary>
        public bool IsReadOnly { get => items.IsReadOnly; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when an item is added, removed, changed, moved, or the entire list is
        /// refreshed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify that a value has changed in the collection.
        /// </summary>
        /// <param name="propertyName">Name of the changed property.</param>
        public void RaisePropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        /// <summary>
        /// Method called when an item is added, removed, changed, moved, or the entire list is
        /// refreshed.
        /// </summary>
        /// <param name="sender">Obcjet that called the event.</param>
        /// <param name="e">Event handler.</param>
        protected virtual void ItemPropertyChanged(object sender, PropertyChangedEventArgs e) =>
            PropertyChanged?.Invoke(sender, e);

        #endregion

        #region Methods

        public IEnumerator<T> GetEnumerator() => GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (isDisposing == true || items == null || items?.Count() <= 0) yield break;
            foreach (var item in items)
            {
                if (isDisposing == true) yield break;
                yield return item;
            }
        }

        public void ObserAll()
        {
            foreach (var item in items) item.PropertyChanged += ItemPropertyChanged;
        }

        public void UnobserAll()
        {
            foreach (var item in items) item.PropertyChanged -= ItemPropertyChanged;
        }

        public void Add(T item)
        {
            item.PropertyChanged += ItemPropertyChanged;
            items.Add(item);
            RaisePropertyChanged(nameof(items));
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (var item in items) Add(item);
        }

        public void Clear()
        {
            UnobserAll();
            items.Clear();
        }

        public bool Contains(T item) => items.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => items.CopyTo(array, arrayIndex);

        /// <summary>
        /// Remove and clean ram memory.
        /// </summary>
        /// <param name="item">Item to be remobed.</param>
        /// <returns>Retur the state of the operation.</returns>
        public bool Remove(T item)
        {
            bool removed = false;
            if (items.Contains(item))
            {
                item.PropertyChanged -= ItemPropertyChanged;
                removed = items.Remove(item);
                GC.Collect(GC.GetGeneration(item));
                RaisePropertyChanged(nameof(items));
                return removed;
            }
            else return removed;
        }

        bool ICollection<T>.Remove(T item) => items.Remove(item);

        #endregion

        #region Dispose

        ~LightObservableCollection()
        {
            if (isDisposing == false) Dispose(true);
        }

        public void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (this.isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            if (items != null)
            {
                this.Clear();
                items = null;
            }
            if (PropertyChanged != null) PropertyChanged = null;
            isDisposing = null;
        }

        

        #endregion
    }
}
