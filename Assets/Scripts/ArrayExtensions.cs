using UnityEngine;

namespace AnarPerPortes
{
    public static class ArrayExtensions
    {
        public static T RandomItem<T>(this T[] array)
        {
            return array[Random.Range(0, array.Length)];
        }
    }
}
