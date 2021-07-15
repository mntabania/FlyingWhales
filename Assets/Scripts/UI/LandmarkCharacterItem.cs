﻿using EZObjectPools;
using TMPro;
using UnityEngine;
using Traits;

public class LandmarkCharacterItem : PooledObject {

    public Character character { get; private set; }

    public CharacterPortrait portrait;

    private InfoUIBase _parentBase;

    [SerializeField] private RectTransform thisTrans;
    [SerializeField] private TextMeshProUGUI nameLbl;
    [SerializeField] private TextMeshProUGUI subLbl;
    [SerializeField] private GameObject travellingIcon;
    [SerializeField] private GameObject arrivedIcon;
    [SerializeField] private GameObject unrestrainedGO;
    [SerializeField] private GameObject restrainedIcon;
    [SerializeField] private GameObject coverGO;

    public void SetCharacter(Character character, InfoUIBase parentBase) {
        this.character = character;
        this._parentBase = parentBase;
        UpdateInfo();
        UpdateLocationIcons();
    }
    public void ShowCharacterInfo() {
        UIManager.Instance.ShowCharacterInfo(character);
    }
    private void UpdateInfo() {
        portrait.GeneratePortrait(character);
        nameLbl.text = character.name;
        subLbl.text = character.characterClass.className;
    }

    public void ShowItemInfo() {
        if (character == null) {
            return;
        }
        UIManager.Instance.ShowSmallInfo(character.name);
        //if (character.currentParty.characters.Count > 1) {
        //    UIManager.Instance.ShowSmallInfo(character.currentParty.name);
        //} else {
        //    UIManager.Instance.ShowSmallInfo(character.name);
        //}
    }
    public void HideItemInfo() {
        UIManager.Instance.HideSmallInfo();
    }

    private void UpdateLocationIcons() {
        if (_parentBase is TileObjectInfoUI) {
            if (character.traitContainer.HasTrait("Abducted", "Restrained")) {
                restrainedIcon.SetActive(true);
                unrestrainedGO.SetActive(false);
            } else {
                restrainedIcon.SetActive(false);
                unrestrainedGO.SetActive(true);
            }
            if (character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld) {
                travellingIcon.SetActive(true);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(true);
            }  else {
                travellingIcon.SetActive(false);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(false);
            }
        } else {
            travellingIcon.SetActive(false);
            arrivedIcon.SetActive(false);
            coverGO.SetActive(false);
        }
    }

    public void ShowTravellingTooltip() {
        //UIManager.Instance.ShowSmallInfo("Travelling to " + character.currentParty.icon.targetLocation.tileLocation.settlementOfTile.name);
        //UIManager.Instance.ShowSmallLocationInfo(character.currentParty.icon.targetLocation.tileLocation.settlementOfTile, thisTrans, new Vector3(434f, 0f, 0f), "Travelling to:");
        //if (character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld == false) {
        //    return;
        //}
        //Region showingRegion = UIManager.Instance.GetCurrentlyShowingSmallInfoLocation();
        //Region targetRegion = character.carryComponent.masterCharacter.movementComponent.targetRegionToTravelInWorld;
        //if (showingRegion == null || showingRegion != targetRegion) {
            
        //    float x = UIManager.Instance.locationSmallInfoRT.position.x;
        //    //float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
        //    UIManager.Instance.ShowSmallLocationInfo(targetRegion, new Vector3(x, thisTrans.position.y - 15f, 0f), "Travelling to:");
        //}
    }
    public void ShowArrivedTooltip() {
        //UIManager.Instance.ShowSmallInfo("Arrived at " + character.currentParty.specificLocation.name);
        //if (character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld == false) {
        //    return;
        //}
        //Region showingRegion = UIManager.Instance.GetCurrentlyShowingSmallInfoLocation();
        //Region targetRegion = character.carryComponent.masterCharacter.movementComponent.targetRegionToTravelInWorld;
        //if (showingRegion == null || showingRegion != targetRegion) {
        //    float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
        //    UIManager.Instance.ShowSmallLocationInfo(targetRegion, new Vector3(x, thisTrans.position.y - 15f, 0f), "Arrived at:");
        //}
    }
    public void ShowRestrainedTooltip() {
        string info = string.Empty;
        Trait abductedTrait = character.traitContainer.GetTraitOrStatus<Trait>("Abducted");
        Trait restrainedTrait = character.traitContainer.GetTraitOrStatus<Trait>("Restrained");
        if (abductedTrait != null) {
            info += abductedTrait.GetToolTipText();
        }
        if (restrainedTrait != null) {
            if(info != string.Empty) {
                info += "\n";
            }
            info += restrainedTrait.GetToolTipText();
        }
        if(info != string.Empty) {
            UIManager.Instance.ShowSmallInfo(info);
        }
    }
    public void HideToolTip() {
        UIManager.Instance.HideSmallLocationInfo();
    }

    #region Listeners
    private void OnCharacterStartedTravellingOutside(Character travellingCharacter) {
        if (character.carryComponent.IsCurrentlyPartOf(travellingCharacter)) {
            UpdateLocationIcons();
        }
    }
    private void OnCharacterDoneTravellingOutside(Character travellingCharacter) {
        if (character.carryComponent.IsCurrentlyPartOf(travellingCharacter)) {
            UpdateLocationIcons();
        }
    }
    private void OnCharacterChangedRace(Character character) {
        if (character.id == this.character.id) {
            UpdateInfo();
        }
    }
    private void OnTraitAdded(Character character, Trait trait) {
        if(character.id == this.character.id) {
            if(trait.name == "Abducted" || trait.name == "Restrained") {
                restrainedIcon.SetActive(true);
                unrestrainedGO.SetActive(false);
            }
        }
    }
    private void OnTraitRemoved(Character character, Trait trait) {
        if (character.id == this.character.id) {
            if (trait.name == "Abducted" || trait.name == "Restrained") {
                if(!character.traitContainer.HasTrait("Abducted", "Restrained")) {
                    restrainedIcon.SetActive(false);
                    unrestrainedGO.SetActive(true);
                }
            }
        }
    }
    #endregion


    public override void Reset() {
        base.Reset();
        //Messenger.RemoveListener<Party>(Signals.PARTY_STARTED_TRAVELLING, OnPartyStartedTravelling);
        //Messenger.RemoveListener<Party>(Signals.PARTY_DONE_TRAVELLING, OnPartyDoneTravelling);
    }

    private void OnEnable() {
        Messenger.AddListener<Character>(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, OnCharacterStartedTravellingOutside);
        Messenger.AddListener<Character>(CharacterSignals.FINISHED_TRAVELLING_IN_WORLD, OnCharacterDoneTravellingOutside);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnTraitAdded);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnTraitRemoved);
    }

    private void OnDisable() {
        if (Messenger.eventTable.ContainsKey(CharacterSignals.STARTED_TRAVELLING_IN_WORLD)) {
            Messenger.RemoveListener<Character>(CharacterSignals.STARTED_TRAVELLING_IN_WORLD, OnCharacterStartedTravellingOutside);
        }
        if (Messenger.eventTable.ContainsKey(CharacterSignals.FINISHED_TRAVELLING_IN_WORLD)) {
            Messenger.RemoveListener<Character>(CharacterSignals.FINISHED_TRAVELLING_IN_WORLD, OnCharacterDoneTravellingOutside);
        }
        if (Messenger.eventTable.ContainsKey(CharacterSignals.CHARACTER_CHANGED_RACE)) {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        }
        Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnTraitAdded);
        Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_REMOVED, OnTraitRemoved);
    }
}
