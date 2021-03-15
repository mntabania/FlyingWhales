﻿using UnityEngine;
using UtilityScripts;
using Traits;
using Interrupts;

public class MentalBreakChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs upon mental break";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Mental_Break_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<InterruptHolder>(InterruptSignals.INTERRUPT_STARTED, OnMentalBreak);
    }
    private void OnMentalBreak(InterruptHolder interrupt) {
        Character character = interrupt.actor;
        if (character.faction.factionType.type != FACTION_TYPE.Demon_Cult && interrupt.interrupt.type == INTERRUPT.Mental_Break) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, UnityEngine.Random.Range(1, 3), character.gridTileLocation.parentMap, CURRENCY.Chaotic_Energy);
        }
    }
}