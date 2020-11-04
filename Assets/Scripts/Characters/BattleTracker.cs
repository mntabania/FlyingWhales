using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleTracker {
    public List<ELEMENT> elementalWeaknesses;
    public List<ELEMENT> elementalResistances;
    public int lastDamageDealt;

    public BattleTracker() {
        elementalWeaknesses = new List<ELEMENT>();
        elementalResistances = new List<ELEMENT>();
        lastDamageDealt = 0;
    }

    public void AddElementalWeakness(ELEMENT element) {
        if (!elementalWeaknesses.Contains(element)) {
            elementalWeaknesses.Add(element);
        }
    }
    public void AddElementalResistance(ELEMENT element) {
        if (!elementalResistances.Contains(element)) {
            elementalResistances.Add(element);
        }
    }
}
