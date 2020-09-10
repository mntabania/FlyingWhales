using UnityEngine;

public class MonsterChaosOrb : PassiveSkill {
    public override string name => "Chaos Orbs on Monster Death";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Monster_Chaos_Orb;
    
    public override void ActivateSkill() {
        Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
    }
    private void OnCharacterDied(Character character) {
        if (character is Summon && character.faction != null && character.faction.isPlayerFaction && character.marker != null) {
            Messenger.Broadcast(Signals.CREATE_CHAOS_ORBS, character.worldPosition, Random.Range(2, 6), character.currentRegion.innerMap);
        }
    }
}
