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
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, UnityEngine.Random.Range(3, 5), character.gridTileLocation.parentMap);
    }
}