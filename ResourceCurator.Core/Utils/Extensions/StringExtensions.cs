using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;

namespace Utils
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Contains with <see cref="StringComparison"/>
        /// </summary>
        /// <param name="source">string where</param>
        /// <param name="str">string to check</param>
        /// <param name="comparison">type of comparison</param>
        /// <returns></returns>
        public static bool Contains(this string source, string str, StringComparison comparison)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (str == null)
                throw new ArgumentNullException(nameof(str));

            if (source.Length < str.Length || str.Length <= 0 || str.Length <= 0)
                return false;

            return source.IndexOf(str, comparison) >= 0;
        }


        public static string ToSafeFilename(this string path, string replacement = "_") => string.Join(replacement, path.Split(Path.GetInvalidFileNameChars()));

    }


}
