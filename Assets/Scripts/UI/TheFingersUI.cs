using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TheFingersUI : MonoBehaviour {
    [Header("General")]
    public Button createBtn;
    public Image createProgress;

    //[Header("Minion")]
    //public TextMeshProUGUI minionName;
    //public CharacterPortrait minionPortrait;
    //public Button selectMinionBtn;

    [Header("Faction")]
    public GameObject createNewFactionGO;
    public GameObject characterNameplateItemPrefab;
    public TMP_InputField factionNameInput;
    public TextMeshProUGUI leaderNameLbl;
    public ScrollRect characterScrollRect;
    public GameObject ideologyHolder;
    public IdeologyPickerUI[] ideologiesPicker;
    //public TextMeshProUGUI ideologiesText;
    public Dictionary<Character, CharacterNameplateItem> characterNameplateItems { get; private set; }

    [Header("Exclusive Ideology")]
    public GameObject exclusiveIdeologyHolder;
    public TMP_Dropdown exclusiveIdeologyCategoryDropdown;
    public TMP_Dropdown exclusiveIdeologyRequirementDropdown;
    //private Goader fingers { get; set; }
    private Character chosenLeader { get; set; }
    //private List<FactionIdeology> ideologies = new List<FactionIdeology>();
    private string[] criteriaRaces = new string[] { "HUMANS", "ELVES" };

    #region General
    public void OnClickCreate(BaseLandmark landmark) {
        //fingers = landmark as Goader;
        if(characterNameplateItems == null) {
            characterNameplateItems = new Dictionary<Character, CharacterNameplateItem>();
        }
        //fingers.tileLocation.region.SetAssignedMinion(chosenMinion);
        //chosenMinion.SetAssignedRegion(fingers.tileLocation.region);
        //fingers.StartDelay(0);
        ShowCreateNewFactionUI();
        UpdateCreateButton();
        UpdateSelectMinionBtn();
    }
    private void UpdateCreateButton() {
        createProgress.gameObject.SetActive(false);
        // createBtn.interactable = /*chosenMinion != null &&*/ !fingers.hasBeenActivated && !createNewFactionGO.activeSelf;
        // if (!createBtn.interactable) {
        //     if (fingers.hasBeenActivated) {
        //         createProgress.gameObject.SetActive(true);
        //         createProgress.fillAmount = 0;
        //     }
        // }
    }
    public void OnClickCreateNewFaction() {
        if (CanCreateNewFaction()) {
            Faction newFaction = FactionManager.Instance.CreateNewFaction(RACE.NONE, factionName: factionNameInput.text);
            chosenLeader.ChangeFactionTo(newFaction);
            newFaction.SetLeader(chosenLeader, false);

            //for (int i = 0; i < ideologies.Count; i++) {
            //    newFaction.ideologyComponent.SetCurrentIdeology(i, ideologies[i]);
            //}
            //newFaction.ideologyComponent.SwitchToIdeology((FACTION_IDEOLOGY) ideologyDropdown.value);
            //if(newFaction.ideologyComponent.currentIdeologies.ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
            //    Exclusive exclusiveIdeology = newFaction.ideologyComponent.currentIdeologies as Exclusive;
            //    exclusiveIdeology.SetIndividualRequirements((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value, exclusiveIdeologyRequirementDropdown.options[exclusiveIdeologyRequirementDropdown.value].text);
            //}

            Region regionLocation = chosenLeader.currentRegion;
            //if (chosenLeader.currentRegion != null) {
            //    regionLocation = chosenLeader.currentRegion;
            //} else if (chosenLeader.currentArea != null) {
            //    regionLocation = chosenLeader.currentArea.region;
            //}

            Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Create Faction", "character_create_faction");
            log.AddToFillers(chosenLeader, chosenLeader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(newFaction, newFaction.name, LOG_IDENTIFIER.FACTION_1);
            log.AddToFillers(regionLocation, regionLocation.name, LOG_IDENTIFIER.LANDMARK_1);
            log.AddLogToInvolvedObjects();
            // PlayerManager.Instance.player.ShowNotificationFrom(log);

            //chosenLeader.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Faction_Leader, chosenLeader);

            //fingers.Activate();

            HideCreateNewFactionUI();
            UpdateSelectMinionBtn();
            UpdateCreateButton();
            PlayerUI.Instance.ShowGeneralConfirmation("New Faction", "Found New Faction!");

        }
    }
    private bool CanCreateNewFaction() {
        if (string.IsNullOrEmpty(factionNameInput.text)) {
            PlayerUI.Instance.ShowGeneralConfirmation("No Faction Name Error!", "Please enter a faction name.");
            return false;
        }
        if (chosenLeader == null) {
            PlayerUI.Instance.ShowGeneralConfirmation("No Faction Leader Error!", "Please select a faction leader.");
            return false;
        }
        //if(ideologies.Count != FactionManager.Instance.categorizedFactionIdeologies.Length) {
        //    PlayerUI.Instance.ShowGeneralConfirmation("Incomplete Ideologies!", "Please continue selecting ideologies for your faction.");
        //    return false;
        //}
        return true;
    }
    private bool CanAddIdeology(FACTION_IDEOLOGY ideologyType) {
        //if (ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
        //    if ((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE && exclusiveIdeologyRequirementDropdown.value == 0) { //This means that the race choice is NONE
        //        PlayerUI.Instance.ShowGeneralConfirmation("No Race Requirement Error!", "Please select a race for the requirement.");
        //        return false;
        //    }
        //}
        return true;
    }
    #endregion

    #region Minion
    public void OnClickSelectMinion() {
        List<Character> characters = new List<Character>();
        for (int i = 0; i < PlayerManager.Instance.player.minions.Count; i++) {
            characters.Add(PlayerManager.Instance.player.minions[i].character);
        }
        string title = "Select Minion";
        UIManager.Instance.ShowClickableObjectPicker(characters, SetChosenMinion, null, CanChooseMinion, title);
    }
    private bool CanChooseMinion(Character character) {
        return !character.minion.isAssigned;
    }
    private void SetChosenMinion(object c) {
        Character character = c as Character;
        //chosenMinion = character.minion;
        //minionPortrait.GeneratePortrait(chosenMinion.character);
        //minionName.text = chosenMinion.character.name;
        //minionPortrait.gameObject.SetActive(true);
        //minionName.gameObject.SetActive(true);
        UpdateCreateButton();
        UIManager.Instance.HideObjectPicker();
    }
    private void UpdateSelectMinionBtn() {
        //selectMinionBtn.interactable = !fingers.hasBeenActivated && !createNewFactionGO.activeSelf;
    }
    #endregion

    #region New Faction
    private void ShowCreateNewFactionUI() {
        SetChosenCharacter(null);
        factionNameInput.text = string.Empty;
        PopulateCharactersToChooseFrom();
        HideChooseIdeology();
        createNewFactionGO.SetActive(true);
    }
    public void HideCreateNewFactionUI() {
        createNewFactionGO.SetActive(false);
        UpdateCreateButton();
    }
    private void ShowChooseIdeology() {
        ideologyHolder.SetActive(true);
        HideExclusiveIdeology();
        PopulateIdeologiesPickerUI();
    }
    private void HideChooseIdeology() {
        ideologyHolder.SetActive(false);
        //exclusiveIdeologyHolder.SetActive(false);
    }
    private void PopulateCharactersToChooseFrom() {
        UtilityScripts.Utilities.DestroyChildren(characterScrollRect.content);
        characterNameplateItems.Clear();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if(character.isFriendlyFactionless /*&& character.faction.leader != character*/) {
                CharacterNameplateItem item = CreateNewCharacterNameplateItem();
                item.SetObject(character);
                item.SetAsToggle();
                item.SetPortraitInteractableState(false);
                item.AddOnToggleAction(OnToggleCharacter);
                item.gameObject.SetActive(true);
                characterNameplateItems.Add(character, item);
            }
        }
    }
    private CharacterNameplateItem CreateNewCharacterNameplateItem() {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(characterNameplateItemPrefab.name, Vector3.zero, Quaternion.identity, characterScrollRect.content);
        CharacterNameplateItem item = go.GetComponent<CharacterNameplateItem>();
        go.SetActive(false);
        return item;
    }
    private void OnToggleCharacter(Character character, bool isOn) {
        if (chosenLeader != null) {
            characterNameplateItems[chosenLeader].SetToggleState(false);
        }
        if (isOn) {
            //if (chosenLeader == null) {
            //    ShowChooseIdeology();
            //} else {
            //    characterNameplateItems[chosenLeader].SetToggleState(false);
            //}
            SetChosenCharacter(character);
            ShowChooseIdeology();
        } else {
            SetChosenCharacter(null);
            HideChooseIdeology();
        }
    }
    private void SetChosenCharacter(Character character) {
        chosenLeader = character;
        if(chosenLeader == null) {
            leaderNameLbl.text = string.Empty;
        } else {
            leaderNameLbl.text = $"{character.name}, {character.raceClassName}";
        }
    }
    private void PopulateIdeologiesPickerUI() {
        for (int i = 0; i < ideologiesPicker.Length; i++) {
            ideologiesPicker[i].Initialize(i);
        }
    }
    #endregion

    #region Ideology
    private int ideologyCategoryIndex;
    public void ShowAppropriateIdeologyContent(FACTION_IDEOLOGY ideologyType, int index) {
        ideologyCategoryIndex = index;
        if (ideologyType == FACTION_IDEOLOGY.EXCLUSIVE) {
            ShowExclusiveIdeology();
        } else {
            HideExclusiveIdeology();
        }
    }

    #endregion

    #region Exclusive Ideology
    public void OnClickOkExclusive() {
        if (CanAddIdeology(FACTION_IDEOLOGY.EXCLUSIVE)) {
            ideologiesPicker[ideologyCategoryIndex].SetExclusiveRequirements((EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value, exclusiveIdeologyRequirementDropdown.options[exclusiveIdeologyRequirementDropdown.value].text);
            HideExclusiveIdeology();
        }
    }
    public void OnClickCancelExclusive() {
        HideExclusiveIdeology();
    }
    public void UpdateExclusiveIdeologyContent(EXCLUSIVE_IDEOLOGY_CATEGORIES exclusiveCategory, string exclusiveRequirement, int categoryIndex) {
        ideologyCategoryIndex = categoryIndex;
        ShowExclusiveIdeology();
        for (int i = 0; i < exclusiveIdeologyCategoryDropdown.options.Count; i++) {
            if(exclusiveIdeologyCategoryDropdown.options[i].text == System.Enum.GetName(typeof(EXCLUSIVE_IDEOLOGY_CATEGORIES), exclusiveCategory)) {
                exclusiveIdeologyCategoryDropdown.value = i;
                break;
            }
        }
        for (int i = 0; i < exclusiveIdeologyRequirementDropdown.options.Count; i++) {
            if (exclusiveIdeologyRequirementDropdown.options[i].text == exclusiveRequirement) {
                exclusiveIdeologyRequirementDropdown.value = i;
                break;
            }
        }
    }
    private void ShowExclusiveIdeology() {
        PopulateExclusiveIdeologyCategory();
        PopulateExclusiveIdeologyRequirements();
        exclusiveIdeologyHolder.SetActive(true);
    }
    private void HideExclusiveIdeology() {
        exclusiveIdeologyHolder.SetActive(false);
    }
    private void PopulateExclusiveIdeologyCategory() {
        exclusiveIdeologyCategoryDropdown.ClearOptions();
        string[] exclusiveIdeologyCategories = System.Enum.GetNames(typeof(EXCLUSIVE_IDEOLOGY_CATEGORIES));
        for (int i = 0; i < exclusiveIdeologyCategories.Length; i++) {
            exclusiveIdeologyCategoryDropdown.options.Add(new TMP_Dropdown.OptionData(exclusiveIdeologyCategories[i]));
        }
        exclusiveIdeologyCategoryDropdown.RefreshShownValue();
        exclusiveIdeologyCategoryDropdown.value = 0;
    }
    private void PopulateExclusiveIdeologyRequirements() {
        EXCLUSIVE_IDEOLOGY_CATEGORIES category = (EXCLUSIVE_IDEOLOGY_CATEGORIES) exclusiveIdeologyCategoryDropdown.value;
        exclusiveIdeologyRequirementDropdown.ClearOptions();
        if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.GENDER) {
            string[] genders = System.Enum.GetNames(typeof(GENDER));
            for (int i = 0; i < genders.Length; i++) {
                exclusiveIdeologyRequirementDropdown.options.Add(new TMP_Dropdown.OptionData(genders[i]));
            }
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.RACE) {
            //string[] races = System.Enum.GetNames(typeof(RACE));
            string[] races = criteriaRaces;
            for (int i = 0; i < races.Length; i++) {
                exclusiveIdeologyRequirementDropdown.options.Add(new TMP_Dropdown.OptionData(races[i]));
            }
        } else if (category == EXCLUSIVE_IDEOLOGY_CATEGORIES.TRAIT) {
            string[] traits = FactionManager.Instance.exclusiveIdeologyTraitRequirements;
            for (int i = 0; i < traits.Length; i++) {
                exclusiveIdeologyRequirementDropdown.options.Add(new TMP_Dropdown.OptionData(traits[i]));
            }
        }
        exclusiveIdeologyRequirementDropdown.RefreshShownValue();
        exclusiveIdeologyRequirementDropdown.value = 0;
    }
    public void OnChangeExclusiveIdeologyCategory(int index) {
        PopulateExclusiveIdeologyRequirements();
    }
    #endregion

    #region Action: Force Leave Faction
    public void OnClickForceLeaveFaction() {
        List<Faction> viableFactions = new List<Faction>();
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.isMajorNonPlayer) {
                viableFactions.Add(faction);
            }
        }
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(viableFactions, "Choose Faction", CanChooseFactionToLeave, null, null, OnPickFaction, ConfirmLeave, "Leave Faction");
    }
    private bool CanChooseFactionToLeave(Faction faction) {
        //return faction.isMajorNonPlayer;
        return true;
    }
    private void OnPickFaction(object obj) {
        Faction faction = obj as Faction;
        UIManager.Instance.dualObjectPicker.PopulateColumn(faction.characters, CanChooseCharacterToLeave, null, null, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Character");
    }
    private bool CanChooseCharacterToLeave(Character character) {
        return character.faction.leader != character;
    }
    private void ConfirmLeave(object obj1, object obj2) {
        Faction faction = obj1 as Faction;
        Character character = obj2 as Character;

        character.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Faction, character, "left_faction_normal");
        //character.ChangeFactionTo(FactionManager.Instance.friendlyNeutralFaction);
        //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "left_faction_normal");
        //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
        //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    }
    #endregion

    #region Action: Force Join Faction
    public void OnClickForceJoinFaction() {
        List<Character> viableCharacters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            if (character.isFriendlyFactionless && character.isStillConsideredAlive && character.IsAble()) {
                viableCharacters.Add(character);
            }
        }
        UIManager.Instance.dualObjectPicker.ShowDualObjectPicker(viableCharacters, "Choose Character", CanChooseCharacterToJoin, null, null, OnPickCharacter, ConfirmJoin, "Join Faction");
    }
    private bool CanChooseCharacterToJoin(Character character) {
        return true;
    }
    private void OnPickCharacter(object obj) {
        Character character = obj as Character;
        List<Faction> viableFactions = new List<Faction>();
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.isMajorNonPlayer) {
                viableFactions.Add(faction);
            }
        }
        UIManager.Instance.dualObjectPicker.PopulateColumn(viableFactions, (faction) => CanChooseFactionToJoin(faction, character), null, null, UIManager.Instance.dualObjectPicker.column2ScrollView, UIManager.Instance.dualObjectPicker.column2ToggleGroup, "Choose Faction");
    }
    private bool CanChooseFactionToJoin(Faction faction, Character character) {
        return !faction.isDestroyed && faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(character);
    }
    private void ConfirmJoin(object obj1, object obj2) {
        Character character = obj1 as Character;
        Faction faction = obj2 as Faction;

        character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, faction.characters[0], "join_faction_normal");
        //character.ChangeFactionTo(faction);
        //Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "join_faction_normal");
        //log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //log.AddToFillers(faction, faction.name, LOG_IDENTIFIER.FACTION_1);
        //character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
    }
    #endregion
}
