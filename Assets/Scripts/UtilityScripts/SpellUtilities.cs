using UnityEngine;
namespace UtilityScripts {
    public static class SpellUtilities {

        public static int GetModifiedSpellCost(int p_baseCost, float p_modification) {
            if (p_baseCost == -1) {
                return p_baseCost;
            } else {
                return Mathf.CeilToInt(p_baseCost * p_modification);
            }
        }
        public static string GetDisplayOfCurrentChargesWithBonusChargesNotCombined(int charges, int maxCharges, int bonusCharges, bool showCharges) {
            string str = string.Empty;
            if (showCharges) {
                str += $"{charges}/{maxCharges} {Utilities.ChargesIcon()}";
            }
            if (bonusCharges > 0) {
                if(str != string.Empty) {
                    str += $" + ";
                }
                str += $"{bonusCharges} {Utilities.BonusChargesIcon()}";
            }
            return str;
        }
    }
}