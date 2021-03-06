﻿using SkiaSharp;
using Xamarin.Forms;
using System.Diagnostics;
using SkiaSharp.Views.Forms;
using CheckListNotes.Models;
using Xamarin.Forms.Internals;
using CheckListNotes.PageModels.Converters;

namespace CheckListNotes.Pages.UserControls
{
    public class MaterialGrid : Grid
    {
        #region Atributes

#if DEBUG
        //int draws = 0, invalidations = 0, repositions = 0;
#endif
        bool? hasChanges;
        bool? isDrawing;
        bool? isDisposing;
        SKCanvasView canvas;
        SKPaint previousPaint;
        Rectangle? previousBounds;
        Thickness? previousPaddin;
        Thickness? previousMargin;
        SKRoundRect previousRoundRect;
        StringToColorConverter stringToColor;
        StringToThicknessConverter stringToThickness;
        StringToCornerRadiusConverter stringToCornerRadius;
        Color? previousSurfaceColor;
        Color? previousShadowColor;

        #endregion

        public MaterialGrid() => InitializeComponets();

        #region Bindable Properties

        public object SurfaceColor
        {
            get => GetColor(GetValue(SurfaceColorProperty));
            set => SetValue(SurfaceColorProperty, value);
        }

        public static readonly BindableProperty SurfaceColorProperty =
            BindableProperty.Create(nameof(SurfaceColor), typeof(object), typeof(MaterialGrid),
            Color.Transparent, BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        public object ShadowColor
        {
            get => GetColor(GetValue(ShadowColorProperty));
            set => SetValue(ShadowColorProperty, value);
        }

        public static readonly BindableProperty ShadowColorProperty =
            BindableProperty.Create(nameof(ShadowColor), typeof(object), typeof(MaterialGrid),
            Color.FromHex("#B3000000"), BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        public object ShadowSize
        {
            get => GetThickness(GetValue(ShadowSizeProperty));
            set => SetValue(ShadowSizeProperty, value);
        }

        public static readonly BindableProperty ShadowSizeProperty =
            BindableProperty.Create(nameof(ShadowSize), typeof(object),
            typeof(MaterialGrid), new Thickness(5d), BindingMode.OneWay,
            propertyChanged: OnPropertyChanged);

        public object CornerRadius
        {
            get => GetCornerRadius(GetValue(CornerRadiusProperty));
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly BindableProperty CornerRadiusProperty =
            BindableProperty.Create(nameof(CornerRadius), typeof(object),
            typeof(MaterialGrid), new CornerRadius(5d), BindingMode.OneWay,
            propertyChanged: OnPropertyChanged);

        public ShadowPositionTemplate ShadowPosition
        {
            get => (ShadowPositionTemplate)GetValue(ShadowPositionProperty);
            set => SetValue(ShadowPositionProperty, value);
        }

        public static readonly BindableProperty ShadowPositionProperty =
            BindableProperty.Create(nameof(CornerRadius), typeof(ShadowPositionTemplate),
            typeof(MaterialGrid), ShadowPositionTemplate.Outside, BindingMode.OneWay,
            propertyChanged: OnPropertyChanged);

        #endregion

        #region Events

        protected override void OnAdded(View view)
        {
            base.OnAdded(view);
            ChangeGridSpan();
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            ChangeGridSpan();
        }

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var control = (MaterialGrid)bindable;
            if (control?.Parent == null || control?.Bounds.IsEmpty == true) return;
            control?.canvas?.InvalidateSurface();
        }

        protected void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            if (isDrawing == true || this.Parent == null || this.Bounds.IsEmpty) return;
            if (SurfaceColor is null || ShadowColor is null || ShadowSize is null || CornerRadius is null) return;
#if DEBUG
            //Debug.WriteLine($"Invalidated G: #{++invalidations}");
#endif
            if (UpdateShadowPosition()) RedrawSurface(e);
            else if (previousRoundRect != null && previousPaint != null)
            {
                SKCanvas canvas = e.Surface.Canvas;
                canvas.DrawRoundRect(previousRoundRect, previousPaint);
                canvas.Flush();
#if DEBUG
                //Debug.WriteLine($"Drawed G: #{++draws}");
#endif
            }
        }

        #endregion

        #region Methods

        #region Initilizalize Components

        private void InitializeComponets()
        {
            hasChanges = false;
            isDrawing = false;
            isDisposing = false;
            previousSurfaceColor = Color.Transparent;
            previousShadowColor = Color.Transparent;
            stringToColor = new StringToColorConverter();
            stringToThickness = new StringToThicknessConverter();
            stringToCornerRadius = new StringToCornerRadiusConverter();
            canvas = new SKCanvasView();
            canvas.PaintSurface += OnPaintSurface;
            Children.Add(canvas);
        }

        #endregion

        #region Draw

        private void RedrawSurface(SKPaintSurfaceEventArgs e)
        {
            isDrawing = true;
            previousBounds = this.Bounds;
            previousSurfaceColor = (Color)SurfaceColor;
            previousShadowColor = (Color)ShadowColor;
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;
            Thickness shadow = (Thickness)ShadowSize;
            SKColor surfaceColor = previousSurfaceColor.Value.ToSKColor();
            SKColor shadowColor = previousShadowColor.Value.ToSKColor();
            CornerRadius corner = (CornerRadius)CornerRadius;

            var with = (float)(info.Width - shadow.HorizontalThickness);
            var height = (float)(info.Height - shadow.VerticalThickness);

            var rect = new SKRect();
            rect.Offset((float)shadow.Left, (float)shadow.Top);
            rect.Size = new SKSize(with, height);

            var roundRect = previousRoundRect = new SKRoundRect(rect, (float)corner.TopRight,
            (float)corner.BottomLeft);
            canvas.Clear();
            

            var paint = previousPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                Color = surfaceColor,
                IsAntialias = true
            };
            // set drop shadow with position x/y of 2, and blur x/y of 4
            paint.ImageFilter = SKImageFilter.CreateDropShadow(0, 0, 4, 4, shadowColor,
                SKDropShadowImageFilterShadowMode.DrawShadowAndForeground);

            canvas.DrawRoundRect(roundRect, paint);
            canvas.Flush();
#if DEBUG
            //Debug.WriteLine($"Drawed G: #{++draws}");
#endif
            //await Task.Delay(300);
            isDrawing = false;
            //return Task.CompletedTask;
        }

