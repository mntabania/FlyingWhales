using UnityEngine;

public class EnemiesChaosOrb : PassiveSkill {
    public override string name => "Gain Mana Orbs from feuds";
    public override string description => "Mana Orbs on become Enemies or Rivals";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Enemies_Chaos_Orb;
    
    public override void ActivateSkill() {
        Messenger.AddListener<Character, Character, string>(CharacterSignals.OPINION_LABEL_DECREASED, OnOpinionLabelChanged);
    }
    private void OnOpinionLabelChanged(Character character, Character targetCharacter, string newOpinion) {
        if (newOpinion == RelationshipManager.Enemy && character.hasMarker) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.currentRegion.innerMap);
        } else if (newOpinion == RelationshipManager.Rival && character.hasMarker) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 2, character.currentRegion.innerMap);
        }
    }
}