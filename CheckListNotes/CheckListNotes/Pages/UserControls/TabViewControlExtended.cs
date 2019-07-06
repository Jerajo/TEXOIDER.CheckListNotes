using Xamarin.Forms;
using Xam.Plugin.TabView;
using PortableClasses.Extensions;
using System.Threading.Tasks;

namespace CheckListNotes.Pages.UserControls
{
    public class TabViewControlExtended : Xam.Plugin.TabView.TabViewControl
    {
        #region Atributes

        bool userChange;
        bool changind;
        public bool IsChangind
        {
            get => (GlobalDataService.IsProcesing || changind);
            set => GlobalDataService.IsProcesing = changind = value;
        }

        #endregion

        public TabViewControlExtended() : base() => userChange = false;

        #region SETTERS ANG GETTERS

        public int CurrentIndex
        {
            get => (int)GetValue(CurrentIndexProperty);
            set => SetValue(CurrentIndexProperty, value);
        }

        public static readonly BindableProperty CurrentIndexProperty = 
            BindableProperty.Create(nameof(CurrentIndex), typeof(int), typeof(TabViewControlExtended), 0, BindingMode.TwoWay, propertyChanged: CurrentIndexPropertyChanged);

        #endregion

        #region Events

        protected override void OnPositionChanging(ref PositionChangingEventArgs e)
        {
            if (e.NewPosition < 0 || e.NewPosition >= ItemSource.Count) e.Canceled = true;
            else if (changind || SelectedTabIndex == e.NewPosition) e.Canceled = true;
            else
            {
                IsChangind = true;
                base.OnPositionChanging(ref e);
            }
        }

        protected override void OnPositionChanged(PositionChangedEventArgs e)
        {
            if (this.CurrentIndex == e.NewPosition) return;
            userChange = true;
            this.CurrentIndex = e.NewPosition;
            base.OnPositionChanged(e);
            IsChangind = false;
        }

        private static void CurrentIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = bindable as TabViewControlExtended;
            if (control == null || control.userChange == true) return;
            if (control?.SelectedTabIndex == (int)newValue) return;
            control.userChange = false;
            control?.SetPosition((int)newValue);
        }

        #endregion
    }
}
