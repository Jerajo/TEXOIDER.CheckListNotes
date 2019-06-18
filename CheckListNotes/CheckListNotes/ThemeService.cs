using System;
using System.IO;
using System.Diagnostics;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xamarin.Forms;

namespace CheckListNotes
{
    public class ThemeService : IDisposable
    {
        #region Atributos

        private bool? isDisposing;
        private string resourcePath;
        private Stream stream;

        #endregion

        public ThemeService(string resourcePath)
        {
            isDisposing = false;
            this.resourcePath = resourcePath;
            stream = FileSystem.OpenAppPackageFileAsync(resourcePath).Result;
        }

        #region Main Method

        public Task LoadTheme()
        {
            using (var reader = new StreamReader(stream))
            {
                var text = reader.ReadToEnd();
                var document = JToken.Parse(text);
                var resourses = document.ToObject<Dictionary<string, string>>();
                foreach (var item in resourses)
                    App.Current.Resources[item.Key] = Color.FromHex(item.Value);
            }

            // Notify that the current language has changed.
            AppResourcesLisener.Current.RisePropertyChanged(AppResourcesLisener.Theme);

            return Task.CompletedTask;
        }

        #endregion

        #region Dispose

        ~ThemeService()
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
            resourcePath = null;
            stream = null;
#if DEBUG
            Debug.WriteLine("Object destroyect: [ Id: {0}, Name: {1} ].", 
                this.GetHashCode(), nameof(ThemeService));
#endif
        }

        

        #endregion
    }
}
