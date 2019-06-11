using System;
using System.IO;
using System.Diagnostics;
using Xamarin.Essentials;
using Newtonsoft.Json.Linq;
using System.Globalization;
using CheckListNotes.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace CheckListNotes
{
    public class LanguageService : IDisposable
    {
        #region Atributos

        private bool? isDisposing;
        private string resourcePath;
        private Stream stream;

        #endregion

        public LanguageService()
        {
            isDisposing = false;
            resourcePath = "Languages.json";
            stream = FileSystem.OpenAppPackageFileAsync(resourcePath).Result;
        }

        #region Methods

        public Task LoadLanguage()
        {
            using (var reader = new StreamReader(stream))
            {
                var languageCode = Config.Current.Language.ToLower();
                if (!LanguagesDataTemplate.Contains(languageCode))
                    throw new CultureNotFoundException();
                var text = reader.ReadToEnd();
                var document = JToken.Parse(text);
                var resourses = document[languageCode].ToObject<Dictionary<string,string>>();
                foreach (var item in resourses) App.Current.Resources[item.Key] = item.Value;
            }

            // Notify that the current language has changed.
            AppResourcesLisener.Current.RisePropertyChanged(AppResourcesLisener.Language);

            return Task.CompletedTask;
        }

        #endregion

        #region Dispose

        ~LanguageService()
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
#if DEBUG
            Debug.WriteLine($"Object destroyect: Name: {nameof(LanguageService)}, Id: {this.GetHashCode()}].");
#endif
            isDisposing = null;
            resourcePath = null;
            stream = null;
        }
        
        #endregion
    }
}
