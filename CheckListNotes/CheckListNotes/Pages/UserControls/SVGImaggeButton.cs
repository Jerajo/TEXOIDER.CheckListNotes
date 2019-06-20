using System;
using SkiaSharp;
using System.IO;
using System.Text;
using Xamarin.Forms;
using System.Diagnostics;
using SkiaSharp.Views.Forms;
using System.Threading.Tasks;
using SVG = SkiaSharp.Extended.Svg;
using CheckListNotes.Pages.Extensions;
using System.Runtime.CompilerServices;

namespace CheckListNotes.Pages.UserControls
{
    public class SVGImaggeButton : ImageButton, IDisposable
    {
        #region Atributes

#if DEBUG
        int invalidate = 0, draws = 0, loads = 0;
#endif
        bool? svgLoaded;
        bool? svgLoading;
        bool? isDisposing;
        SVG.SKSvg svg;
        SKBitmap bitmap;
        SKCanvas canvas;
        SKColor? previousColor;
        Rectangle? previousBounds;
        SKMatrix? scale;

        #endregion

        public SVGImaggeButton() : base()
        {
            svgLoaded = false;
            svgLoading = false;
            isDisposing = false;
            svg = new SVG.SKSvg();
            previousColor = new SKColor();
            previousBounds = new Rectangle();
            bitmap = new SKBitmap();
            canvas = new SKCanvas(bitmap);
            scale = SKMatrix.MakeScale(1, 1);
            this.MeasureInvalidated += OnMeasureInvalidated;
        }

        #region Bindable Properties

        public string SVGSource
        {
            get => (string)GetValue(SVGSourceProperty);
            set => SetValue(SVGSourceProperty, value);
        }

        public static readonly BindableProperty SVGSourceProperty =
            BindableProperty.Create(nameof(SVGSource), typeof(string), typeof(SVGImaggeButton),
            "", BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        public Color? IconColor
        {
            get => (Color)GetValue(IconColorProperty);
            set => SetValue(IconColorProperty, value);
        }

        public static readonly BindableProperty IconColorProperty =
            BindableProperty.Create(nameof(IconColor), typeof(Color), typeof(SVGImaggeButton),
            null, BindingMode.OneWay, propertyChanged: OnPropertyChanged);

        #endregion

        #region Events

        private static void OnPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var constrol = (SVGImaggeButton)bindable;
            if (constrol?.Parent == null || 
                constrol?.Parent.Bounds().IsEmpty == true) return;
            constrol?.InvalidateMeasure();
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (svgLoaded == true) return;
            if (this.Parent == null || this.Parent.Bounds().IsEmpty) return;
            if (propertyName != nameof(SVGSource) && propertyName != nameof(IconColor) &&
                propertyName != nameof(Width) && propertyName != nameof(Height)) return;
#if DEBUG
            Debug.WriteLine($"Invalidation #{++invalidate}");
#endif
            this.InvalidateMeasure();
        }

        protected virtual void OnMeasureInvalidated(object sender, EventArgs e)
        {
            if (svgLoading == true) return;
            if (this.Parent == null || this.Parent.Bounds().IsEmpty) return;
            if (previousColor == IconColor?.ToSKColor() && 
                previousBounds == this.Parent?.Bounds()) return;
            DrawImage();
        }

        #endregion

        #region Methods

        protected virtual bool LoadSVG()
        {
            var parentBounds = this.Parent.Bounds();
            if (parentBounds.IsEmpty == true || string.IsNullOrEmpty(SVGSource))
                return false;
            if (previousBounds == parentBounds) return true;

            var svgText = (string)App.Current.Resources[SVGSource];
            var strm = new MemoryStream(Encoding.Default.GetBytes(svgText));
            svg.Load(strm);

            var svgSize = svg.Picture.CullRect;
            float svgMax = Math.Max(svgSize.Width, svgSize.Height);

            // get aspect ratio and responsive desing
            SKRect bounds = svg.Picture.CullRect;
            var padding = this.Parent.Padding();
            float width = (float)(parentBounds.Width - padding.HorizontalThickness);
            float height = (float)(parentBounds.Height - padding.VerticalThickness);
            float xRatio = width / bounds.Width;
            float yRatio = height / bounds.Height;
            float ratio = Math.Min(xRatio, yRatio);

            scale = SKMatrix.MakeScale(ratio, ratio);

            // generate a bitmap with a canvas
            bitmap = new SKBitmap((int)(bounds.Width * ratio), (int)(bounds.Height * ratio));
            canvas = new SKCanvas(bitmap);
#if DEBUG
            Debug.WriteLine($"Load #{++loads}");
#endif
            return (svgLoaded = true).Value;
        }

