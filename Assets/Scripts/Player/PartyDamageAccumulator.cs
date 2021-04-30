using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class PartyDamageAccumulator {
    public int accumulatedDamage { get; private set; }

    public PartyDamageAccumulator() { }

    public void Initialize(SaveDataPartyDamageAccumulator partyDamageAccumulator) {
        accumulatedDamage = partyDamageAccumulator.accumulatedDamage;
    }
    
    private void AccumulateDamage(int p_amount) {
        if (p_amount < 0) {
            p_amount *= -1;
        }
        accumulatedDamage += p_amount;
    }
    public void AccumulateDamage(int p_amount, Character p_character) { 
        AccumulateDamage(p_amount);
        int threshold = EditableValuesManager.Instance.chaosOrbExpulsionThresholdFromRaid;
        if (p_character != null && accumulatedDamage >= threshold) {
            int numOfChaosOrbs = accumulatedDamage / threshold;
            int subtractFromAccumulatedDamage = numOfChaosOrbs * threshold;
            accumulatedDamage -= subtractFromAccumulatedDamage;
            if (numOfChaosOrbs > 0) {
                Messenger.Broadcast(PartySignals.PARTY_RAID_DAMAGE_CHAOS_ORB, p_character);
            }
        }
    }
    public void Reset() {
        accumulatedDamage = 0;
    }
}

[System.Serializable]
public class SaveDataPartyDamageAccumulator : SaveData<PartyDamageAccumulator> {
    public int accumulatedDamage;

    public override void Save(PartyDamageAccumulator data) {
        base.Save(data);
        accumulatedDamage = data.accumulatedDamage;
    }
}
