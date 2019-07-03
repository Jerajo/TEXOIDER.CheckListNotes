using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Threading.Tasks;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TabView : ContentView
    {
        #region Atributes

        bool firtsInit;
        bool isAnimating;
        public event EventHandler<TabChangingArgs> TabChanging;
        public event EventHandler<TabChangedArgs> TabChanged;

        #endregion

        public TabView() : base() { InitializeComponent(); Init(); }

        #region Bindable Properties

        public bool IsAnimating
        {
            get => (GlobalDataService.IsProcesing || isAnimating);
            set => GlobalDataService.IsProcesing = isAnimating = value;
        }

        protected int CurrentIndex
        {
            get
            {
                var column = Grid.GetColumn(SelectionIndicator);
                if (column >= 0 && column <= 1) return column;
                else return -1;
            }
        }

        public int Index
        {
            get => (int)GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public static readonly BindableProperty IndexProperty =
            BindableProperty.Create(nameof(Index), typeof(int), typeof(TabView), 0, BindingMode.TwoWay, propertyChanged: IndexPropertyChanged);

        public View ItemSource
        {
            get => (View)GetValue(ItemSourceProperty);
            set => SetValue(ItemSourceProperty, value);
        }

        public static readonly BindableProperty ItemSourceProperty =
            BindableProperty.Create(nameof(ItemSource), typeof(View), typeof(TabView), default, BindingMode.TwoWay, propertyChanged: ItemSourcePropertyChanged);

        #endregion

        #region Events

        private static void IndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is TabView view)) return;
            if (view.IsAnimating || view.CurrentIndex == (int)newValue) return;
            view.ChangeIndex((int)newValue);
        }

        protected virtual void OnTabChanging(TabChangingArgs e)
        {
            if (e.OldValue == e.NewValue) e.IsCanceled = true;
            else if (this.CurrentIndex == e.NewValue) e.IsCanceled = true;
            else if (e.NewValue < 0 || e.NewValue > 1) e.IsCanceled = true;
            else TabChanging?.Invoke(this, e);
        }

        protected virtual void OnTabTapped(object sender, EventArgs e)
        {
            if (sender.Equals(HeaderLeft)) ChangeIndex(0);
            else ChangeIndex(1);
        }

        protected virtual void OnTabChanged(TabChangedArgs e)
        {
            Index = e.NewValue;
            TabChanged?.Invoke(this, e);
        }

        private static void ItemSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is TabView view)) return;
            view.ItemsContainer.Content = (View)newValue;
        }

        #endregion

        #region Methods

        #region Public

        public virtual async void ChangeIndex(int index)
        {
            if (IsAnimating && !firtsInit)
                return;
            firtsInit = !(isAnimating = true);

            var oldValue = this.CurrentIndex;
            var changing = new TabChangingArgs { OldValue = oldValue, NewValue = index };
            OnTabChanging(changing);

            if (changing == null || changing.IsCanceled)
            {
                isAnimating = false;
                return;
            }

            await AnimateTabTransition(index);

            OnTabChanged(new TabChangedArgs { OldValue = oldValue, NewValue = index });
            isAnimating = false;
        }

        #endregion

        #region Private

        private void Init()
        {
            firtsInit = true;
            isAnimating = false;
            ItemsContainer.Content = ItemSource;
        }

        private async Task AnimateTabTransition(int index)
        {
            Grid.SetColumn(SelectionIndicator, index);
            await Task.Delay(300);
        }

        #endregion

        #endregion
    }

    #region Auxiliary Classes

    public class TabChangingArgs : EventArgs
    {
        public bool IsCanceled { get; set; } = false;
        public int OldValue { get; set; }
        public int NewValue { get; set; }
    }

    public class TabChangedArgs : EventArgs
    {
        public int OldValue { get; set; }
        public int NewValue { get; set; }
    }

    #endregion
}