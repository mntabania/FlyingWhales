using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class BoneGolem : Summon {
    public override string raceClassName => "Bone Golem";
    public BoneGolem() : base(SUMMON_TYPE.Bone_Golem, "Bone Golem", RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Fire Prone");
    }
    public BoneGolem(string className) : base(SUMMON_TYPE.Bone_Golem, className, RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Fire Prone");
    }
    public BoneGolem(SaveDataSummon data) : base(data) {
        visuals.SetHasBlood(false);
    }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Bone_Golem_Behaviour);
    }
    public override void SubscribeToSignals() {
        if (hasSubscribedToSignals) {
            return;
        }
        base.SubscribeToSignals();
        Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.AddListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void UnsubscribeSignals() {
        if (!hasSubscribedToSignals) {
            return;
        }
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_EXITED_AREA, OnCharacterExitedArea);
        Messenger.RemoveListener<Character, LocationStructure>(CharacterSignals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    #endregion

    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (character != this && combatComponent.isInCombat && homeStructure != null) {
            if (structure != homeStructure) {
                combatComponent.RemoveHostileInRange(character);
            }
        }
    }

    private void OnCharacterExitedArea(Character character, Area p_area) {
        if (character != this && combatComponent.isInCombat) {
            if (HasTerritory()) {
                if (IsTerritory(p_area)) {
                    bool isCharacterInStillInTerritory = IsTerritory(character.gridTileLocation.area);
                    if (!isCharacterInStillInTerritory) {
                        combatComponent.RemoveHostileInRange(character);
                    }
                }
            }
        }
    }
}