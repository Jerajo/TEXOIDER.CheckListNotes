namespace CheckListNotes
{
    using Xamarin.Forms;
    using CheckListNotes.Models;
    public class AppResourcesLisener : BaseModel
    {
        #region Intance

        private static AppResourcesLisener current;
        public static AppResourcesLisener Current 
        {
            get => current ?? (current = new AppResourcesLisener());
        }

        #endregion

        #region GETTERS AND SETTERS

        public const string Language = "Language";
        public const string Theme = "Theme";

        public ResourceDictionary Resources
        {
            get => Application.Current.Resources;
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            current = null;
        }

        #endregion
    }
}
