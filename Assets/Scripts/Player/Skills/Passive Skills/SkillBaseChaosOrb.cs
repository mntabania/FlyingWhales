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
        Messenger.AddListener<Character, GoapPlanJob>(CharacterSignals.CHARACTER_FINISHED_JOB_SUCCESSFULLY, OnCharacterFinishedAction);
    }
    private void OnZapDone(IPointOfInterest p_character) {
        Character character = p_character as Character;
        if (character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer) {
#if DEBUG_LOG
            Debug.Log("Chaos Orb Produced - [" + character.name + "] - [Zap] - [1]");
#endif
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
        }
    }

    private void OnAgitateDone(IPointOfInterest p_character) {
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [On Agitate] - [1]");
#endif
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
    }

    private void OnRemoveBuffDone(IPointOfInterest p_character) {
        Character character = p_character as Character;
        if (character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer) {
#if DEBUG_LOG
            Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnRemoveBuffDone] - [1]");
#endif
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
        }
    }

    private void OnExpelDone(IPointOfInterest p_character) {
#if DEBUG_LOG
        Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnExpelDone] - [1]");
#endif
        Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 1, p_character.gridTileLocation.parentMap);
    }

    private void OnCharacterDied(Character p_character) {
        if (p_character.isNormalAndNotAlliedWithPlayer) {
            Character responsibleCharacter = p_character.traitContainer.GetTraitOrStatus<Trait>("Dead").responsibleCharacter;
            if (responsibleCharacter is Summon && responsibleCharacter.behaviourComponent.isAgitated) {
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [killed by agitated] - [2]");
#endif
                LocationGridTile chaosOrbSpawnLocation = !responsibleCharacter.hasMarker ? responsibleCharacter.deathTilePosition : responsibleCharacter.gridTileLocation;
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, chaosOrbSpawnLocation.centeredWorldLocation, 2, chaosOrbSpawnLocation.parentMap);
            }
        }
    }

    private void OnTraitableGainedTrait(ITraitable p_traitable, Trait p_trait) {
        Character character = p_traitable as Character;
        if (character != null && character.race.IsSapient() && character.isNormalAndNotAlliedWithPlayer) {
            if (p_trait.name == "Starving" || p_trait.name == "Sulking" || p_trait.name == "Exhausted") {
#if DEBUG_LOG
                Debug.Log("Chaos Orb Produced - [" + character.name + "] - [Sulking Starving Exhausted] - [1]");
#endif
                Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, character.worldPosition, 1, character.gridTileLocation.parentMap);
            }
        }
    }

    private void OnCharacterFinishedAction(Character p_character, GoapPlanJob p_job) {
        if (p_job.isTriggeredFlaw) {
#if DEBUG_LOG
            Debug.Log("Chaos Orb Produced - [" + p_character.name + "] - [OnCharacterFinishedAction - trigger flaw] - [2]");
#endif
            Messenger.Broadcast(PlayerSignals.CREATE_CHAOS_ORBS, p_character.worldPosition, 2, p_character.gridTileLocation.parentMap);
        }
    }
}