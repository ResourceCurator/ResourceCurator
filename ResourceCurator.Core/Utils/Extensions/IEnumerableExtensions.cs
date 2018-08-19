using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Utils
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Obtains the data as a list; if it is *already* a list, the original object is returned without
        /// any duplication; otherwise, ToList() is invoked.
        /// </summary>
        public static List<T> AsList<T>(this IEnumerable<T> source) => source is List<T> list ? list : source?.ToList();

    }

    public static class SequenceHelper
    {
        /// <summary>
        /// Safe Enumerable.SequenceEqual, check ReferenceEquals с null, with another IEnumerable then sequenced equals
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <returns>Равны или нет</returns>
        public static bool SequenceEqual<T>(IEnumerable<T> x, IEnumerable<T> y)
        {
            if (ReferenceEquals(x, y))    return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return Enumerable.SequenceEqual(x, y);

        }

        /// <summary>
        /// Gets the sequence hash code for class collection.
        /// </summary>
        /// <param name="objects">The IEnumerable object</param>
        /// <returns></returns>
        public static int GetSequenceHashCode<T>(IEnumerable<T> objects) where T: class
        {
            if (ReferenceEquals(objects, null))
                return 0;

            unchecked
            {
                int collectionHash = 17;

                foreach (var item in objects)
                {
                    // I know about equals with default(T) with typeof(T).IsClass but it's so exprensive
                    collectionHash = collectionHash * 397 ^  (item?.GetHashCode() ?? 0);
                }
                return collectionHash;
            }
        }

        /// <summary>
        /// Gets the sequence hash code for struct collection.
        /// </summary>
        /// <param name="objects">The IEnumerable object</param>
        /// <returns></returns>
        public static int GetSequenceHashCodeStruct<T>(IEnumerable<T> objects) where T: struct
        {
            if (ReferenceEquals(objects, null))
                return 0;

            unchecked
            {
                int collectionHash = 17;

                foreach (var item in objects)
                {
                    collectionHash = collectionHash * 397 ^  item.GetHashCode();
                }
                return collectionHash;
            }
        }
    }
}
