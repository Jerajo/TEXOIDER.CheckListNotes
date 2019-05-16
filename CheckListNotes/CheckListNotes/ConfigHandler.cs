using System;
using Xamarin.Essentials;
using Plugin.Settings.Abstractions;

namespace CheckListNotes
{
    public static class ConfigHandler
    {
        #region Atributes

        private static dynamic config;

        #endregion

        #region SETERS AND GETTERS

        public static bool HasLoaded { get; set; } = false;

        public static AppConfig Config
        {
            get => config ?? Load();
        }

        #endregion

        #region Methods

        public static object Load()
        {
            HasLoaded = true;
            return config = new AppConfig();
        }

        #endregion
    }

    public class AppConfig : ISettings
    {
        #region Add or Updaate Values

        public bool AddOrUpdateValue(string key, decimal value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, (double)value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, bool value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, long value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, string value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, int value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, float value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, DateTime value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, Guid value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool AddOrUpdateValue(string key, double value, string fileName = null)
        {
            try
            {
                Preferences.Set(key, value);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        #endregion

        #region Get Value or defoult

        public decimal GetValueOrDefault(string key, decimal defaultValue, string fileName = null)
        {
            try
            {
                return (decimal)Preferences.Get(key, (double)defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public bool GetValueOrDefault(string key, bool defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public long GetValueOrDefault(string key, long defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public string GetValueOrDefault(string key, string defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public int GetValueOrDefault(string key, int defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public float GetValueOrDefault(string key, float defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public DateTime GetValueOrDefault(string key, DateTime defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public Guid GetValueOrDefault(string key, Guid defaultValue, string fileName = null)
        {
            try
            {
                var config = Preferences.Get(key, "");
                var parsed = Guid.TryParse(config, out Guid newGuid);
                return (parsed) ? newGuid : defaultValue;
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public double GetValueOrDefault(string key, double defaultValue, string fileName = null)
        {
            try
            {
                return Preferences.Get(key, defaultValue);
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        #endregion

        #region Auxiliary Methods

        public bool OpenAppSettings() => true;

        public void Clear(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) Preferences.Clear();
            else Preferences.Clear(fileName);
        }

        public bool Contains(string key, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) return Preferences.ContainsKey(key);
            else return Preferences.ContainsKey(key, fileName);
        }

        public void Remove(string key, string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName)) Preferences.Remove(key);
            else Preferences.Remove(key, fileName);
        }

        #endregion
    }
}
