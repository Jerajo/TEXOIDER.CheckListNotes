using System.IO;
using SkiaSharp;
using System.Text;
using Xamarin.Forms;
using System.Diagnostics;
using CheckListNotes.Models;
using SkiaSharp.Views.Forms;
using SVG = SkiaSharp.Extended.Svg;

namespace CheckListNotes
{
    public class AppResourcesLisener : BaseModel
    {
        #region Atributes

        public const string Language = "Language";
        public const string Theme = "Theme";
        private bool? isDispposing;
        private static AppResourcesLisener current;
        private static LanguageResources languages;
        private static ThemeResources themes;
        private static ImageResources images;

        #endregion

        private AppResourcesLisener()
        {
            isDispposing = false;
            languages = new LanguageResources();
            themes = new ThemeResources();
            images = new ImageResources();
        }

        #region SETTERS AND GETTERS

        public static AppResourcesLisener Current
        {
            get => current ?? (current = new AppResourcesLisener());
        }

        public static LanguageResources Languages
        {
            get => languages ?? (languages = new LanguageResources());
        }

        public static ThemeResources Themes
        {
            get => themes ?? (themes = new ThemeResources());
        }

        public static ImageResources Images
        {
            get => images ?? (images = new ImageResources());
        }

        #endregion

        #region Dispose

        ~AppResourcesLisener()
        {
            if (isDispposing == false) Dispose(true);
        }

        public void Dispose(bool isDispposing)
        {
            this.isDispposing = isDispposing;
            if (this.isDispposing == true) Dispose();
        }

        public void Dispose()
        {
            themes = null;
            images = null;
            languages = null;
            isDispposing = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Id: {0}, Name: {1} ].",
                this.GetHashCode(), nameof(AppResourcesLisener));
#endif
            current = null;
        }

        #endregion
    }

    #region Auxiliary Classes

    public class ImageResources
    {
        public ImageSource this[string index]
        {
            get => GetImageSource(index);
        }

        public ImageSource GetImageSource(string index, Color? color = null, int? size = null) => ImageSource.FromStream(() => 
        {
            var imgSize = size ?? (int)App.Current.Resources["IconsSize"];
            var imgColor = color ?? AppResourcesLisener.Themes["FontColorInverse"];
            var svgText = (string)App.Current.Resources[index];
            var xmlReader = new MemoryStream(Encoding.Default.GetBytes(svgText));
            SVG.SKSvg svg = new SVG.SKSvg();
            svg.Load(xmlReader);

            SKBitmap bitmap = new SKBitmap((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height);
            SKCanvas canvas = new SKCanvas(bitmap);

            var paint = new SKPaint();
            paint.ColorFilter = SKColorFilter.CreateBlendMode(imgColor.ToSKColor(), SKBlendMode.SrcIn);

            canvas.Clear();
            canvas.DrawPicture(svg.Picture, paint);
            canvas.Flush();
            canvas.Save();

            SKImage image = SKImage.FromBitmap(GetResizedBitmap(bitmap, imgSize));
            SKData data = image.Encode(SKEncodedImageFormat.Png, 100);
            return data.AsStream();
        });

        private SKBitmap GetResizedBitmap(SKBitmap bitmap, int size)
        {
            SKBitmap scaledBitmap = new SKBitmap(size, size);
            var resoult = bitmap.ScalePixels(scaledBitmap, SKFilterQuality.High);
            return resoult ? scaledBitmap : bitmap;
        }

        /*x //-------------------------// Deprecated //-------------------------//
         * string sFile = $"{FileSystem.AppDataDirectory}/{Path.GetFileNameWithoutExtension(index)}.png";
         * if (File.Exists(sFile))
         *     return ImageSource.FromStream(() => File.OpenRead(sFile));
         *     
         * //Save data in a file 
         * using (MemoryStream memStream = new MemoryStream())
         * {
         *     data.SaveTo(memStream);
         *     memStream.Seek(0, SeekOrigin.Begin);
         *     imgSource = ImageSource.FromStream(() => memStream);
         *     FileStream fileStream = new FileStream(sFile, FileMode.Create);
         *     memStream.CopyTo(fileStream);
         *     return ImageSource.FromStream(() => File.OpenRead(sFile));
         * }
         */
    }

    public class ThemeResources
    {
        public Color this[string index]
        {
            get => (Color)App.Current.Resources[index];
            set => App.Current.Resources[index] = value;
        }
    }

    public class LanguageResources
    {
        public string this[string index]
        {
            get => App.Current.Resources[index].ToString();
            set => App.Current.Resources[index] = value;
        }
    }

    #endregion
}