        #endregion

        #region Calclulate

        protected bool UpdateShadowPosition()
        {
            var shadow = (Thickness)ShadowSize;

            if (!HasChanges()) return false;
            if (previousPaddin == Padding && previousMargin == Margin) return true;

            switch (ShadowPosition)
            {
                case ShadowPositionTemplate.AtThePadding:
                    shadow.Left = (float)Padding.Left;
                    shadow.Top = (float)Padding.Top;
                    shadow.Right = (float)Padding.Right;
                    shadow.Bottom = (float)Padding.Bottom;
                    this.canvas.Margin = new Thickness(-shadow.Left, -shadow.Top, -shadow.Right, -shadow.Bottom);
                    break;
                case ShadowPositionTemplate.AtTheMargin:
                    shadow.Left = (float)Margin.Left;
                    shadow.Top = (float)Margin.Top;
                    shadow.Right = (float)Margin.Right;
                    shadow.Bottom = (float)Margin.Bottom;
                    var l = Padding.Left + shadow.Left;
                    var t = Padding.Top + shadow.Top;
                    var r = Padding.Right + shadow.Right;
                    var b = Padding.Bottom + shadow.Bottom;
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                case ShadowPositionTemplate.AtTheMiddle:
                    l = Padding.Left + shadow.Left / 2;
                    t = Padding.Top + shadow.Top / 2;
                    r = Padding.Right + shadow.Right / 2;
                    b = Padding.Bottom + shadow.Bottom / 2;
                    var ml = Margin.Left + shadow.Left / 2;
                    var mt = Margin.Top + shadow.Top / 2;
                    var mr = Margin.Right + shadow.Right / 2;
                    var mb = Margin.Bottom + shadow.Bottom / 2;
                    Padding = new Thickness(l, t, r, b);
                    Margin = new Thickness(ml, mt, mr, mb);
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                case ShadowPositionTemplate.Outside:
                    l = Padding.Left + shadow.Left;
                    t = Padding.Top + shadow.Top;
                    r = Padding.Right + shadow.Right;
                    b = Padding.Bottom + shadow.Bottom;
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                case ShadowPositionTemplate.Inside:
                    l = Padding.Left + shadow.Left;
                    t = Padding.Top + shadow.Top;
                    r = Padding.Right + shadow.Right;
                    b = Padding.Bottom + shadow.Bottom;
                    Padding = new Thickness(l, t, r, b);
                    this.canvas.Margin = new Thickness(-l, -t, -r, -b);
                    break;
                default:
                    return false;
            }
            ShadowSize = shadow;
            previousPaddin = Padding;
            previousMargin = Margin;
#if DEBUG
            //Debug.WriteLine($"Repositioned G: #{++repositions}");
#endif
            return false;
        }

        #endregion

        #region Auxiliary Methods

        private void ChangeGridSpan()
        {
            if (ColumnDefinitions.Count > 0)
            {
                if (Grid.GetColumnSpan(canvas) != ColumnDefinitions.Count)
                    Grid.SetColumnSpan(canvas, ColumnDefinitions.Count);
            }
            if (RowDefinitions.Count > 0)
            { 
                if (Grid.GetRowSpan(canvas) != RowDefinitions.Count)
                    Grid.SetRowSpan(canvas, RowDefinitions.Count);
            }
        }

        private bool HasChanges()
        {
            if (previousBounds != this.Bounds) hasChanges = true;
            else if (previousSurfaceColor != (Color)SurfaceColor ||
                previousShadowColor != (Color)ShadowColor) hasChanges = true;
            else if (previousPaddin != Padding || previousMargin != Margin) hasChanges = true;
            else hasChanges = false;
            return hasChanges.Value;
        }

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

        #endregion

        #region Disposable

        ~MaterialGrid()
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
            if (canvas != null)
            {
                canvas.PaintSurface -= OnPaintSurface;
                canvas = null;
            }
            hasChanges = null;
            isDrawing = null;
            previousBounds = null;
            previousPaddin = null;
            previousMargin = null;
            stringToColor = null;
            stringToThickness = null;
            stringToCornerRadius = null;
            previousSurfaceColor = null;
            previousShadowColor = null;
            previousRoundRect = null;
            previousPaint = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Name: {0}, Id: {1} ].", nameof(MaterialGrid), this.GetHashCode());
#endif
            isDisposing = null;
        }

        #endregion
    }
}
