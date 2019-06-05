namespace PortableClasses.Extensions
{
    public static class StringExtensions
    {
        #region Get

        #region Overloads

        /// <summary>
        /// Returns the last split inside the text.
        /// </summary>
        public static string GetLastSplit(this string value, string separator) =>
            GetLastSplit(value, separator.ToCharArray());

        /// <summary>
        /// Returns the first split inside the text.
        /// </summary>
        public static string GetFistSplit(this string value, string separator) =>
            GetFistSplit(value, separator.ToCharArray());

        /// <summary>
        /// Returns all the splits inside the text betwin the start and the end value.
        /// </summary>
        public static string GetSplitRange(this string value, string separator,
            int? startValue = null, int? endValue = null) => 
            GetSplitRange(value, startValue, endValue, separator.ToCharArray());

        #endregion

        /// <summary>
        /// Returns the last split inside the text.
        /// </summary>
        public static string GetLastSplit(this string value, params char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = (stringSplited.Length > 0) ? stringSplited[stringSplited.Length - 1] : value;
            return stringShorted;
        }

        /// <summary>
        /// Returns the first split inside the text.
        /// </summary>
        public static string GetFistSplit(this string value, params char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = (stringSplited.Length > 0) ? stringSplited[0] : value;
            return stringShorted;
        }

        /// <summary>
        /// Returns all the splits inside the text betwin the start and the end value.
        /// </summary>
        public static string GetSplitRange(this string value, int? startValue = null, 
            int? endValue = null, params char[] separator)
        {
            var stringSplited = value.Split(separator);
            startValue = startValue ?? 0;
            endValue = endValue ?? stringSplited.Length;
            var stringShorted = "";
            for (var i = startValue; i < endValue; i++)
                stringShorted += (i == startValue) ? stringSplited[i.Value] : 
                    $".{stringSplited[i.Value]}";
            return stringShorted;
        }

        #endregion

        #region Remove

        #region Overloads

        /// <summary>
        /// Returns the string without the last split text.
        /// </summary>
        public static string RemoveLastSplit(this string value, string separator) =>
            RemoveLastSplit(value, separator.ToCharArray());

        /// <summary>
        /// Returns the string without the first split text.
        /// </summary>
        public static string RemoveFistSplit(this string value, string separator) =>
            RemoveFistSplit(value, separator.ToCharArray());

        #endregion

        /// <summary>
        /// Returns the string without the last split text.
        /// </summary>
        public static string RemoveLastSplit(this string value, params char[] separator)
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
        public static string RemoveFistSplit(this string value, params char[] separator)
        {
            var stringSplited = value.Split(separator);
            var stringShorted = "";
            for (var i = 1; i < stringSplited.Length; i++) stringShorted += stringSplited[i];
            return stringShorted;
        }

        #endregion
    }
}
