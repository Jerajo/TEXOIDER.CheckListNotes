namespace CheckListNotes
{
    using CheckListNotes.Models;
    public class AppResourcesLisener : BaseModel
    {
        #region Atributes

        public const string Language = "Language";
        public const string Theme = "Theme";

        #endregion

        #region Intance

        private static AppResourcesLisener current;
        public static AppResourcesLisener Current
        {
            get => current ?? (current = new AppResourcesLisener());
        }
        public string this[string index]
        {
            get => App.Current.Resources[index].ToString();
            set => App.Current.Resources[index] = value;
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
