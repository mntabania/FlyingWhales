﻿using UnityEngine;
using UtilityScripts;

public class UndeadChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs on Undead Death";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Undead_Chaos_Orb;
    
    public override void ActivateSkill() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnCharacterDied(Character character) {
        if (character.faction != null && character.faction.factionType.type == FACTION_TYPE.Undead && character.marker != null) {
            bool shouldCreateChaosOrbs = true;
            if (character.characterClass.className == "Zombie") {
                //Note: add chance to create chaos orbs when character is Zombie, because this passive skill can become OP
                //if chaos orbs are created every time a Zombie dies since Zombies can reanimate, then die again.
                shouldCreateChaosOrbs = GameUtilities.RollChance(35);
            }
            if (shouldCreateChaosOrbs) {
                Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.currentRegion.innerMap);    
            }
        }
    }
}