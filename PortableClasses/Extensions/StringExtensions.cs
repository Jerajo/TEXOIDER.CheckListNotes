namespace PortableClasses.Extensions
{
    public static class StringExtensions
    {
        #region Get

        #region Overloads

        /// <summary>
        /// Returns the last split inside the text.
        /// </summary>
        public static string GetLastSplit(this string value, char separator) =>
            GetLastSplit(value, new char[] { separator });

        /// <summary>
        /// Returns the last split inside the text.
        /// </summary>
        public static string GetLastSplit(this string value, string separator) =>
            GetLastSplit(value, separator.ToCharArray());

        /// <summary>
        /// Returns the first split inside the text.
        /// </summary>
        public static string GetFistSplit(this string value, char separator) =>
            GetFistSplit(value, new char[] { separator });

        /// <summary>
        /// Returns the first split inside the text.
        /// </summary>
        public static string GetFistSplit(this string value, string separator) =>
            GetFistSplit(value, separator.ToCharArray());

        #endregion

        /// <summary>
        /// Returns the last split inside the text.
        /// </summary>
        public static string GetLastSplit(this string value, char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = (stringSplited.Length > 0) ? stringSplited[stringSplited.Length - 1] : value;
            return stringShorted;
        }

        /// <summary>
        /// Returns the first split inside the text.
        /// </summary>
        public static string GetFistSplit(this string value, char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = (stringSplited.Length > 0) ? stringSplited[0] : value;
            return stringShorted;
        }

        #endregion

        #region Remove

        #region Overloads

        /// <summary>
        /// Returns the string without the last split text.
        /// </summary>
        public static string RemoveLastSplit(this string value, char separator) =>
            RemoveLastSplit(value, new char[] { separator });

        /// <summary>
        /// Returns the string without the last split text.
        /// </summary>
        public static string RemoveLastSplit(this string value, string separator) =>
            RemoveLastSplit(value, separator.ToCharArray());

        /// <summary>
        /// Returns the string without the first split text.
        /// </summary>
        public static string RemoveFistSplit(this string value, char separator) =>
            RemoveFistSplit(value, new char[] { separator });

        /// <summary>
        /// Returns the string without the first split text.
        /// </summary>
        public static string RemoveFistSplit(this string value, string separator) =>
            RemoveFistSplit(value, separator.ToCharArray());

        #endregion

        /// <summary>
        /// Returns the string without the last split text.
        /// </summary>
        public static string RemoveLastSplit(this string value, char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = "";
            for (var i = 0; i < stringSplited.Length - 1; i++)
                stringShorted += stringSplited[i];
            return stringShorted;
        }

        /// <summary>
        /// Returns the string without the first split text.
        /// </summary>
        public static string RemoveFistSplit(this string value, char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = "";
            for (var i = 1; i < stringSplited.Length; i++) stringShorted += stringSplited[i];
            return stringShorted;
        }

        #endregion
    }
}
