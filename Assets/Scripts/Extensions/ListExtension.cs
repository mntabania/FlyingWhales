using System.Collections.Generic;

public static class ListExtension {
    public static string ComafyList<T>(this List<T> p_list) {
        string converted = string.Empty;
        for (int i = 0; i < p_list.Count; i++) {
            T element = p_list[i];
            converted = $"{converted}{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(element.ToString())}";
            if (p_list.IsSecondToTheLastIndex(i)) {
                converted = $"{converted} and ";
            } else if (!p_list.IsLastIndex(i)) {
                converted = $"{converted}, ";    
            }
        }
        return converted;
    }
    public static bool IsLastIndex<T>(this List<T> p_list, int p_index) {
        return p_list.Count - 1 == p_index;
    }
    public static bool IsSecondToTheLastIndex<T>(this List<T> p_list, int p_index) {
        return p_list.Count - 2 == p_index;
    }
    public static bool IsIndexInList<T>(this List<T> p_list, int p_index) {
        return p_index >= 0 && p_index < p_list.Count;
    }
    /// <summary>
    /// Shuffles the element order of the specified list.
    /// </summary>
    public static void Shuffle<T>(this IList<T> ts) {
        var count = ts.Count;
        var last = count - 1;
        for (var i = 0; i < last; ++i) {
            var r = UnityEngine.Random.Range(i, count);
            var tmp = ts[i];
            ts[i] = ts[r];
            ts[r] = tmp;
        }
    }
    public static bool HasTileWithFeature(this List<Area> p_list, string p_featureName) {
        for (int i = 0; i < p_list.Count; i++) {
            Area tile = p_list[i];
            if (tile.featureComponent.HasFeature(p_featureName)) {
                return true;
            }
        }
        return false;
    }
    public static void ListRemoveRange<T>(this List<T> sourceList, List<T> itemsToRemove) {
        for (int i = 0; i < itemsToRemove.Count; i++) {
            T currItem = itemsToRemove[i];
            sourceList.Remove(currItem);
        }
    }
    public static bool HasValueInListUntilIndex<T>(this List<T> sourceList, int p_index, T value) {
        int loopCount = p_index + 1;
        for (int i = 0; i < loopCount; i++) {
            T currentValue = sourceList[i];
            if (currentValue.Equals(value)) {
                return true;
            }
        }
        return false;
    }
}

public static class ArrayExtensions {
    public static string ComafyList<T>(this T[] p_list) {
        string converted = string.Empty;
        for (int i = 0; i < p_list.Length; i++) {
            T element = p_list[i];
            converted = $"{converted}{UtilityScripts.Utilities.NotNormalizedConversionEnumToString(element.ToString())}";
            if (p_list.IsSecondToTheLastIndex(i)) {
                converted = $"{converted} and ";
            } else if (!p_list.IsLastIndex(i)) {
                converted = $"{converted}, ";    
            }
        }
        return converted;
    }
    public static bool IsSecondToTheLastIndex<T>(this T[] p_list, int p_index) {
        return p_list.Length - 2 == p_index;
    }
    public static bool IsLastIndex<T>(this T[] p_list, int p_index) {
        return p_list.Length - 1 == p_index;
    }
    public static bool IsIndexInArray<T>(this T[] p_list, int p_index) {
        return p_index >= 0 && p_index < p_list.Length;
    }
}