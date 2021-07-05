using UnityEngine;
using UtilityScripts;

public class RaidChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from Raid Damage";
    public override string description => "Mana Orbs from Raid Damage";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Raid_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(PartySignals.PARTY_RAID_DAMAGE_CHAOS_ORB, OnRaidDamageProduceChaosOrb);
    }
    private void OnRaidDamageProduceChaosOrb(Character character) {
        int orbsCount = 1;
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, orbsCount, character.gridTileLocation.parentMap);
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + character.name + "] - [OnRaidDamageProduceChaosOrb] - [" + orbsCount + "]");
#endif
    }
}