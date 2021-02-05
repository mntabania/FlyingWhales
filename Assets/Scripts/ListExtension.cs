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
    public static bool HasTileWithFeature(this List<HexTile> p_list, string p_featureName) {
        for (int i = 0; i < p_list.Count; i++) {
            HexTile tile = p_list[i];
            if (tile.featureComponent.HasFeature(p_featureName)) {
                return true;
            }
        }
        return false;
    }
}
