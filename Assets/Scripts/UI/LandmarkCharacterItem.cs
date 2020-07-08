using EZObjectPools;
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
        subLbl.text = character.raceClassName;
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
        if (_parentBase is RegionInfoUI) {
            if (character.traitContainer.HasTrait("Abducted", "Restrained")) {
                restrainedIcon.SetActive(true);
                unrestrainedGO.SetActive(false);
            } else {
                restrainedIcon.SetActive(false);
                unrestrainedGO.SetActive(true);
            }
            if ((character.carryComponent.masterCharacter.avatar.isTravelling && character.carryComponent.masterCharacter.avatar.travelLine != null) || character.carryComponent.masterCharacter.avatar.isTravellingOutside) {
                travellingIcon.SetActive(true);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(true);
            } else {
                travellingIcon.SetActive(false);
                arrivedIcon.SetActive(false);
                coverGO.SetActive(false);
            }
        } else if (_parentBase is TileObjectInfoUI) {
            if (character.traitContainer.HasTrait("Abducted", "Restrained")) {
                restrainedIcon.SetActive(true);
                unrestrainedGO.SetActive(false);
            } else {
                restrainedIcon.SetActive(false);
                unrestrainedGO.SetActive(true);
            }
            if (character.carryComponent.masterCharacter.avatar.isTravelling && character.carryComponent.masterCharacter.avatar.travelLine != null) {
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
        if (character.carryComponent.masterCharacter.avatar.targetLocation == null) {
            return;
        }
        Region showingRegion = UIManager.Instance.GetCurrentlyShowingSmallInfoLocation();
        if (showingRegion == null || showingRegion.id != character.carryComponent.masterCharacter.avatar.targetLocation.id) {
            
            float x = UIManager.Instance.locationSmallInfoRT.position.x;
            //float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
            UIManager.Instance.ShowSmallLocationInfo(character.carryComponent.masterCharacter.avatar.targetLocation, new Vector3(x, thisTrans.position.y - 15f, 0f), "Travelling to:");
        }
    }
    public void ShowArrivedTooltip() {
        //UIManager.Instance.ShowSmallInfo("Arrived at " + character.currentParty.specificLocation.name);
        Region showingRegion = UIManager.Instance.GetCurrentlyShowingSmallInfoLocation();
        if (showingRegion == null || showingRegion.id != character.carryComponent.masterCharacter.avatar.targetLocation.id) {
            if (character.carryComponent.masterCharacter.avatar.targetLocation == null) {
                return;
            }
            float x = thisTrans.position.x + thisTrans.sizeDelta.x + 50f;
            UIManager.Instance.ShowSmallLocationInfo(character.carryComponent.masterCharacter.avatar.targetLocation, new Vector3(x, thisTrans.position.y - 15f, 0f), "Arrived at:");
        }
    }
    public void ShowRestrainedTooltip() {
        string info = string.Empty;
        Trait abductedTrait = character.traitContainer.GetNormalTrait<Trait>("Abducted");
        Trait restrainedTrait = character.traitContainer.GetNormalTrait<Trait>("Restrained");
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
        Messenger.AddListener<Character>(Signals.CHARACTER_STARTED_TRAVELLING_OUTSIDE, OnCharacterStartedTravellingOutside);
        Messenger.AddListener<Character>(Signals.CHARACTER_DONE_TRAVELLING_OUTSIDE, OnCharacterDoneTravellingOutside);
        Messenger.AddListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnTraitAdded);
        Messenger.AddListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnTraitRemoved);
    }

    private void OnDisable() {
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_STARTED_TRAVELLING_OUTSIDE)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_STARTED_TRAVELLING_OUTSIDE, OnCharacterStartedTravellingOutside);
        }
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_DONE_TRAVELLING_OUTSIDE)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_DONE_TRAVELLING_OUTSIDE, OnCharacterDoneTravellingOutside);
        }
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_CHANGED_RACE)) {
            Messenger.RemoveListener<Character>(Signals.CHARACTER_CHANGED_RACE, OnCharacterChangedRace);
        }
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_ADDED, OnTraitAdded);
        Messenger.RemoveListener<Character, Trait>(Signals.CHARACTER_TRAIT_REMOVED, OnTraitRemoved);
    }
}
