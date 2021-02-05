using System;
using TMPro;
using UnityEngine.UI;

public static class UIExtensions {
    public static int GetDropdownOptionIndex(this TMP_Dropdown p_dropDown, string p_optionName, bool ignoreCase = false) {
        for (int i = 0; i < p_dropDown.options.Count; i++) {
            TMP_Dropdown.OptionData optionData = p_dropDown.options[i];
            if (string.Equals(optionData.text, p_optionName, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal)) {
                return i;
            }
        }
        return -1;
    }
    
    public static T ConvertCurrentSelectedOption<T>(this TMP_Dropdown p_dropDown) where T : struct, IConvertible {
        if (!typeof(T).IsEnum) {
            throw new ArgumentException("T must be an enumerated type");
        }
        string currentlySelected = p_dropDown.options[p_dropDown.value].text;
        foreach (T item in Enum.GetValues(typeof(T))) {
            string normalizedItem = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(item.ToString());
            if (normalizedItem.ToLower().Equals(currentlySelected.Trim().ToLower())) {
                return item;
            }
        }
        return default;
    }
    public static T ConvertOption<T>(this TMP_Dropdown p_dropDown, int p_optionIndex) where T : struct, IConvertible {
        if (!typeof(T).IsEnum) {
            throw new ArgumentException("T must be an enumerated type");
        }
        string option = p_dropDown.options[p_optionIndex].text;
        foreach (T item in Enum.GetValues(typeof(T))) {
            string normalizedItem = UtilityScripts.Utilities.NotNormalizedConversionEnumToString(item.ToString());
            if (normalizedItem.ToLower().Equals(option.Trim().ToLower())) {
                return item;
            }
        }
        return default;
    }
}
