using Xamarin.Forms;
using System.Diagnostics;
using Xamarin.Forms.Xaml;
using Xamarin.Forms.Internals;

namespace CheckListNotes.Pages.UserControls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CardCell : ContentView
    {
        public CardCell() => InitializeComponent();

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

        #endregion

        #region Methods

        private Color GetColor(object value)
        {
            if (value is Color color) return color;
            else if (value is DynamicResource resource)
                return (Color)App.Current.Resources[resource.Key];
            else return Color.Transparent;
        }

        #endregion

        #region Dispose

        ~CardCell()
        {
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(CardView)}, Id: {this.GetHashCode()}].");
#endif
        }

        #endregion
    }
}