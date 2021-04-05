using UnityEngine;
using UtilityScripts;

public class DarkRitualChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from Dark Ritual Action Cultists";
    public override string description => "Mana Orbs on Dark Ritual Action";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Dark_Ritual_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<GoapNode>(JobSignals.CHARACTER_FINISHED_ACTION, OnSuccessPraying);
    }
    private void OnSuccessPraying(GoapNode p_goapNode) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_goapNode.target.worldPosition, 4, p_goapNode.target.gridTileLocation.parentMap);
    }
}