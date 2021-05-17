using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class PlayerDamageAccumulator {
    public int accumulatedDamage { get; private set; }
    public bool activatedSpellDamageChaosOrbPassiveSkill { get; private set; }

    public PlayerDamageAccumulator() {

    }
    public PlayerDamageAccumulator(SaveDataPlayerDamageAccumulator data) {
        accumulatedDamage = data.accumulatedDamage;
        activatedSpellDamageChaosOrbPassiveSkill = data.activatedSpellDamageChaosOrbPassiveSkill;
    }
    public void SetActivatedSpellDamageChaosOrbPassiveSkill(bool p_state) {
        activatedSpellDamageChaosOrbPassiveSkill = p_state;
    }
    private void AccumulateDamage(int p_amount) {
        if(p_amount < 0) {
            p_amount *= -1;
        }
        accumulatedDamage += p_amount;
        PlayerUI.Instance.UpdateAccumulatedDamageText(accumulatedDamage);
    }
    public void AccumulateDamage(int p_amount, LocationGridTile p_expelChaosOrbsOn, Character p_character) {
        if (!activatedSpellDamageChaosOrbPassiveSkill) {
            return;
        }
        AccumulateDamage(p_amount);
        int threshold = EditableValuesManager.Instance.chaosOrbExpulsionThreshold;
        if (p_expelChaosOrbsOn != null && accumulatedDamage >= threshold) {
            int numOfChaosOrbs = accumulatedDamage / threshold;
            int subtractFromAccumulatedDamage = numOfChaosOrbs * threshold;
            accumulatedDamage -= subtractFromAccumulatedDamage;
            
            if(numOfChaosOrbs > 0) {
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnPlayerDamageDone] - [" + numOfChaosOrbs + "]");
#endif
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_expelChaosOrbsOn.centeredWorldLocation, numOfChaosOrbs, p_expelChaosOrbsOn.parentMap);
            }
        }
    }
}

[System.Serializable]
public class SaveDataPlayerDamageAccumulator : SaveData<PlayerDamageAccumulator> {
    public int accumulatedDamage;
    public bool activatedSpellDamageChaosOrbPassiveSkill;

    public override void Save(PlayerDamageAccumulator data) {
        base.Save(data);
        accumulatedDamage = data.accumulatedDamage;
        activatedSpellDamageChaosOrbPassiveSkill = data.activatedSpellDamageChaosOrbPassiveSkill;
    }
    public override PlayerDamageAccumulator Load() {
        return new PlayerDamageAccumulator(this);
    }
}