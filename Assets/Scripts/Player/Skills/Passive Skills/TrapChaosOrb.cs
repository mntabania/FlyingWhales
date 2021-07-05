using UnityEngine;
using UtilityScripts;

public class TrapChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from Trap";
    public override string description => "Mana Orbs from Trap";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Trap_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Character>(PlayerSkillSignals.ON_TRAP_ACTIVATED_ON_VILLAGER, OnTrapAvtivated);
    }
    private void OnTrapAvtivated(Character character) {
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + character.name + "] - [OnTrapAvtivated] - [2]");
#endif
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 2, character.gridTileLocation.parentMap);
    }
}