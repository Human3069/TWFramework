using System.Collections.Generic;
using UnityEngine;

namespace _TW_Framework
{
    public static class ExtentionMethods
    {
        public static void Swap<T>(this List<T> _list, int from, int to)
        {
            T tmp = _list[from];
            _list[from] = _list[to];
            _list[to] = tmp;
        }

        public static void ChangeIndex<T>(this List<T> _list, int oldIndex, int newIndex)
        {
            T item = _list[oldIndex];

            _list.RemoveAt(oldIndex);
            _list.Insert(newIndex, item);
        }

        public static void SortBySiblingIndex<T>(this List<T> oldList)
        {
            List<T> tempList = new List<T>();
            for (int i = 0; i < oldList.Count; i++)
            {
                tempList.Add(oldList[i]);
            }

            for (int i = 0; i < oldList.Count; i++)
            {
                T element = oldList[i];
                if (element is not MonoBehaviour)
                {
                    throw new System.Exception("");
                }

                MonoBehaviour monoElement = element as MonoBehaviour;

                int _siblingIndex = monoElement.transform.GetSiblingIndex();
                tempList[_siblingIndex] = element;
            }

            for (int i = 0; i <  tempList.Count; i++)
            {
                oldList[i] = tempList[i];
            }
        }
    }
}