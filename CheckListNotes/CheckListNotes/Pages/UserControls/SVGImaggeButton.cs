using System;
using SkiaSharp;
using System.IO;
using System.Text;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Diagnostics;
using SkiaSharp.Views.Forms;
using SVG = SkiaSharp.Extended.Svg;

namespace CheckListNotes.Pages.UserControls
{
    public class SVGImaggeButton : ImageButton, IDisposable
    {
        #region Atributes

        bool? svgLoaded;
        bool? isDisposing;
        Color? previousColor;

        #endregion

        public SVGImaggeButton() : base()
        {
            svgLoaded = false;
            isDisposing = false;
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
            if (constrol?.Parent == null) return;
            constrol?.RedrawImage();
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (svgLoaded == true) return;
            if ((propertyName != nameof(Height) && propertyName != nameof(Width))
                || this.Parent == null) return;
            RedrawImage();
        }

        #endregion

        #region Methods

        private void RedrawImage()
        {
            var parentWidth = (double)this.Parent.GetValue(WidthProperty);
            var parentHeight = (double)this.Parent.GetValue(HeightProperty);
            var iconColor = IconColor;
            var index = SVGSource;

            if (parentWidth <= 0 || parentHeight <= 0 ||
                string.IsNullOrEmpty(index)) return;

            svgLoaded = true;
            string sFile = $"{FileSystem.AppDataDirectory}/{Path.GetFileNameWithoutExtension(index)}.png";

            // load genereted image if exists
            if (File.Exists(sFile) && previousColor == IconColor)
            {
                SetSource();
                return;
            }

            // load svg
            var svgText = (string)App.Current.Resources[index];
            var strm = new MemoryStream(Encoding.Default.GetBytes(svgText));
            SVG.SKSvg svg = new SVG.SKSvg();
            svg.Load(strm);

            // get aspect ratio and responsive desing
            SKRect bounds = svg.Picture.CullRect;
            float width = bounds.Width;
            float height = bounds.Height;
            double xRatio = parentWidth / width;
            double yRatio = parentHeight / height;
            float ratio = (float)Math.Min(xRatio, yRatio);

            // generate a bitmap with a canvas
            using (SKBitmap bitmap = new SKBitmap((int)(width), (int)(height)))
            //using (SKBitmap bitmap = new SKBitmap((int)(width * ratio), (int)(height * ratio)))
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                // resize canvas
                //var matrix = SKMatrix.MakeScale(ratio, ratio);
                //canvas.DrawPicture(svg.Picture, ref matrix);

                // change color
                if (iconColor is Color color)
                {
                    previousColor = iconColor;
                    var paint = new SKPaint();
                    paint.ColorFilter = SKColorFilter.CreateBlendMode(
                        // the color, also `(SKColor)0xFFFF0000` is valid
                        color.ToSKColor(),       
                        SKBlendMode.SrcIn); // use the source color
                    canvas.DrawPicture(svg.Picture, paint);
                }
                else canvas.DrawPicture(svg.Picture);
                canvas.Flush();
                canvas.Save();


                // resize bitmap
                SKBitmap resaizeBitmap = bitmap.Resize((new SKBitmap((int)(width * ratio), 
                    (int)(height * ratio))).Info, SKFilterQuality.High);

                // save generated bitmap
                using (SKImage image = SKImage.FromBitmap(resaizeBitmap))
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream memStream = new MemoryStream())
                {
                    data.SaveTo(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);

                    FileStream fileStream = new FileStream(sFile, FileMode.Create);
                    memStream.CopyTo(fileStream);
                    fileStream.Close();
                }
                SetSource();
            }

            void SetSource() => Source = ImageSource.FromStream(() => File.OpenRead(sFile));
        }

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
            svgLoaded = null;
            isDisposing = null;
            previousColor = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Name: {0}, Id: {1} ].", nameof(SVGImaggeButton), this.GetHashCode());
#endif
        }

        #endregion
    }
}
