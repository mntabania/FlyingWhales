using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is only used during battles, all values will be reset every battle
public class CharacterBattleOnlyTracker {
    public float hpLostPercent;
    public int lastDamageTaken;
    public Dictionary<string, int> consecutiveAttackMisses;

    public CharacterBattleOnlyTracker() {
        consecutiveAttackMisses = new Dictionary<string, int>();
    }
    public void Reset() {
        hpLostPercent = 0f;
        lastDamageTaken = 0;
        consecutiveAttackMisses.Clear();
    }

    public void AddAttackMiss(string skillName, int amount) {
        if (consecutiveAttackMisses.ContainsKey(skillName)) {
            consecutiveAttackMisses[skillName] += amount;
        } else {
            consecutiveAttackMisses.Add(skillName, amount);
        }
    }
    public void ResetAttackMiss(string skillName) {
        if (consecutiveAttackMisses.ContainsKey(skillName)) {
            consecutiveAttackMisses[skillName] = 0;
        }
    }
}
