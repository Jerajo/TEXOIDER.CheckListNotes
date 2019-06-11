using System;
using Windows.System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Foundation.Collections;

namespace PortableClasses.Services
{
    public class TonePicker : IDisposable
    {
        #region Atributes

        private ToneFilters? toneFilter = ToneFilters.None;
        private string typeFilter = TypeFilters.None;
        private LauncherOptions options;
        private LaunchUriResult result;
        private ValueSet inputData;
        private bool? isDisposing;

        #endregion

        public TonePicker(string defoultTone, ToneFilters toneFilter) : this()
        {
            DefoultTone = defoultTone;
            ToneFilter = toneFilter;
        }
        public TonePicker()
        {
            isDisposing = false;
            options = new LauncherOptions();
            options.TargetApplicationPackageFamilyName = "Microsoft.Tonepicker_8wekyb3d8bbwe";
            inputData = new ValueSet()
            {
                { "Action", "PickRingtone" }
            };
        }

        #region SETTERS AND GETTERS

        public string DefoultTone { get; set; }

        public ToneFilters? ToneFilter
        {
            get => toneFilter;
            set
            {
                toneFilter = value;
                switch (toneFilter)
                {
                    case ToneFilters.Alarms:
                        typeFilter = TypeFilters.Alarms;
                        break;
                    case ToneFilters.Ringtones:
                        typeFilter = TypeFilters.Ringtones;
                        break;
                    case ToneFilters.Notifications:
                        typeFilter = TypeFilters.Notifications;
                        break;
                    case ToneFilters.None: default:
                        typeFilter = TypeFilters.None;
                        break;
                }
            }
        }

        #endregion

        #region Methods

        public Task<Dictionary<string, string>> Pick(string defoultTone, ToneFilters toneFilter)
        {
            DefoultTone = defoultTone;
            ToneFilter = toneFilter;
            return Pick();
        }

        public Task<Dictionary<string, string>> Pick()
        {
            if (!string.IsNullOrEmpty(DefoultTone))
                inputData.Add("CurrentToneFilePath", DefoultTone);
            if (typeFilter != TypeFilters.None)
                inputData.Add("TypeFilter", typeFilter);

            result = Launcher.LaunchUriForResultsAsync(new Uri("ms-tonepicker:"), options, inputData).GetResults();

            var dictionary = new Dictionary<string, string>();
            if (result.Status == LaunchUriStatus.Success)
            {
                Int32 resultCode = (Int32)result.Result["Result"];
                if (resultCode == 0)
                {
                    dictionary["ToneToken"] = result.Result["ToneToken"] as string;
                    dictionary["DisplayName"] = result.Result["DisplayName"] as string;
                }
            }
            return Task.FromResult(dictionary);
        }

        #endregion

        #region Dispose

        ~TonePicker()
        {
            if (isDisposing == false) Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            this.isDisposing = isDisposing;
            if (isDisposing == true) Dispose();
        }

        public void Dispose()
        {
            typeFilter = null;
            toneFilter = null;
            options = null;
            inputData = null;
            DefoultTone = null;
            result = null;
            isDisposing = null;
#if DEBUG
            Debug.WriteLine("Object destroyed: { Id: {1}, Name: {0} }.", this.GetHashCode(), nameof(TonePicker));
#endif
        }

        #endregion
    }

    #region Auxiliary classes

    public enum ToneFilters
    {
        None = 0,
        Alarms = 1,
        Ringtones = 2,
        Notifications = 3
    }

    public struct TypeFilters
    {
        public const string None = "None";
        public const string Alarms = "Alarms";
        public const string Ringtones = "Ringtones";
        public const string Notifications = "Notifications";
    }
    
    #endregion
}

#region Imlementation

/* /---------------------/ Call Tone picker /---------------------/
 * 1.0) Check the device on Xamarin forms
 * if (Device.CurrentPlatform != Device.UWP) return;
 * 
 * 1.1) Check if tone picker is avalible on device
 * var status = await Launcher.QueryUriSupportAsync(new Uri("ms-tonepicker:"),     
 *                                      LaunchQuerySupportType.UriForResults,
 *                                      "Microsoft.Tonepicker_8wekyb3d8bbwe");
 * 
 * 1.2) If is avalible call it
 * if (status == LaunchQuerySupportStatus.Available)
 * {
 *     using (var tonePicker = new TonePicker())
 *     {
 *         Dictionary<string, string> resoult;
 *         resoult = tonePicker.Pick("x-ms-ring:", ToneFilters.Alarms);
 *         
 *         if (resoult.Contains("ToneToken"))
 *             Config.Current.ToastSound = resoult["ToneToken"];
 *     }
 * }
 */

#endregion
