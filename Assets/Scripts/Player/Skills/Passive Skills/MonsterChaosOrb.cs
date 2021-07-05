using UnityEngine;

public class MonsterChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from Monster Deaths";
    public override string description => "Mana Orbs on Monster Death";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Monster_Chaos_Orb;
    
    public override void ActivateSkill() {
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnCharacterDied(Character character) {
        if (character is Summon && character.faction != null && character.faction.isPlayerFaction && character.hasMarker && !character.destroyMarkerOnDeath) {
            //Note: Do not create chaos orbs if marker is set to be destroyed on death, this is because we assume that if the characters marker should be destroyed on death
            //then it is because itt died due to special circumstances (i.e. Small Spider growing up into Giant Spider, Dragon disappearing, etc.)
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.currentRegion.innerMap);
        }
    }
}
