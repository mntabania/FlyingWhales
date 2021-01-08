using System.Collections.Generic;

public static class ListExtension {
    public static string ComafyList<T>(this List<T> p_list) {
        string converted = string.Empty;
        for (int i = 0; i < p_list.Count; i++) {
            T element = p_list[i];
            converted = $"{converted}{element.ToString()}";
            if (!p_list.IsLastIndex(i)) {
                converted = $"{converted}, ";    
            }
        }
        return converted;
    }
    public static bool IsLastIndex<T>(this List<T> p_list, int p_index) {
        return p_list.Count - 1 == p_index;
    }
    public static bool IsIndexInList<T>(this List<T> p_list, int p_index) {
        return p_index >= 0 && p_index < p_list.Count;
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
