using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class PlayerDamageAccumulator {
    public int accumulatedDamage { get; private set; }
    public bool activatedSpellDamageChaosOrbPassiveSkill { get; private set; }

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
    public void AccumulateDamage(int p_amount, LocationGridTile p_expelChaosOrbsOn) {
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
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_expelChaosOrbsOn.centeredWorldLocation, numOfChaosOrbs, p_expelChaosOrbsOn.parentMap);
            }
        }
    }
}
