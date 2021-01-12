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
    }
}