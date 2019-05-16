using System;

namespace PortableClasses.Services
{
    public class ConfigHandlerService : IDisposable
    {
        public ConfigHandlerService() { }

        public ConfigHandlerService(string path)
        {
            Path = path;
            Load();
        }

        #region SETTERS AND GETTERS

        public string Path { get; set; }

        public object Config { get; private set; }

        public bool IsDisposing { get; private set; } = false;

        #endregion

        #region Methods

        #region Public Methods

        public object Load(string path = null)
        {
            if (string.IsNullOrEmpty(Path) && string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            try
            {
                if (!string.IsNullOrEmpty(path)) Path = path;
                using (var service = new FileService())
                {
                    return Config = service.Read(Path);
                }
            }
            catch (Exception) { throw; }
        }

        public void Save()
        {
            if (Config == null) throw new NullReferenceException($"The atribute: {nameof(Config)} can't be null.");
            if (string.IsNullOrEmpty(Path)) throw new NullReferenceException($"The atribute: {nameof(Path)} can't be null or empty.");
            try
            {
                using (var service = new FileService())
                {
                    service.Write(Config, Path);
                }
            }
            catch (Exception) { throw; }
        }

        public void Save(object config, string path)
        {
            if (Config == null) throw new ArgumentNullException(nameof(config));
            if (string.IsNullOrEmpty(Path)) throw new ArgumentNullException(nameof(path));
            try
            {
                Config = config; Path = path;
                using (var service = new FileService())
                {
                    service.Write(Config, Path);
                }
            }
            catch (Exception) { throw; }
        }

        #endregion

        #region Auxliary Methods

        public void Dispose(bool isDisposing)
        {
            if (isDisposing) Dispose();
        }

        public void Dispose()
        {
            IsDisposing = true;
            Path = null;
            Config = null;
        }

        #endregion

        #endregion
    }
}

#region Implementation

/* Load Config
 * using(var service = new ConfigHandlerService())
 * {
 *     var path = "~/AppConfig.json";
 *     dynamic config = service.Load(path);
 *     
 *     SelectedTextSize = config.TextSize;
 *     // And so on...
 * }
 * 
 * Save Config
 * using(var service = new ConfigHandlerService())
 * {
 *     var path = "~/AppConfig.json";
 *     dynamic config = service.Load(path);
 *     
 *     config.TextSize = SelectedTextSize;
 *     // And so on...
 *     
 *     service.Save();
 * }
 * 
 * Or Save Config by
 * var path = "~/AppConfig.json";
 * dynamic config = new JObject();
 * 
 * config.TextSize = SelectedTextSize;
 * // And so on...
 * 
 * (new ConfigHandlerService()).Save(config, path);
 * 
 */

#endregion

