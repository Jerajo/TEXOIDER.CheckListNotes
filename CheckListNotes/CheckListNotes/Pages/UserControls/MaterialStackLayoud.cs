using SkiaSharp;
using Xamarin.Forms;
using System.Diagnostics;
using SkiaSharp.Views.Forms;
using CheckListNotes.Models;
using Xamarin.Forms.Internals;
using CheckListNotes.PageModels.Converters;

namespace CheckListNotes.Pages.UserControls
{
    public class MaterialStackLayoud : StackLayout
    {
        #region Atributes

        bool? isDisposing;
        SKCanvasView canvas;
        StringToColorConverter stringToColor;
        StringToThicknessConverter stringToThickness;
        StringToCornerRadiusConverter stringToCornerRadius;

        #endregion

        public MaterialStackLayoud() : base()
        {
            isDisposing = false;
            stringToColor = new StringToColorConverter();
            stringToThickness = new StringToThicknessConverter();
            stringToCornerRadius = new StringToCornerRadiusConverter();
            canvas = new SKCanvasView();
            canvas.PaintSurface += OnPaintSurface;
            Children.Add(canvas);
        }

        #region Bindable Properties

        public object SurfaceColor
        {
            get => GetColor(GetValue(SurfaceColorProperty));
            set => SetValue(SurfaceColorProperty, value);
        }

        private static readonly BindableProperty SurfaceColorProperty =
            BindableProperty.Create(nameof(SurfaceColor), typeof(object), typeof(MaterialPanel),
            Color.Transparent, BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        public object ShadowColor
        {
            get => GetColor(GetValue(ShadowColorProperty));
            set => SetValue(ShadowColorProperty, value);
        }

        private static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create(nameof(ShadowColor), typeof(object), typeof(MaterialPanel),
            Color.FromHex("#B3000000"), BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        public object ShadowSize
        {
            get => GetThickness(GetValue(ShadowSizeProperty));
            set => SetValue(ShadowSizeProperty, value);
        }

        private static readonly BindableProperty ShadowSizeProperty =
            BindableProperty.Create(nameof(ShadowSize), typeof(object),
            typeof(MaterialPanel), new Thickness(5d), BindingMode.OneWay,
            propertyChanged: OnPropertyChanged);

        public object CornerRadius
        {
            get => GetCornerRadius(GetValue(CornerRadiusProperty));
            set => SetValue(CornerRadiusProperty, value);
        }

        private static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(nameof(CornerRadius), typeof(object),
            typeof(MaterialPanel), new CornerRadius(5d), BindingMode.OneWay,
            propertyChanged: OnPropertyChanged);

        public ShadowPositionTemplate ShadowPosition
        {
            get => (ShadowPositionTemplate)GetValue(ShadowPositionProperty);
            set => SetValue(ShadowPositionProperty, value);
        }

        private static readonly BindableProperty ShadowPositionProperty =
            BindableProperty.Create(nameof(CornerRadius), typeof(ShadowPositionTemplate),
            typeof(MaterialPanel), ShadowPositionTemplate.Outside, BindingMode.OneWay,
            propertyChanged: OnPropertyChanged);

        #endregion

        #region Events

        #region Coerce Value Call Back

        private Color GetColor(object value)
        {
            if (value is Color color) return color;
            else if (value is DynamicResource resource)
                return (Color)App.Current.Resources[resource.Key];
            else return stringToColor?.Convert(value) ?? Color.Transparent;
        }

        private Thickness GetThickness(object value)
        {
            if (value is Thickness size) return size;
            else if (value is DynamicResource resource)
                return (Thickness)App.Current.Resources[resource.Key];
            else return stringToThickness?.Convert(value) ?? new Thickness(5);
        }

        private CornerRadius GetCornerRadius(object value)
        {
            if (value is CornerRadius corner) return corner;
            else if (value is DynamicResource resource)
                return (CornerRadius)App.Current.Resources[resource.Key];
            else return stringToCornerRadius?.Convert(value) ?? new CornerRadius(5);
        }

        #endregion

        #region Property Changed

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            //if (ColumnDefinitions.Count > 0 && Grid.GetColumnSpan(canvas) != ColumnDefinitions.Count) Grid.SetColumnSpan(canvas, ColumnDefinitions.Count);

            //if (RowDefinitions.Count > 0 && Grid.GetRowSpan(canvas) != RowDefinitions.Count)
            //    Grid.SetRowSpan(canvas, RowDefinitions.Count);
        }

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MaterialStackLayoud)bindable;
            control?.canvas?.InvalidateSurface();
        }

