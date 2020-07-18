using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Golem : Summon {
    public override string raceClassName => "Golem";
    public Golem() : base(SUMMON_TYPE.Golem, "Golem", RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Hibernating");
    }
    public Golem(string className) : base(SUMMON_TYPE.Golem, className, RACE.GOLEM, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Indestructible");
        traitContainer.AddTrait(this, "Hibernating");
    }
    public Golem(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override string GetClassForRole(CharacterRole role) {
        return "Barbarian"; //all golems are barbarians
    }
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Golem_Behaviour);
    }
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    }
    //public override void OnPlaceSummon(LocationGridTile tile) {
    //    base.OnPlaceSummon(tile);
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, tile.parentAreaMap.npcSettlement);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    //protected override void IdlePlans() {
    //    base.IdlePlans();
    //    //CharacterState state = stateComponent.SwitchToState(CHARACTER_STATE.BERSERKED, null, specificLocation);
    //    //state.SetIsUnending(true);
    //    GoToWorkArea();
    //}
    //protected override void OnSeenBy(Character character) {
    //    base.OnSeenBy(character);
    //    if (traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)) {
    //        return;
    //    }
    //    if (!character.IsHostileWith(this)) {
    //        return;
    //    }
    //    //add taunted trait to the character
    //    character.traitContainer.AddTrait(character, new Taunted(), this);
    //}
    #endregion

    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
        if (character != this && combatComponent.isInCombat && homeStructure != null) {
            if (structure != homeStructure) {
                combatComponent.RemoveHostileInRange(character);
            }
        }
    }

    private void OnCharacterExitedHexTile(Character character, HexTile tile) {
        if (character != this && combatComponent.isInCombat) {
            if (HasTerritory()) {
                if (IsTerritory(tile)) {
                    bool isCharacterInStillInTerritory = character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && IsTerritory(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
                    if (!isCharacterInStillInTerritory) {
                        combatComponent.RemoveHostileInRange(character);
                    }
                }
            }
        }
    }
}