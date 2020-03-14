using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UnityEngine.Assertions;

public class BlizzardFeature : TileFeature {

    private List<Character> _charactersOutside;
    private string _currentFreezingCheckSchedule;
    //private BlizzardParticleEffect _effect;
    private GameObject _effect;

    public BlizzardFeature() {
        name = "Blizzard";
        description = "There is a blizzard in this location.";
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
        RescheduleFreezingCheck(); //this will start the freezing check loop
        
        //schedule removal of this feature after x amount of ticks.
        GameDate expiryDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(4));
        SchedulingManager.Instance.AddEntry(expiryDate, () => tile.featureComponent.RemoveFeature(this, tile), this);
        GameObject go = GameManager.Instance.CreateParticleEffectAt(tile.GetCenterLocationGridTile(), PARTICLE_EFFECT.Blizzard);
        _effect = go; //go.GetComponent<BlizzardParticleEffect>()

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
        if (string.IsNullOrEmpty(_currentFreezingCheckSchedule) == false) {
            SchedulingManager.Instance.RemoveSpecificEntry(_currentFreezingCheckSchedule); //this will stop the freezing check loop 
        }
        //_effect.StopParticleEffect();
        ObjectPoolManager.Instance.DestroyObject(_effect);
    }
    #endregion

    #region Listeners
    private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure, HexTile featureOwner) {
        if (structure != null && structure.isInterior == false && character.gridTileLocation != null 
            && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap 
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == featureOwner) {
            AddCharacterOutside(character);
        }
    }
    private void OnCharacterLeftStructure(Character character, LocationStructure structure, HexTile featureOwner) {
        //character left a structure that was outside. If the character entered a structure that is outside. That 
        //is handled at OnCharacterArrivedAtStructure
        if (structure.isInterior == false && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap 
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == featureOwner) {
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
        }
    }
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
    private void CheckForFreezing() {
        string summary = $"{GameManager.Instance.TodayLogString()}Starting freezing check...";
        int chance = 15;
        for (int i = 0; i < _charactersOutside.Count; i++) {
            Character character = _charactersOutside[i];
            int roll = UnityEngine.Random.Range(0, 100);
            summary =
                $"{summary}\nRolling freezing check for {character.name}. Roll is {roll.ToString()}. Chance is {chance.ToString()}";
            if (roll < chance) {
                summary =
                    $"{summary}\n\tChance met for {character.name}. Adding Freezing trait...";
                character.traitContainer.AddTrait(character, "Freezing", bypassElementalChance: true);
            }
        }
        //reschedule 15 minutes after.
        RescheduleFreezingCheck();
        Debug.Log(summary);
    }
    private void RescheduleFreezingCheck() {
        GameDate dueDate = GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnMinutes(15));
        _currentFreezingCheckSchedule = SchedulingManager.Instance.AddEntry(dueDate, CheckForFreezing, this);
    }
    #endregion
}