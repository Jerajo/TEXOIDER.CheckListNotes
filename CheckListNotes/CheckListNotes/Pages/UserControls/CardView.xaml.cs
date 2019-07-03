using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using PortableClasses.Enums;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardView : ContentView
    {
        public CardView() => InitializeComponent();

        #region Bindalbe Properties

        public object ShadowColor
        {
            get => GetColor(GetValue(ShadowColorProperty));
            set => SetValue(ShadowColorProperty, value);
        }

        public static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create(nameof(ShadowColor), typeof(object), typeof(MaterialGrid),
            Color.FromHex("#B3000000"), BindingMode.OneWay, propertyChanged: ShadowColorShanged);

        #endregion

        #region Events

        private static void ShadowColorShanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (!(bindable is CardView control)) return;
            control.MaterialContainer.ShadowColor = newValue;
        }

        protected virtual void SwitchToggling(object sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != nameof(SwitchIsCompleted.IsToggled)) return;
            Device.BeginInvokeOnMainThread(async () =>
            {
                SwitchIsCompleted.IsEnabled = false;
                await Task.Delay(300);
                SwitchIsCompleted.IsEnabled = true;
            });
        }

        #endregion

        #region Methods

        private Color GetColor(object value)
        {
            if (value is Color color) return color;
            else if (value is DynamicResource resource)
                return (Color)App.Current.Resources[resource.Key];
            else return Color.Transparent;
        }

        public void InsertOrRemoveContent(View backgroundContent)
        {
            if (MainGrid.Children.Contains(backgroundContent))
                MainGrid.Children.Remove(backgroundContent);
            else MainGrid.Children.Insert(0, backgroundContent);
        }

        #endregion

        #region Dispose

        ~CardView()
        {
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(CardView)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion
    }
}