        protected virtual void DrawImage()
        {
            svgLoading = true;

            // load sgv bitmap and canvas
            if (!LoadSVG())
            {
                svgLoading = false;
                return;
            }

            // change color
            canvas.Clear();
            var scale = this.scale ?? SKMatrix.MakeScale(1, 1);
            if (IconColor is Color color)
            {
                previousColor = color.ToSKColor();
                var paint = new SKPaint();
                paint.ColorFilter = SKColorFilter.CreateBlendMode(
                    // the color, also `(SKColor)0xFFFF0000` is valid
                    previousColor.Value,       
                    SKBlendMode.SrcIn); // use the source color
                canvas.DrawPicture(svg.Picture, ref scale, paint);
            }
            else canvas.DrawPicture(svg.Picture, ref scale);
            canvas.Flush();
            canvas.Save();

            Device.BeginInvokeOnMainThread(async () => 
            {
                Source = ImageSource.FromStream(() =>
                {
                    SKImage image = SKImage.FromBitmap(bitmap);
                    SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
                    return data.AsStream();
                });
                await Task.Delay(1000);
                previousBounds = this.Parent.Bounds();
                svgLoading = false;
            });
#if DEBUG
            Debug.WriteLine($"Draw #{++draws}");
#endif
        }

        #region DEPRECATED

        // load genereted image if exists DEPRECATED.
        //xif (LoadSouce(SVGSource)) return;
        //protected virtual bool LoadSouce(string fileName)
        //{
        //    string pathToFile = $"{FileSystem.AppDataDirectory}/{Path.GetFileNameWithoutExtension(fileName)}.png";
        //    if (File.Exists(pathToFile) && previousColor == IconColor?.ToSKColor())
        //    {
        //        Source = ImageSource.FromStream(() => File.OpenRead(pathToFile));
        //        return true;
        //    }
        //    else return false;
        //}

        // Save generated bitmap DEPRECATED.
        //xSaveFile(resaizeBitmap, SVGSource);
        //xLoadSouce(SVGSource);
        //protected virtual void SaveFile(SKBitmap bitmap, string fileName)
        //{
        //    string pathToFile = $"{FileSystem.AppDataDirectory}/{Path.GetFileNameWithoutExtension(fileName)}.png";
        //    // resize bitmap
        //    SKBitmap resaizeBitmap = bitmap.Resize((new SKBitmap((int)(width * ratio), 
        //        (int)(height * ratio))).Info, SKFilterQuality.High);
        //    using (SKImage image = SKImage.FromBitmap(bitmap))
        //    using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
        //    using (Stream memStream = data.AsStream())
        //    {
        //        FileStream fileStream = new FileStream(pathToFile, FileMode.Create);
        //        memStream.CopyTo(fileStream);
        //        fileStream.Close();
        //    }
        //}

        #endregion

        #endregion

        #region Dispose

        ~SVGImaggeButton()
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
            this.MeasureInvalidated -= OnMeasureInvalidated;
            svgLoaded = null;
            svgLoading = null;
            previousColor = null;
            previousBounds = null;
            bitmap = null;
            canvas = null;
            scale = null;
            if (canvas != null)
            {
                canvas.Dispose();
                canvas = null;
            }
            if (bitmap != null)
            {
                bitmap.Dispose();
                bitmap = null;
            }
            svg = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Name: {0}, Id: {1} ].", nameof(SVGImaggeButton), this.GetHashCode());
#endif
            isDisposing = null;
        }

        #endregion
    }
}