        protected void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (SurfaceColor is null || ShadowColor is null || ShadowSize is null ||
                CornerRadius is null) return;
            else RedrawSurface(e);
        }

        #endregion

        #endregion

        #region Methods

        private void RedrawSurface(SKPaintSurfaceEventArgs e)
        {
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            Thickness shadow = (Thickness)ShadowSize;
            SKColor surfaceColor = ((Color)(SurfaceColor)).ToSKColor();
            SKColor shadowColor = ((Color)ShadowColor).ToSKColor();
            CornerRadius corner = (CornerRadius)CornerRadius;
            ShadowPositionTemplate position = ShadowPosition;

            float left = (float)shadow.Left;
            float top = (float)shadow.Top;
            float right = (float)shadow.Right;
            float bottom = (float)shadow.Bottom;

            switch (ShadowPosition)
            {
                case ShadowPositionTemplate.AtThePadding:
                    left = (float)Padding.Left;
                    top = (float)Padding.Top;
                    right = (float)Padding.Right;
                    bottom = (float)Padding.Bottom;
                    this.canvas.Margin = new Thickness(-left, -top, -right, -bottom);
                    break;
                case ShadowPositionTemplate.AtTheMargin:
                    left = (float)Margin.Left;
                    top = (float)Margin.Top;
                    right = (float)Margin.Right;
                    bottom = (float)Margin.Bottom;
                    var l = Padding.Left + left;
                    var t = Padding.Top + top;
                    var r = Padding.Right + right;
                    var b = Padding.Bottom + bottom;
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                case ShadowPositionTemplate.AtTheMiddle:
                    l = Padding.Left + left / 2;
                    t = Padding.Top + top / 2;
                    r = Padding.Right + right / 2;
                    b = Padding.Bottom + bottom / 2;
                    var ml = Margin.Left + left / 2;
                    var mt = Margin.Top + top / 2;
                    var mr = Margin.Right + right / 2;
                    var mb = Margin.Bottom + bottom / 2;
                    Padding = new Thickness(l, t, r, b);
                    Margin = new Thickness(ml, mt, mr, mb);
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                case ShadowPositionTemplate.Outside:
                    l = Padding.Left + left;
                    t = Padding.Top + top;
                    r = Padding.Right + right;
                    b = Padding.Bottom + bottom;
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                case ShadowPositionTemplate.Inside:
                default:
                    l = Padding.Left + left;
                    t = Padding.Top + top;
                    r = Padding.Right + right;
                    b = Padding.Bottom + bottom;
                    Padding = new Thickness(l, t, r, b);
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
            }

            var with = (info.Width - (left + right));
            var height = (info.Height - (top + bottom));

            var rect = new SKRect();
            rect.Offset(left, top);
            rect.Size = new SKSize(with, height);

            using (var roundRect = new SKRoundRect(rect, (float)corner.TopRight,
                (float)corner.BottomLeft))
            {
                canvas.Clear();

                var paint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = surfaceColor,
                    IsAntialias = true
                };
                // set drop shadow with position x/y of 2, and blur x/y of 4
                paint.ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 4, 4, shadowColor,
                    SKDropShadowImageFilterShadowMode.DrawShadowAndForeground);

                //canvas.ClipRoundRect(roundRect);
                canvas.DrawRoundRect(roundRect, paint);
            }
        }

        #endregion

        #region Disposable

        ~MaterialStackLayoud()
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
            Children.Clear();
            isDisposing = null;
            stringToColor = null;
            stringToThickness = null;
            stringToCornerRadius = null;
            canvas = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Name: {0}, Id: {1} ].", nameof(MaterialStackLayoud), this.GetHashCode());
#endif
        }

        #endregion
    }
}
