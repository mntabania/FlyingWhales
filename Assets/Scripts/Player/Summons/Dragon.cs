using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class Dragon : Summon {
    public override string raceClassName => "Dragon";

    public bool isAwakened { get; private set; }

    public Dragon() : base(SUMMON_TYPE.Dragon, "Dragon", RACE.DRAGON, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Immune");
        traitContainer.AddTrait(this, "Hibernating");
    }
    public Dragon(string className) : base(SUMMON_TYPE.Dragon, className, RACE.DRAGON, UtilityScripts.Utilities.GetRandomGender()) {
        //SetMaxHPMod(1000);
        visuals.SetHasBlood(false);
        traitContainer.AddTrait(this, "Immune");
        traitContainer.AddTrait(this, "Hibernating");
    }
    public Dragon(SaveDataCharacter data) : base(data) { }

    #region Overrides
    public override void Initialize() {
        base.Initialize();
        behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.Golem_Behaviour);
    }
    //public override void SubscribeToSignals() {
    //    base.SubscribeToSignals();
    //    Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
    //    Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    //}
    //public override void UnsubscribeSignals() {
    //    base.UnsubscribeSignals();
    //    Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE, OnCharacterExitedHexTile);
    //    Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
    //}
    #endregion

    public void Awaken() {
        isAwakened = true;
        traitContainer.RemoveTrait(this, "Immune");
        traitContainer.RemoveTrait(this, "Hibernating");
    }

    //private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
    //    if (character != this && combatComponent.isInCombat && homeStructure != null) {
    //        if (structure != homeStructure) {
    //            combatComponent.RemoveHostileInRange(character);
    //        }
    //    }
    //}

    //private void OnCharacterExitedHexTile(Character character, HexTile tile) {
    //    if (character != this && combatComponent.isInCombat) {
    //        if (HasTerritory()) {
    //            if (IsTerritory(tile)) {
    //                bool isCharacterInStillInTerritory = character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && IsTerritory(character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner);
    //                if (!isCharacterInStillInTerritory) {
    //                    combatComponent.RemoveHostileInRange(character);
    //                }
    //            }
    //        }
    //    }
    //}
}