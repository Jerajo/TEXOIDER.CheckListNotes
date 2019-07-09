using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Collections;
using CheckListNotes.Models;
using System.Collections.Specialized;
using PortableClasses.Implementations;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SearchListView : ContentView, IDisposable
    {
        #region Atributes

        bool? isDisposing;
        TapGestureRecognizer tapGestureRecognizer;

        #endregion

        #region Constructors

        public SearchListView() { InitializeComponent(); Init(); }
        public SearchListView(IPage page) : this()
        {
            string placeHorder = "";
            if (page is ListOfCheckListsPage)
                placeHorder = AppResourcesLisener.Languages["ListOfListEntryListNameTitle"];
            else placeHorder = AppResourcesLisener.Languages["TaskFormEditorTaskNameTitle"];
            SearchEntry.Placeholder = placeHorder;
        }

        #endregion

        #region Bindable propreties

        public bool? IsSearching
        {
            get => (bool?)GetValue(IsSearchingProperty);
            set => SetValue(IsSearchingProperty, value);
        }

        public static readonly BindableProperty IsSearchingProperty =
            BindableProperty.Create(nameof(IsSearching), typeof(bool?), typeof(SearchListView), false, BindingMode.TwoWay);

        public FulyObservableCollection<ICardModel> ItemsSource
        {
            get => (FulyObservableCollection<ICardModel>)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        public static readonly BindableProperty ItemsSourceProperty =
            BindableProperty.Create(nameof(ItemsSource),
            typeof(FulyObservableCollection<ICardModel>),
            typeof(SearchListView), null, BindingMode.TwoWay,
            propertyChanged: ItemsSourcePropertyChanged);

        public ICardModel SelectedItem
        {
            get => (ICardModel)GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public static readonly BindableProperty SelectedItemProperty =
            BindableProperty.Create(nameof(SelectedItem),
            typeof(ICardModel), typeof(SearchListView), null, BindingMode.OneWayToSource);

        #endregion

        #region Events

        #region SetUp Events

        private static void ItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is SearchListView control)) return;
            if (newValue is FulyObservableCollection<ICardModel> newList)
                newList.CollectionChanged += control.OnCollectionChanged;
            else if (oldValue is FulyObservableCollection<ICardModel> oldList)
            {
                oldList.CollectionChanged -= control.OnCollectionChanged;
                GC.Collect();
            }
        }

        protected override void OnBindingContextChanged()
        {
            if (BindingContext != null)
            {
                this.SetBinding(ItemsSourceProperty, "SearchList");
                this.SetBinding(SelectedItemProperty, "SelectedItem");
                this.SetBinding(IsSearchingProperty, "IsSearching");
                IsSearching = true;
            }
            base.OnBindingContextChanged();
        }

        #endregion

        #region User Events

        private void CloseSearch(object sender, EventArgs e)
        {
            if (!(this.Parent is Grid control)) return;
            IsSearching = null;
            control.Children.Remove(this);
        }

        private void OnItemSelected(object sender, EventArgs e)
        {
            if (!(sender is CardCell control)) return;
            if (control.BindingContext.Equals(SelectedItem)) return;
            else
            {
                Device.BeginInvokeOnMainThread(async () =>
                { // ContentSecundary
                    control.ShadowColor = (Color)App.Current.Resources["ContentPrimary"];
                    await control.ScaleTo(.98, 300, Easing.SpringOut);
                    SelectedItem = control.BindingContext as ICardModel;
                });
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e) =>
            SearchEntry?.SearchCommand?.Execute(e.NewTextValue);

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is null) return;
            int ContainerCount = Container?.Children.Count ?? 0;
            IList newItems = e.NewItems, oldItems = e.OldItems;
            int? newCount = e.NewItems?.Count, oldCount = e.OldItems?.Count;
            int newIndex = e.NewStartingIndex, oldIndex = e.OldStartingIndex;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (newItems == null || newCount <= 0) return;
                    if (newCount > 1) foreach (var item in newItems) AddItem(item);
                    else AddItem(newItems[0]); break;
                case NotifyCollectionChangedAction.Remove:
                    if (oldItems == null || oldCount <= 0) return;
                    RemoveItems(); GC.Collect(); break;
            }

            void AddItem(object item)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (this.Container?.Children == null || item == null) return;
                    var control = new CardCell();
                    control.BindingContext = item;
                    control.GestureRecognizers.Clear();
                    control.GestureRecognizers.Add(tapGestureRecognizer);
                    Container.Children.Add(control);
                });
            }

            void RemoveItems()
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    for (int i = ContainerCount - 1; i >= 0; i--)
                    {
                        if (Container?.Children.Count <= 0) return;
                        Container.Children[i].BindingContext = null;
                        Container?.Children.RemoveAt(i);
                    }
                });
            }
        }

        #endregion

        #endregion

        #region Methods

        private void Init()
        {
            isDisposing = false;
            tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnItemSelected;
        }

        public void Searching() => SearchEntry.SearchCommand?.Execute(SearchEntry.Text);

        #endregion

        #region Dispose

        ~SearchListView()
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
            isDisposing = null;
            IsSearching = null;
            tapGestureRecognizer = null;
        }

        #endregion
    }
}