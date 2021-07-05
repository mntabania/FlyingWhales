using UnityEngine;
using UtilityScripts;
using Traits;
using Interrupts;

public class MentalBreakChaosOrb : PassiveSkill {
    public override string name => "Gain Chaos Orbs from Mental Breaks";
    public override string description => "This is a Passive Skill that produces Chaos Orbs whenever a Villager enters a Mental Break.";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Mental_Break_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnDarkRitualStarted);
    }
    private void OnDarkRitualStarted(InterruptHolder interrupt) {
        Character character = interrupt.actor;
        if (character.faction.factionType.type != FACTION_TYPE.Demon_Cult && interrupt.interrupt.type == INTERRUPT.Mental_Break) {
            int numChaosOrbs = GameUtilities.RandomBetweenTwoNumbers(1, 3);
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, numChaosOrbs, character.gridTileLocation.parentMap);
#if DEBUG_LOG
            Debug.Log("Chaos Orb Produced - [" + character.name + "] - [OnDarkRitualStarted] - [" + numChaosOrbs + "]");
#endif
        }
    }
}