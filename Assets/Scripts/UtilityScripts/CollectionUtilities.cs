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
        public static List<T> Shuffle<T>(List<T> list) {
            List<T> newList = new List<T>(list);
            int n = newList.Count;
            while (n > 1) {
                n--;
                int k = UtilityScripts.Utilities.Rng.Next(n + 1);
                T value = newList[k];
                newList[k] = newList[n];
                newList[n] = value;
            }
            return newList;
        }
        public static void ListRemoveRange<T>(List<T> sourceList, List<T> itemsToRemove) {
            for (int i = 0; i < itemsToRemove.Count; i++) {
                T currItem = itemsToRemove[i];
                sourceList.Remove(currItem);
            }
        }
        public static string GetDictionaryLog<T, V>(Dictionary<T, V> dict) {
            string log = String.Empty;
            if (dict == null) {
                log = "Null dictionary";
            } else {
                foreach (KeyValuePair<T, V> kvp in dict) {
                    log += $"{kvp.Key.ToString()} - {kvp.Value.ToString()}";
                }    
            }
            
            return log;
        }
        public static T[] CreateCopyOfArray<T>(T[] sourceArray) {
            T[] copy = new T[sourceArray.Length];
            for (int i = 0; i < sourceArray.Length; i++) {
                copy[i] = sourceArray[i];
            }
            return copy;
        }
        public static List<T> RemoveElements<T>(ref List<T> sourceList, List<T> elementsToRemove) {
            List<T> newList = new List<T>();
            for (int i = 0; i < sourceList.Count; i++) {
                T currElement = sourceList[i];
                if (elementsToRemove.Contains(currElement)) {
                    continue; //do not add that element to new list
                }
                newList.Add(currElement);
            }
            return newList;
        }
        public static List<T> RemoveElements<T>(ref List<T> sourceList, T[] elementsToRemove) {
            List<T> newList = new List<T>();
            for (int i = 0; i < sourceList.Count; i++) {
                T currElement = sourceList[i];
                if (elementsToRemove.Contains(currElement)) {
                    continue; //do not add that element to new list
                }
                newList.Add(currElement);
            }
            return newList;
        }
        /// <summary>
        /// Get a random index from the given list.
        /// This will return -1 if the list has no elements.
        /// </summary>
        /// <param name="list">The sample list.</param>
        /// <returns>An integer.</returns>
        public static int GetRandomIndexInList<T>(List<T> list) {
            if (list.Count == 0) {
                return -1;
            }
            return UtilityScripts.Utilities.Rng.Next(0, list.Count);
        }
        public static T GetRandomElement<T>(List<T> list) {
            if (list.Count == 0) {
                return default;
            }
            return list[UtilityScripts.Utilities.Rng.Next(0, list.Count)];
        }
        public static T GetRandomElement<T>(T[] list) {
            if (list.Length == 0) {
                return default;
            }
            return list[UtilityScripts.Utilities.Rng.Next(0, list.Length)];
        }
        public static T GetRandomElement<T>(IEnumerable<T> list) {
            var enumerable = list.ToList();
            return enumerable.ElementAt(UtilityScripts.Utilities.Rng.Next(0, enumerable.Count()));
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
    }
}