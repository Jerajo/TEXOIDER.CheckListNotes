using System;
using SkiaSharp;
using Xamarin.Forms;
using System.Diagnostics;
using SkiaSharp.Views.Forms;
using Xamarin.Forms.Internals;
using CheckListNotes.PageModels.Converters;

namespace CheckListNotes.Pages.UserControls
{
    public class MaterialPanel : SKCanvasView, IDisposable
    {
        #region Atributes

        bool? isDisposing;
        StringToColorConverter stringToColor;
        StringToThicknessConverter stringToThickness;
        StringToCornerRadiusConverter stringToCornerRadius;

        #endregion

        public MaterialPanel() : base()
        {
            isDisposing = false;
            stringToColor = new StringToColorConverter();
            stringToThickness = new StringToThicknessConverter();
            stringToCornerRadius = new StringToCornerRadiusConverter();
        }

        #region Bindable Properties

        public object SurfaceColor
        {
            get => GetColor(GetValue(SurfaceColorProperty));
            set => SetValue(SurfaceColorProperty, value);
        }

        private static readonly BindableProperty SurfaceColorProperty =
            BindableProperty.Create(nameof(ShadowColor), typeof(object), typeof(MaterialPanel),
            Color.Transparent, BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        public object ShadowColor
        {
            get => GetColor(GetValue(ShadowColorProperty));
            set => SetValue(SurfaceColorProperty, value);
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

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MaterialPanel)bindable;
            control?.InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            if (SurfaceColor is null || ShadowColor is null || ShadowSize is null ||
                CornerRadius is null) base.OnPaintSurface(e);
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
            SKColor surfaceColor = ((Color)SurfaceColor).ToSKColor();
            SKColor shadowColor = ((Color)ShadowColor).ToSKColor();
            CornerRadius corner = (CornerRadius)CornerRadius;

            var left = (float)shadow.Left;
            var top = (float)shadow.Top;
            var right = (float)shadow.Right;
            var bottom = (float)shadow.Bottom;

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

        ~MaterialPanel()
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
            stringToColor = null;
            stringToThickness = null;
            stringToCornerRadius = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Name: {0}, Id: {1} ].", nameof(MaterialPanel), this.GetHashCode());
#endif
        }

        #endregion
    }
}
