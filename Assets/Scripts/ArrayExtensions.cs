using System.Collections.Generic;
using UnityEngine;

namespace AnarPerPortes
{
    public static class ArrayExtensions
    {
        public static T RandomItem<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }

        public static T RandomItem<T>(this List<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }
    }
}
