using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyGames.TreeLib
{
    internal static class TreeExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            return ascending ? source.OrderBy(selector) : source.OrderByDescending(selector);
        }
    }
}