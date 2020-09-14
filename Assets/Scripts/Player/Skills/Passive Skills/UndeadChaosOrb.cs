using UnityEngine;

public class UndeadChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs on Undead Death";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Undead_Chaos_Orb;
    
    public override void ActivateSkill() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnCharacterDied(Character character) {
        if (character.faction != null && character.faction.factionType.type == FACTION_TYPE.Undead && character.marker != null) {
            Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.currentRegion.innerMap);
        }
    }
}