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
}
