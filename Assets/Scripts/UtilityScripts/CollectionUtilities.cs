using System;
using System.Collections.Generic;
using System.Linq;
namespace UtilityScripts {
    public static class CollectionUtilities {
        public static T GetNextElementCyclic<T>(List<T> collection, int index) {
            if (index > collection.Count) {
                throw new ArgumentOutOfRangeException("Trying to get next element cyclic, but provided index is greater than the size of the collection!");
            }
            if (index == collection.Count - 1) {
                //if index provided is equal to the number of elements in the list,
                //then the next element is the first element
                return collection[0];
            }
            return collection[index + 1];
        }
        public static T GetNextElementCyclic<T>(T[] collection, int index) {
            if (index > collection.Length) {
                throw new ArgumentOutOfRangeException("Trying to get next element cyclic, but provided index is greater than the size of the collection!");
            }
            if (index == collection.Length - 1) {
                //if index provided is equal to the number of elements in the list,
                //then the next element is the first element
                return collection[0];
            }
            return collection[index + 1];
        }
        public static bool ContainsRange<T>(List<T> sourceList, List<T> otherList) {
            //this is used to check whether a list has all the values in another list
            for (int i = 0; i < otherList.Count; i++) {
                if (!sourceList.Contains(otherList[i])) {
                    return false;
                }
            }
            return true;
        }
        public static T[] GetEnumValues<T>() where T : struct {
            if (!typeof(T).IsEnum) {
                throw new ArgumentException("GetValues<T> can only be called for types derived from System.Enum", "T");
            }
            return (T[]) Enum.GetValues(typeof(T));
        }
        public static void Shuffle<T>(List<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = UtilityScripts.Utilities.Rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        public static void Shuffle<T>(List<T> list, List<T> newList) {
            newList.AddRange(list);
            int n = newList.Count;
            while (n > 1) {
                n--;
                int k = UtilityScripts.Utilities.Rng.Next(n + 1);
                T value = newList[k];
                newList[k] = newList[n];
                newList[n] = value;
            }
        }
        public static void ListRemoveRange<T>(List<T> sourceList, List<T> itemsToRemove) {
            for (int i = 0; i < itemsToRemove.Count; i++) {
                T currItem = itemsToRemove[i];
                sourceList.Remove(currItem);
            }
        }
        public static void RemoveElements<T>(List<T> sourceList, T[] elementsToRemove) {
            for (int i = 0; i < sourceList.Count; i++) {
                T currElement = sourceList[i];
                if (elementsToRemove.Contains(currElement)) {
                    sourceList.RemoveAt(i);
                    i--;
                }
            }
        }
        /// <summary>
        /// Get a random index from the given list.
        /// This will return -1 if the list has no elements.
        /// </summary>
        /// <param name="list">The sample list.</param>
        /// <returns>An integer.</returns>
        public static int GetRandomIndexInList<T>(List<T> list) {
            if (list == null || list.Count == 0) {
                return -1;
            }
            return UtilityScripts.Utilities.Rng.Next(0, list.Count);
        }
        public static T GetRandomElement<T>(List<T> list) {
            if (list == null || list.Count == 0) {
                return default;
            }
            return list[UtilityScripts.Utilities.Rng.Next(0, list.Count)];
        }
        public static T GetRandomElement<T>(T[] list) {
            if (list == null || list.Length == 0) {
                return default;
            }
            return list[UtilityScripts.Utilities.Rng.Next(0, list.Length)];
        }
        public static T GetRandomElement<T>(IEnumerable<T> list) {
            //var enumerable = list.ToList();
            if(list == null) {
                return default;
            }
            int count = list.Count();
            if(count <= 0) {
                return default;
            }
            return list.ElementAt(UtilityScripts.Utilities.Rng.Next(0, count));
        }
        public static List<T> GetRandomElements<T>(List<T> list, int count) {
            int elementsToGet = count;
            if (list.Count < elementsToGet) {
                //list count is less than needed elements to get, just return the provided list
                return list;
            }
            List<T> randomElements = new List<T>();
            List<T> choices = new List<T>(list);
            for (int i = 0; i < elementsToGet; i++) {
                T chosen = GetRandomElement(choices);
                choices.Remove(chosen);
                randomElements.Add(chosen);
            }
            return randomElements;
        }
        public static bool IsLastIndex<T>(List<T> list, int index) {
            if (index + 1 == list.Count) {
                return true;
            }
            return false;
        }
    }
}