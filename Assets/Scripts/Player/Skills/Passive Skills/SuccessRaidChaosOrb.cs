﻿using UnityEngine;
using UtilityScripts;

public class SuccessRaidChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs after successful raid/snatch";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Player_Success_Raid_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<Party>(PartySignals.PARTY_QUEST_FINISHED_SUCCESSFULLY, OnSuccessParty);
    }
    private void OnSuccessParty(Party party) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, party.members[0].worldPosition, UnityEngine.Random.Range(1, 2), party.members[0].gridTileLocation.parentMap, CURRENCY.Chaotic_Energy);
    }
}