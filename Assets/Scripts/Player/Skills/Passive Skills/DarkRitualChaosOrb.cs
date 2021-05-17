using UnityEngine;
using UtilityScripts;

public class DarkRitualChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from Dark Ritual Action Cultists";
    public override string description => "Mana Orbs on Dark Ritual Action";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Dark_Ritual_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<ActualGoapNode>(JobSignals.ON_FINISH_PRAYING, OnSuccessPraying);
    }
    private void OnSuccessPraying(ActualGoapNode p_goapNode) {
        if (p_goapNode.goapType == INTERACTION_TYPE.DARK_RITUAL) {
#if DEBUG_LOG
            Debug.Log("Chaos Orb Produced - [" + p_goapNode.target.name + "] - [OnSuccessPraying - Dark Ritual] - [4]");
#endif
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_goapNode.target.worldPosition, 4, p_goapNode.target.gridTileLocation.parentMap);
        }
    }
}