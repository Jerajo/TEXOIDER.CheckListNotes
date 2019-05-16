using Xamarin.Forms;
using Xam.Plugin.TabView;

namespace CheckListNotes.Pages.UserControls
{
    public class TabViewControlExtended : TabViewControl
    {
        #region Atributes

        private int currentIndex = 0;

        #endregion

        public TabViewControlExtended() : base()
        {
            PositionChanged += IndexChanged;
        }

        #region SETTERS ANG GETTERS

        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                currentIndex = value;
                SetValue(CurrentIndexProperty, value);
            }
        }

        public static readonly BindableProperty CurrentIndexProperty = 
            BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(TabViewControlExtended), 0, BindingMode.TwoWay, propertyChanged: CurrentIndexPropertyChanged);

        private static void CurrentIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as TabViewControlExtended;
            if (oldValue != newValue) control.SetPosition((int)newValue);
        }

        #endregion

        #region Methods

        private void IndexChanged(object sender, PositionChangedEventArgs e)
        {
            CurrentIndex = e.NewPosition;
        }

        #endregion
    }
}
