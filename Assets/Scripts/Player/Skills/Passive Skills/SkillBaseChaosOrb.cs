using UnityEngine;
using Inner_Maps;
using Traits;

public class SkillBaseChaosOrb : PassiveSkill {
    public override string name => "Mana Orbs from skills";
    public override string description => "Mana Orbs from skills";
    public override PASSIVE_SKILL passiveSkill => PASSIVE_SKILL.Skill_Base_Chaos_Orb;

    public override void ActivateSkill() {
        Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.ZAP_ACTIVATED, OnZapDone);
        Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.AGITATE_ACTIVATED, OnAgitateDone);
        Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.EXPEL_ACTIVATED, OnExpelDone);
        Messenger.AddListener<IPointOfInterest>(PlayerSkillSignals.REMOVE_BUFF_ACTIVATED, OnRemoveBuffDone);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        Messenger.AddListener<ITraitable, Trait>(TraitSignals.TRAITABLE_GAINED_TRAIT, OnTraitableGainedTrait);
    }
    private void OnZapDone(IPointOfInterest p_character) {
        Character character = p_character as Character;
        if (character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
        }
    }

    private void OnAgitateDone(IPointOfInterest p_character) {
        Character character = p_character as Character;
        if (character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
        }
    }

    private void OnRemoveBuffDone(IPointOfInterest p_character) {
        Character character = p_character as Character;
        if (character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer) {
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
        }
    }

    private void OnExpelDone(IPointOfInterest p_character) {
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
    }

    private void OnCharacterDied(Character p_character) {
        if (p_character.isNormalAndNotAlliedWithPlayer) {
            Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
            if (responsibleCharacter is Summon && responsibleCharacter.traitContainer.HasTrait("Agitated")) {
                LocationGridTile chaosOrbSpawnLocation = !p_character.hasMarker ? p_character.deathTilePosition : p_character.gridTileLocation;
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, chaosOrbSpawnLocation.centeredLocalLocation, 2, chaosOrbSpawnLocation.parentMap);
            }
        }
    }

    private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait) {

    }
}