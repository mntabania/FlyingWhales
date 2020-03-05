using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;
using Inner_Maps;
using Traits;

public class RainFeature : TileFeature {

    private List<Character> _charactersOutside;
    private string _currentRainCheckSchedule;
    private GameObject _effect;

    public RainFeature() {
        name = "Rain";
        description = "Rain is pouring down in this location.";
        type = REGION_FEATURE_TYPE.PASSIVE;
        _charactersOutside = new List<Character>();
    }

    #region Override
    public override void OnAddFeature(HexTile tile) {
        base.OnAddFeature(tile);
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE,
            (character, structure) => OnCharacterArrivedAtStructure(character, structure, tile));
        Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_LEFT_STRUCTURE,
            (character, structure) => OnCharacterLeftStructure(character, structure, tile));
        Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE,
            (character, hexTile) => OnCharacterLeftHexTile(character, hexTile, tile));
        Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE,
            (character, hexTile) => OnCharacterEnteredHexTile(character, hexTile, tile));
        //Messenger.AddListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED,
        //    (character, gridTile) => OnTileObjectPlaced(character, gridTile, tile));

        PopulateInitialCharactersOutside(tile);
        RescheduleRainCheck(tile); //this will start the rain check loop
        //CheckForWet(tile);

        //schedule removal of this feature after x amount of ticks.
        GameDate expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
        SchedulingManager.Instance.AddEntry(expiryDate, () => tile.featureComponent.RemoveFeature(this, tile), this);
        GameObject go = GameManager.Instance.CreateParticleEffectAt(tile.GetCenterLocationGridTile(), PARTICLE_EFFECT.Rain);
        _effect = go;

    }
    public override void OnRemoveFeature(HexTile tile) {
        base.OnRemoveFeature(tile);
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE,
            (character, structure) => OnCharacterArrivedAtStructure(character, structure, tile));
        Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_LEFT_STRUCTURE,
            (character, structure) => OnCharacterLeftStructure(character, structure, tile));
        Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_EXITED_HEXTILE,
            (character, hexTile) => OnCharacterLeftHexTile(character, hexTile, tile));
        Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE,
            (character, hexTile) => OnCharacterEnteredHexTile(character, hexTile, tile));
        //Messenger.RemoveListener<TileObject, LocationGridTile>(Signals.TILE_OBJECT_PLACED,
        //    (character, gridTile) => OnTileObjectPlaced(character, gridTile, tile));
        if (string.IsNullOrEmpty(_currentRainCheckSchedule) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_currentRainCheckSchedule); //this will stop the freezing check loop 
        }
        ObjectPoolManager.Instance.DestroyObject(_effect);
    }
    #endregion

    #region Listeners
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure, HexTile featureOwner) {
        if (structure != null && structure.isInterior == false && character.gridTileLocation != null 
            && character.gridTileLocation.buildSpotOwner.isPartOfParentRegionMap 
            && character.gridTileLocation.buildSpotOwner.hexTileOwner == featureOwner) {
            AddCharacterOutside(character);
            //if (!character.traitContainer.HasTrait("Wet")) {
            //    character.traitContainer.AddTrait(character, "Wet");
            //}
        }
    }
    private void OnCharacterLeftStructure(Character character, LocationStructure structure, HexTile featureOwner) {
        //character left a structure that was outside. If the character entered a structure that is outside. That 
        //is handled at OnCharacterArrivedAtStructure
        if (structure.isInterior == false && character.gridTileLocation.buildSpotOwner.isPartOfParentRegionMap
            && character.gridTileLocation.buildSpotOwner.hexTileOwner == featureOwner) {
            RemoveCharacterOutside(character);
        }
    }
    private void OnCharacterLeftHexTile(Character character, HexTile exitedTile, HexTile featureOwner) {
        if (exitedTile == featureOwner) {
            //character left the hextile that owns this feature
            RemoveCharacterOutside(character);
        }
    }
    private void OnCharacterEnteredHexTile(Character character, HexTile enteredTile, HexTile featureOwner) {
        if (enteredTile == featureOwner && character.currentStructure.isInterior == false) {
            AddCharacterOutside(character);
            //if (!character.traitContainer.HasTrait("Wet")) {
            //    character.traitContainer.AddTrait(character, "Wet");
            //}
        }
    }
    //private void OnTileObjectPlaced(TileObject obj, LocationGridTile gridTile, HexTile featureOwner) {
    //    if(gridTile.buildSpotOwner.hexTileOwner == featureOwner && gridTile != null && !gridTile.structure.isInterior) {
    //        obj.traitContainer.AddTrait(obj, "Wet");
    //    }
    //}
    #endregion

    #region Characters Outisde
    private void AddCharacterOutside(Character character) {
        Assert.IsTrue(character.currentStructure.isInterior == false,
            $"{character.name} is being added to characters outside, but isn't actually outside!");
        if (_charactersOutside.Contains(character) == false) {
            _charactersOutside.Add(character);
        }
    }
    private void RemoveCharacterOutside(Character character) {
        _charactersOutside.Remove(character);
    }
    #endregion

    #region Effects
    private void PopulateInitialCharactersOutside(HexTile hex) {
        List<Character> allCharactersInHex = hex.GetAllCharactersInsideHex();
        if (allCharactersInHex != null) {
            for (int i = 0; i < allCharactersInHex.Count; i++) {
                Character character = allCharactersInHex[i];
                if (!character.currentStructure.isInterior) {
                    AddCharacterOutside(character);
                }
            }
        }
    }
    private void CheckForWet(HexTile hex) {
        for (int i = 0; i < _charactersOutside.Count; i++) {
            Character character = _charactersOutside[i];
            character.traitContainer.AddTrait(character, "Wet");
        }
        for (int i = 0; i < hex.locationGridTiles.Count; i++) {
            LocationGridTile gridTile = hex.locationGridTiles[i];
            if (!gridTile.structure.isInterior) {
                gridTile.genericTileObject.traitContainer.AddTrait(gridTile.genericTileObject, "Wet");
                if (gridTile.objHere != null) {
                    gridTile.objHere.traitContainer.AddTrait(gridTile.objHere, "Wet");
                }
            }
        }
        RescheduleRainCheck(hex);
    }
    //private void CheckForRain() {
    //    string summary = $"{GameManager.Instance.TodayLogString()}Starting rain check...";
    //    int chance = 15;
    //    for (int i = 0; i < _charactersOutside.Count; i++) {
    //        Character character = _charactersOutside[i];
    //        int roll = UnityEngine.Random.Range(0, 100);
    //        summary =
    //            $"{summary}\nRolling rain check for {character.name}. Roll is {roll.ToString()}. Chance is {chance.ToString()}";
    //        if (roll < chance) {
    //            summary =
    //                $"{summary}\n\tChance met for {character.name}. Adding Freezing trait...";
    //            character.traitContainer.AddTrait(character, "Freezing", bypassElementalChance: true);
    //        }
    //    }
    //    //reschedule 15 minutes after.
    //    RescheduleRainCheck();
    //    Debug.Log(summary);
    //}
    private void RescheduleRainCheck(HexTile hex) {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(15));
        _currentRainCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, () => CheckForWet(hex), this);
    }
    #endregion
}