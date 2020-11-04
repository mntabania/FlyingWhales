using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This data is needed during battle, all values are saved after battles so as to help the character decide in future battles
public class CharacterBattleTracker {
    public Dictionary<string, BattleTracker> tracker;

    public CharacterBattleTracker() {
        tracker = new Dictionary<string, BattleTracker>();
    }

    public void AddEnemyElementalWeakness(string enemyName, ELEMENT weakness) {
        if (tracker.ContainsKey(enemyName)) {
            tracker[enemyName].AddElementalWeakness(weakness);
        } else {
            BattleTracker battleTracker = new BattleTracker();
            battleTracker.AddElementalWeakness(weakness);
            tracker.Add(enemyName, battleTracker);
        }
    }
    public void AddEnemyElementalResistance(string enemyName, ELEMENT resistance) {
        if (tracker.ContainsKey(enemyName)) {
            tracker[enemyName].AddElementalResistance(resistance);
        } else {
            BattleTracker battleTracker = new BattleTracker();
            battleTracker.AddElementalResistance(resistance);
            tracker.Add(enemyName, battleTracker);
        }
    }

    public void SetLastDamageDealt(string enemyName, int amount) {
        if (tracker.ContainsKey(enemyName)) {
            tracker[enemyName].lastDamageDealt = amount;
        } else {
            BattleTracker battleTracker = new BattleTracker();
            battleTracker.lastDamageDealt = amount;
            tracker.Add(enemyName, battleTracker);
        }
    }
}
