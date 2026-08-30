// SPDX-License-Identifier: MIT
// Copyright (c) 2021–2025 Christian Pistor

using System.Collections.Generic;

namespace TimeLiner.Common
{
    /// <summary>
    /// Extensions for System.Collections.Generic.List.
    /// </summary>
    internal static class ListExtensions
    {
        /// <summary>
        /// Move the list item at the specified index to a new location.
        /// </summary>
        public static void Move<T>(this List<T> list, int oldIndex, int newIndex)
        {
            T item = list[oldIndex];
            list.RemoveAt(oldIndex);
            list.Insert(newIndex, item);
        }
    }
}
