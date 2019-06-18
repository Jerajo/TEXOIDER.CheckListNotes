using Xamarin.Forms;
using System.Diagnostics;
using CheckListNotes.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Xamarin.Forms.Xaml;
using SVG = SkiaSharp.Extended.Svg;
using SkiaSharp;
using System.Xml;
using System.Text;
using Xamarin.Essentials;

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
            get
            {
                var svgText = (string)App.Current.Resources[index];
                using (var xmlReader = new MemoryStream(Encoding.Default.GetBytes(svgText)))
                    return GetSvg(xmlReader, index);
            }
        }

        private ImageSource GetSvg(Stream strm, string index)
        {
            string sFile = $"{FileSystem.AppDataDirectory}/{Path.GetFileNameWithoutExtension(index)}.png";

            if (File.Exists(sFile))
                return ImageSource.FromStream(() => File.OpenRead(sFile));

            SVG.SKSvg svg = new SVG.SKSvg();
            svg.Load(strm);

            using (SKBitmap bitmap = new SKBitmap((int)svg.CanvasSize.Width, (int)svg.CanvasSize.Height))
            using (SKCanvas canvas = new SKCanvas(bitmap))
            {
                canvas.DrawPicture(svg.Picture);
                canvas.Flush();
                canvas.Save();

                using (SKImage image = SKImage.FromBitmap(bitmap))
                using (SKData data = image.Encode(SKEncodedImageFormat.Png, 100))
                using (MemoryStream memStream = new MemoryStream())
                {
                    data.SaveTo(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                    //imgSource = ImageSource.FromStream(() => memStream);
                    
                    FileStream fileStream = new FileStream(sFile, FileMode.Create);
                    memStream.CopyTo(fileStream);
                    return ImageSource.FromStream(() => File.OpenRead(sFile));
                }
            }
        }
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
