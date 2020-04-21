using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.IO;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Tutorial;
using UnityEngine.Events;
using UtilityScripts;

public class ConsoleBase : InfoUIBase {

    private Dictionary<string, Action<string[]>> _consoleActions;

    private List<string> commandHistory;
    //private int currentHistoryIndex;

    [SerializeField] private GameObject consoleGO;
    [SerializeField] private Text consoleLbl;
    [SerializeField] private InputField consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private TextMeshProUGUI commandHistoryLbl;

    [SerializeField] private Dropdown raceDropdown;
    [SerializeField] private Dropdown classDropdown;
    [SerializeField] private InputField levelInput;

    [SerializeField] private GameObject fullDebugGO;
    [SerializeField] private TextMeshProUGUI fullDebugLbl;
    [SerializeField] private TextMeshProUGUI fullDebug2Lbl;

    void Awake() {
        Initialize();
    }

    internal override void Initialize() {
        commandHistory = new List<string>();
        _consoleActions = new Dictionary<string, Action<string[]>>() {
            {"/help", ShowHelp},
            {"/change_faction_rel_stat", ChangeFactionRelationshipStatus},
            {"/kill",  KillCharacter},
            //{"/lfli", LogFactionLandmarkInfo},
            {"/center_character", CenterOnCharacter},
            {"/center_landmark", CenterOnLandmark },
            {"/log_location_history", LogLocationHistory  },
            {"/log_area_characters_history", LogAreaCharactersHistory  },
            {"/get_characters_with_item", GetCharactersWithItem },
            {"/i_toggle_sub", ToggleSubscriptionToInteraction },
            {"/add_trait_character", AddTraitToCharacter },
            {"/remove_trait_character", RemoveTraitToCharacter },
            {"/transfer_character_faction", TransferCharacterToFaction },
            {"/show_full_debug", ShowFullDebug },
            //{"/force_action", ForceCharacterInteraction },
            {"/t_freeze_char", ToggleFreezeCharacter },
            {"/set_mood", SetMoodToCharacter },
            {"/log_awareness", LogAwareness },
            {"/add_rel", AddRelationship },
            {"/rel_deg", ForcedRelationshipDegradation },
            {"/set_hp", SetHP },
            {"/gain_summon",  GainSummon},
            //{"/gain_summon_slot",  GainSummonSlot},
            {"/gain_artifact",  GainArtifact},
            // {"/gain_artifact_slot",  GainArtifactSlot},
            {"/set_fullness", SetFullness },
            {"/set_tiredness", SetTiredness },
            {"/set_happiness", SetHappiness },
            {"/set_comfort", SetComfort },
            {"/set_hope", SetHope },
            {"/gain_i_ability", GainInterventionAbility },
            {"/destroy_tile_obj", DestroyTileObj },
            {"/force_update_animation", ForceUpdateAnimation },
            {"/highlight_structure_tiles", HighlightStructureTiles },
            {"/log_obj_advertisements", LogObjectAdvertisements },
            {"/adjust_opinion", AdjustOpinion },
            {"/join_faction", JoinFaction },
            {"/emotion", TriggerEmotion },
            // {"/adjust_resource", TriggerEmotion },
            {"/change_archetype", ChangeArchetype },
            {"/elemental_damage", ChangeCharacterElementalDamage },
            {"/add_item", AddItemToCharacter },
        };

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        //Messenger.AddListener(Signals.TICK_ENDED, CheckForWrongCharacterData);
        Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
#endif
        InitializeMinion();
    }
    private void Update() {
        fullDebugLbl.text = string.Empty;
        fullDebug2Lbl.text = string.Empty;
        if (GameManager.Instance.showFullDebug) {
            FullDebugInfo();
        }
        fullDebugGO.SetActive(!string.IsNullOrEmpty(fullDebugLbl.text) || !string.IsNullOrEmpty(fullDebug2Lbl.text));

        if (isShowing && consoleInputField.text != "" && Input.GetKeyDown(KeyCode.Return)) {
            SubmitCommand();
        }
        //if (isShowing) {
        //    if (Input.GetKeyDown(KeyCode.UpArrow)) {
        //        int newIndex = currentHistoryIndex - 1;
        //        string command = commandHistory.ElementAtOrDefault(newIndex);
        //        if (!string.IsNullOrEmpty(command)) {
        //            consoleLbl.text = command;
        //            currentHistoryIndex = newIndex;
        //        }
        //    }
        //    if (Input.GetKeyDown(KeyCode.DownArrow)) {
        //        int newIndex = currentHistoryIndex + 1;
        //        string command = commandHistory.ElementAtOrDefault(newIndex);
        //        if (!string.IsNullOrEmpty(command)) {
        //            consoleLbl.text = command;
        //            currentHistoryIndex = newIndex;
        //        }
        //    }
        //}

    }

    #region Full Debug
    private void FullDebugInfo() {
        fullDebugLbl.text = string.Empty;
        fullDebug2Lbl.text = string.Empty;
        if (UIManager.Instance != null && UIManager.Instance.characterInfoUI.isShowing) {
            fullDebugLbl.text += GetMainCharacterInfo();
            fullDebug2Lbl.text += GetSecondaryCharacterInfo();
        }
    }
    private string GetMainCharacterInfo() {
        Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        string text = $"{character.name}'s info:";
        text += $"\n<b>Gender:</b> {character.gender}";
        text += $"\n<b>Race:</b> {character.race}";
        text += $"\n<b>Class:</b> {character.characterClass.className}";
        text += $"\n<b>Is Dead?:</b> {character.isDead}";
        text += $"\n<b>Home Location:</b> {character.homeStructure}" ?? "None";

        text += "\n<b>LOCATION INFO:</b>";
        text += $"\n\t<b>Region Location:</b> {character.currentRegion?.name}" ?? "None";
        text += $"\n\t<b>Structure Location:</b> {character.currentStructure}" ?? "None";
        text += $"\n\t<b>Grid Location:</b> {character.gridTileLocation?.localPlace}" ?? "None";

        text += $"\n<b>Faction:</b> {character.faction?.name}" ?? "None";
        text += $"\n<b>Current Action:</b> {character.currentActionNode?.goapName}" ?? "None";
        //if (character.currentActionNode != null) {
        //    text += "\n<b>Current Plan:</b> " + character.currentActionNode.parentPlan.GetGoalSummary();
        //}
        if (character.currentParty.icon != null) {
            text += $"\n<b>Is Travelling:</b> {character.currentParty.icon.isTravelling}";
            text += $"\n<b>Target Location:</b> {character.currentParty.icon.targetLocation?.name}" ?? "None";
            text += $"\n<b>Target Structure:</b> {character.currentParty.icon.targetStructure}" ?? "None";
        }

        if (character.marker) {
            text += "\n<b>MARKER DETAILS:</b>";
            text += $"\n<b>Target POI:</b> {character.marker.targetPOI?.name}" ?? "None";
            text += $"\n<b>Destination Tile:</b> {character.marker.destinationTile}" ?? "None";
            text += $"\n<b>Stop Movement?:</b> {character.marker.pathfindingAI.isStopMovement}";
        }

        //text += "\n<b>All Plans:</b> ";
        //if (character.allGoapPlans.Count > 0) {
        //    for (int i = 0; i < character.allGoapPlans.Count; i++) {
        //        GoapPlan goapPlan = character.allGoapPlans[i];
        //        text += "\n" + goapPlan.GetPlanSummary();
        //    }
        //} else {
        //    text += "\nNone";
        //}

        text += "\n<b>Action History:</b> ";
        List<string> reverseHistory = new List<string>(character.actionHistory);
        reverseHistory.Reverse();
        if (reverseHistory.Count > 0) {
            for (int i = 0; i < reverseHistory.Count; i++) {
                text += $"\n\n{reverseHistory[i]}";
            }
        } else {
            text += "\nNone";
        }

        return text;
    }
    private string GetSecondaryCharacterInfo() {
        Character character = UIManager.Instance.characterInfoUI.activeCharacter;
        //string text = character.name + "'s Relationships " + character.relationships.Count.ToString();
        //int counter = 0;
        //foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
        //    text += "\n\n" + counter.ToString() + kvp.Value.GetSummary();
        //    counter++;
        //}

        string text = $"\n{character.name}'s Location History:";
        for (int i = 0; i < character.locationHistory.Count; i++) {
            text += $"\n\t{character.locationHistory[i]}";
        }
        return text;
    }
    #endregion

    public void ShowConsole() {
        isShowing = true;
        consoleGO.SetActive(true);
        ClearCommandField();
        consoleInputField.Select();
    }

    public void HideConsole() {
        isShowing = false;
        consoleGO.SetActive(false);
        //consoleInputField.foc = false;
        HideCommandHistory();
        ClearCommandHistory();
        consoleInputField.DeactivateInputField();
    }
    private void ClearCommandField() {
        consoleLbl.text = string.Empty;
    }
    private void ClearCommandHistory() {
        commandHistoryLbl.text = string.Empty;
        commandHistory.Clear();
    }
    private void ShowCommandHistory() {
        commandHistoryGO.SetActive(true);
    }
    private void HideCommandHistory() {
        commandHistoryGO.SetActive(false);
    }
    public void SubmitCommand() {
        string command = consoleLbl.text;
        string[] words = command.Split(' ');
        string mainCommand = words[0];

        var reg = new Regex("\".*?\"");
        var parameters = reg.Matches(command).Cast<Match>().Select(m => m.Value).ToArray();
        //List<string> parameters = matches.Cast<string>().ToList();
        for (int i = 0; i < parameters.Length; i++) {
            string currParameter = parameters[i];
            string trimmed = currParameter.Trim(new char[] { '"' });
            parameters[i] = trimmed;
        }

        if (_consoleActions.ContainsKey(mainCommand)) {
            _consoleActions[mainCommand](parameters);
        } else {
            AddCommandHistory(command);
            AddErrorMessage($"Error: there is no such command as {mainCommand}![-]");
        }
    }
    private void AddCommandHistory(string history) {
        commandHistoryLbl.text += $"{history}\n";
        commandHistory.Add(history);
        //currentHistoryIndex = commandHistory.Count - 1;
        ShowCommandHistory();
    }
    private void AddErrorMessage(string errorMessage) {
        errorMessage += ". Use /help for a list of commands";
        commandHistoryLbl.text += $"<color=#FF0000>{errorMessage}</color>\n";
        ShowCommandHistory();
    }
    private void AddSuccessMessage(string successMessage) {
        commandHistoryLbl.text += $"<color=#00FF00>{successMessage}</color>\n";
        ShowCommandHistory();
    }

    #region Listeners
    private void OnCharacterDoingAction(Character character, ActualGoapNode actionNode) {
        if (typesSubscribedTo.Contains(actionNode.goapType)) {
            Messenger.Broadcast<string, int, UnityAction>(Signals.SHOW_DEVELOPER_NOTIFICATION,
                $"{character.name} is doing {actionNode.goapType}",
                100, () => UIManager.Instance.ShowCharacterInfo(character, true));
            UIManager.Instance.Pause();
        }
    }
    //private void CheckForWrongCharacterData() {
    //    for (int i = 0; i < LandmarkManager.Instance.allAreas.Count; i++) {
    //        NPCSettlement currNpcSettlement = LandmarkManager.Instance.allAreas[i];
    //        if (currNpcSettlement == PlayerManager.Instance.player.playerNpcSettlement) {
    //            continue;
    //        }
    //        for (int j = 0; j < currNpcSettlement.charactersAtLocation.Count; j++) {
    //            Character character = currNpcSettlement.charactersAtLocation[j];
    //            if (character.isDead) {
    //                Debug.LogWarning("There is still a dead character at " + currNpcSettlement.name + " : " + character.name);
    //                //UIManager.Instance.Pause();
    //            }
    //        }
    //        //for (int j = 0; j < currNpcSettlement.possibleSpecialTokenSpawns.Count; j++) {
    //        //    SpecialToken token = currNpcSettlement.possibleSpecialTokenSpawns[j];
    //        //    if (token.structureLocation == null) {
    //        //        Debug.LogWarning("There is token at " + currNpcSettlement.name + " that doesn't have a structure location : " + token.name);
    //        //        //UIManager.Instance.Pause();
    //        //    }
    //        //}
    //    }
    //    for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
    //        Character currCharacter = CharacterManager.Instance.allCharacters[i];
    //        if (!currCharacter.isDead) {
    //            if (currCharacter.faction == null) {
    //                Debug.LogWarning("There is an alive character with a null faction! " + currCharacter.name);
    //                UIManager.Instance.Pause();
    //            }
    //            //if (currCharacter.homeStructure == null) {
    //            //    Debug.LogWarning("There is an alive character with a null home structure! " + currCharacter.name);
    //            //    UIManager.Instance.Pause();
    //            //}
    //            //if (currCharacter.currentStructure == null && currCharacter.minion == null) {
    //            //    Debug.LogWarning("There is an alive character with a null current structure! " + currCharacter.name);
    //            //    //UIManager.Instance.Pause();
    //            //}
    //            if (currCharacter.marker) {
    //                for (int j = 0; j < currCharacter.combatComponent.hostilesInRange.Count; j++) {
    //                    Character hostileInRange = currCharacter.combatComponent.hostilesInRange[j];
    //                    if (hostileInRange.isDead) {
    //                        Debug.LogWarning("There is a dead character (" + hostileInRange.name + ") in " + currCharacter.name + "'s hostile range!");
    //                        UIManager.Instance.Pause();
    //                    }
    //                }
    //            }

    //        }
    //    }
    //}
    #endregion

    #region Misc
    private void ShowHelp(string[] parameters) {
        for (int i = 0; i < _consoleActions.Count; i++) {
            AddCommandHistory(_consoleActions.Keys.ElementAt(i));
        }
    }
    public void AddText(string text) {
        consoleInputField.text += $" {text}";
    }
    public void ShowFullDebug(string[] parameters) {
        GameManager.Instance.showFullDebug = !GameManager.Instance.showFullDebug;
        AddSuccessMessage($"Show Full Debug Info Set to {GameManager.Instance.showFullDebug}");
    }
    public void MoveNextPage(TextMeshProUGUI text) {
        text.pageToDisplay += 1;
    }
    public void MovePreviousPage(TextMeshProUGUI text) {
        text.pageToDisplay -= 1;
    }
    public void ToggleShowAllTileTooltip(bool state) {
        GameManager.showAllTilesTooltip = state;
    }
    #endregion

    #region Faction Relationship
    private void ChangeFactionRelationshipStatus(string[] parameters) {
        if (parameters.Length != 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }
        string faction1ParameterString = parameters[0];
        string faction2ParameterString = parameters[1];
        string newRelStatusString = parameters[2];

        Faction faction1;
        Faction faction2;

        int faction1ID = -1;
        int faction2ID = -1;

        bool isFaction1Numeric = int.TryParse(faction1ParameterString, out faction1ID);
        bool isFaction2Numeric = int.TryParse(faction2ParameterString, out faction2ID);

        string faction1Name = faction1ParameterString;
        string faction2Name = faction2ParameterString;

        FACTION_RELATIONSHIP_STATUS newRelStatus;

        if (isFaction1Numeric) {
            faction1 = FactionManager.Instance.GetFactionBasedOnID(faction1ID);
        } else {
            faction1 = FactionManager.Instance.GetFactionBasedOnName(faction1Name);
        }

        if (isFaction2Numeric) {
            faction2 = FactionManager.Instance.GetFactionBasedOnID(faction2ID);
        } else {
            faction2 = FactionManager.Instance.GetFactionBasedOnName(faction2Name);
        }

        try {
            newRelStatus = (FACTION_RELATIONSHIP_STATUS)Enum.Parse(typeof(FACTION_RELATIONSHIP_STATUS), newRelStatusString, true);
        } catch {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        if (faction1 == null || faction2 == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /change_faction_rel_stat");
            return;
        }

        FactionRelationship rel = FactionManager.Instance.GetRelationshipBetween(faction1, faction2);
        rel.SetRelationshipStatus(newRelStatus);

        AddSuccessMessage(
            $"Changed relationship status of {faction1.name} and {faction2.name} to {rel.relationshipStatus}");
    }
    #endregion

    #region Landmarks
    private void CenterOnLandmark(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of Center on Landmark");
            return;
        }
        string landmarkParameterString = parameters[0];
        int landmarkID;

        bool isLandmarkParameterNumeric = int.TryParse(landmarkParameterString, out landmarkID);
        BaseLandmark landmark = null;
        if (isLandmarkParameterNumeric) {
            landmark = LandmarkManager.Instance.GetLandmarkByID(landmarkID);
        } else {
            landmark = LandmarkManager.Instance.GetLandmarkByName(landmarkParameterString);
        }

        if (landmark == null) {
            AddErrorMessage($"There was an error in the command format of {parameters[0]}");
            return;
        }
        landmark.CenterOnLandmark();
        //UIManager.Instance.ShowLandmarkInfo(landmark);
        //character.CenterOnCharacter();
    }
    #endregion

    #region Characters
    private void KillCharacter(string[] parameters) {
        if (parameters.Length < 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }
        string characterParameterString = parameters[0];
        string causeString = parameters.ElementAtOrDefault(1);
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);

        Character character = null;

        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /kill");
            return;
        }

        if (string.IsNullOrEmpty(causeString)) {
            causeString = "normal";
        }

        character.Death(causeString);
    }
    private void CenterOnCharacter(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of Center on Character");
            return;
        }
        string characterParameterString = parameters[0];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of Center on Character");
            return;
        }
        UIManager.Instance.ShowCharacterInfo(character, true);
        //character.CenterOnCharacter();
    }
    private void AddItemToCharacter(string[] parameters) {
        if (parameters.Length != 2) { //character, tile object type
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AddItemToCharacter");
            return;
        }
        string characterParameterString = parameters[0];
        string objectTypeString = parameters[1];

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out var characterID);
        Character character = null;
        character = isCharacterParameterNumeric ? CharacterManager.Instance.GetCharacterByID(characterID) 
            : CharacterManager.Instance.GetCharacterByName(characterParameterString);
        
        if (character == null) {
            AddErrorMessage("There was an error in the command format of AddItemToCharacter");
            return;
        }

        TILE_OBJECT_TYPE objectType;
        if (Enum.TryParse(objectTypeString, true, out objectType)) {
            TileObject tileObject = InnerMapManager.Instance.CreateNewTileObject<TileObject>(objectType);
            character.ObtainItem(tileObject);
        }
        else {
            AddErrorMessage("There was an error in the command format of AddItemToCharacter");   
        }
    }
    private void LogLocationHistory(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of LogLocationHistory");
            return;
        }
        string characterParameterString = parameters[0];
        int characterID;

        bool isCharacterParameterNumeric = int.TryParse(characterParameterString, out characterID);
        Character character = null;
        if (isCharacterParameterNumeric) {
            character = CharacterManager.Instance.GetCharacterByID(characterID);
        } else {
            character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        }

        if (character == null) {
            AddErrorMessage("There was an error in the command format of LogLocationHistory");
            return;
        }

        string logSummary = $"{character.name}'s location history: ";
        //List<string> logs = character.ownParty.specificLocationHistory;
        //for (int i = 0; i < logs.Count; i++) {
        //    logSummary += "\n" + logs[i];
        //}
        AddSuccessMessage(logSummary);
    }
    private void GetCharactersWithItem(string[] parameters) {
        if (parameters.Length != 1) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of GetCharactersWithItem");
            return;
        }
        string itemParameterString = parameters[0];

        List<Character> characters = new List<Character>();
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character currCharacter = CharacterManager.Instance.allCharacters[i];
            if (currCharacter.isHoldingItem && currCharacter.HasItem(itemParameterString)) {
                characters.Add(currCharacter);
            }
        }
        string summary = $"Characters that have {itemParameterString}: ";
        if (characters.Count == 0) {
            summary += "\nNONE";
        } else {
            for (int i = 0; i < characters.Count; i++) {
                summary += $"\n{characters[i].name}";
            }
        }
        AddSuccessMessage(summary);
    }
    private void AddTraitToCharacter(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AddTraitToCharacter");
            return;
        }
        string characterParameterString = parameters[0];
        string traitParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        //if (AttributeManager.Instance.allTraits.ContainsKey(traitParameterString)) {
        character.traitContainer.AddTrait(character, traitParameterString);
        //} else {
        //    switch (traitParameterString) {
        //        case "Criminal":
        //            character.AddTrait(new Criminal());
        //            break;
        //        default:
        //            AddErrorMessage("There is no trait called " + traitParameterString);
        //            return;
        //    }
        //}
        AddSuccessMessage($"Added {traitParameterString} to {character.name}");
    }
    private void RemoveTraitToCharacter(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AddTraitToCharacter");
            return;
        }
        string characterParameterString = parameters[0];
        string traitParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        if (character.traitContainer.RemoveTrait(character, traitParameterString)) {
            AddSuccessMessage($"Removed {traitParameterString} to {character.name}");
        } else {
            AddErrorMessage($"{character.name} has no trait named {traitParameterString}");
        }


    }
    private void TransferCharacterToFaction(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of TransferCharacterToFaction");
            return;
        }
        string characterParameterString = parameters[0];
        string factionParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);
        if (faction == null) {
            AddErrorMessage($"There is no faction named {factionParameterString}");
            return;
        }

        character.ChangeFactionTo(faction);
        AddSuccessMessage($"Transferred {character.name} to {faction.name}");
    }
    private void ToggleFreezeCharacter(string[] parameter) {
        if (parameter.Length < 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ToggleFreezeCharacter");
            return;
        }
        string characterParameter = parameter[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameter);
        if (character == null) {
            AddErrorMessage($"There is no character with name {characterParameter}");
            return;
        }

        //if (character.canMove) {
        //    character.DecreaseCanMove();
        //} else {
        //    character.IncreaseCanMove();
        //}
        //AddSuccessMessage("Adjusted " + character.name + " do not disturb to " + character.doNotDisturb);
    }
    private void SetMoodToCharacter(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetMoodToCharacter");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string moodParameterString = parameters[1];

        int moodValue;
        if (!int.TryParse(moodParameterString, out moodValue)) {
            AddErrorMessage($"Mood value parameter is not an integer: {moodParameterString}");
            return;
        }
        character.moodComponent.SetMoodValue(moodValue);
        AddSuccessMessage($"Set Mood Value of {character.name} to {moodValue}");
    }
    private void SetFullness(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetFullness");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string fullnessParameterString = parameters[1];

        float fullness = character.needsComponent.fullness;
        if (!float.TryParse(fullnessParameterString, out fullness)) {
            AddErrorMessage($"Fullness parameter is not a float: {fullnessParameterString}");
            return;
        }
        character.needsComponent.SetFullness(fullness);
        AddSuccessMessage($"Set Fullness Value of {character.name} to {fullness}");
    }
    private void SetHappiness(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetHappiness");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string happinessParameterString = parameters[1];

        float happiness = character.needsComponent.happiness;
        if (!float.TryParse(happinessParameterString, out happiness)) {
            AddErrorMessage($"Happiness parameter is not a float: {happinessParameterString}");
            return;
        }
        character.needsComponent.SetHappiness(happiness);
        AddSuccessMessage($"Set Happiness Value of {character.name} to {happiness}");
    }
    private void SetTiredness(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetTiredness");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string tirednessParameterString = parameters[1];

        float tiredness = character.needsComponent.tiredness;
        if (!float.TryParse(tirednessParameterString, out tiredness)) {
            AddErrorMessage($"Tiredness parameter is not a float: {tirednessParameterString}");
            return;
        }
        character.needsComponent.SetTiredness(tiredness);
        AddSuccessMessage($"Set Tiredness Value of {character.name} to {tiredness}");
    }
    private void SetComfort(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetComfort");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string comfortParameterString = parameters[1];

        float comfort = character.needsComponent.comfort;
        if (!float.TryParse(comfortParameterString, out comfort)) {
            AddErrorMessage($"Comfort parameter is not a float: {comfortParameterString}");
            return;
        }
        character.needsComponent.SetComfort(comfort);
        AddSuccessMessage($"Set Comfort Value of {character.name} to {comfort}");
    }
    private void SetHope(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetHope");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string hopeParameterString = parameters[1];

        float hope = character.needsComponent.hope;
        if (!float.TryParse(hopeParameterString, out hope)) {
            AddErrorMessage($"Hope parameter is not a float: {hopeParameterString}");
            return;
        }
        character.needsComponent.SetHope(hope);
        AddSuccessMessage($"Set Hope Value of {character.name} to {hope}");
    }
    private void LogAwareness(string[] parameters) {
        if (parameters.Length != 1) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of LogAwareness");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        character.LogAwarenessList();
        //AddSuccessMessage("Set Mood Value of " + character.name + " to " + moodValue);
    }
    private void AddRelationship(string[] parameters) {
        if (parameters.Length != 3) { //parameters: RELATIONSHIP_TRAIT, Character, Character
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AddRelationship");
            return;
        }
        string typeParameterString = parameters[0];
        RELATIONSHIP_TYPE rel;
        if (!Enum.TryParse<RELATIONSHIP_TYPE>(typeParameterString, out rel)) {
            AddErrorMessage($"There is no relationship of type {typeParameterString}");
        }
        string character1ParameterString = parameters[1];
        string character2ParameterString = parameters[2];

        Character character1 = CharacterManager.Instance.GetCharacterByName(character1ParameterString);
        if (character1 == null) {
            AddErrorMessage($"There is no character with name {character1ParameterString}");
        }
        Character character2 = CharacterManager.Instance.GetCharacterByName(character2ParameterString);
        if (character2 == null) {
            AddErrorMessage($"There is no character with name {character2ParameterString}");
        }
        RelationshipManager.Instance.CreateNewRelationshipBetween(character1, character2, rel);
        AddSuccessMessage($"{character1.name} and {character2.name} now have relationship {rel}");
    }
    private void ForcedRelationshipDegradation(string[] parameters) {
        if (parameters.Length != 2) { //parameters: Character, Character
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ForcedRelationshipDegradation");
            return;
        }
        string character1ParameterString = parameters[0];
        string character2ParameterString = parameters[1];

        Character character1 = CharacterManager.Instance.GetCharacterByName(character1ParameterString);
        if (character1 == null) {
            AddErrorMessage($"There is no character with name {character1ParameterString}");
        }
        Character character2 = CharacterManager.Instance.GetCharacterByName(character2ParameterString);
        if (character2 == null) {
            AddErrorMessage($"There is no character with name {character2ParameterString}");
        }
        RelationshipManager.Instance.RelationshipDegradation(character1, character2);
        AddSuccessMessage(
            $"Relationship degradation between {character1.name} and {character2.name} has been executed.");
    }
    private void SetHP(string[] parameters) {
        if (parameters.Length != 2) { //parameters: Character, hp amount
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ForcedRelationshipDegradation");
            return;
        }
        string characterParameterString = parameters[0];
        string amountParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        int amount = 0;
        if (!int.TryParse(amountParameterString, out amount)) {
            AddErrorMessage($"HP value parameter is not an integer: {amountParameterString}");
            return;
        }
        character.SetHP(amount);
        AddSuccessMessage($"Set HP of {character.name} to {amount}");

    }
    private void ForceUpdateAnimation(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ForceUpdateAnimation");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);
        if (character == null) {
            AddErrorMessage($"There is no character with name {characterParameterString}");
            return;
        }
        character.marker.UpdateAnimation();
    }
    private void AdjustOpinion(string[] parameters) {
        if (parameters.Length != 3) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AdjustOpinion");
            return;
        }
        string character1ParameterString = parameters[0];
        string character2ParameterString = parameters[1];

        Character character1 = CharacterManager.Instance.GetCharacterByName(character1ParameterString);
        Character character2 = CharacterManager.Instance.GetCharacterByName(character2ParameterString);

        if (character1 == null) {
            AddErrorMessage($"There is no character named {character1ParameterString}");
            return;
        }
        if (character2 == null) {
            AddErrorMessage($"There is no character named {character2ParameterString}");
            return;
        }
        string opinionParameterString = parameters[2];

        int value = 0;
        if (!int.TryParse(opinionParameterString, out value)) {
            AddErrorMessage($"Opinion parameter is not an integer: {opinionParameterString}");
            return;
        }
        character1.relationshipContainer.AdjustOpinion(character1, character2, "Base", value);
        AddSuccessMessage($"Adjusted Opinion of {character1.name} towards {character2.name} by {value}");
    }
    private void JoinFaction(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of JoinFaction");
            return;
        }
        string character1ParameterString = parameters[0];
        string factionParameterString = parameters[1];

        Character character1 = CharacterManager.Instance.GetCharacterByName(character1ParameterString);
        Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);

        if (character1 == null) {
            AddErrorMessage($"There is no character named {character1ParameterString}");
            return;
        }
        if (faction == null) {
            AddErrorMessage($"There is no faction named {factionParameterString}");
            return;
        }
        character1.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Faction, faction.characters[0], "join_faction_normal");
        AddSuccessMessage($"{character1.name} joined faction {faction.name}");
    }
    private void TriggerEmotion(string[] parameters) {
        if (parameters.Length != 3) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of TriggerEmotion");
            return;
        }
        string character1ParameterString = parameters[0];
        string character2ParameterString = parameters[1];

        Character character1 = CharacterManager.Instance.GetCharacterByName(character1ParameterString);
        Character character2 = CharacterManager.Instance.GetCharacterByName(character2ParameterString);

        if (character1 == null) {
            AddErrorMessage($"There is no character named {character1ParameterString}");
            return;
        }
        if (character2 == null) {
            AddErrorMessage($"There is no character named {character2ParameterString}");
            return;
        }
        string emotionParameterString = parameters[2];

        Emotion emotion = CharacterManager.Instance.GetEmotion(emotionParameterString);
        if (emotion == null) {
            AddErrorMessage($"Emotion parameter has no data: {emotionParameterString}");
            return;
        }
        CharacterManager.Instance.TriggerEmotion(emotion.emotionType, character1, character2, REACTION_STATUS.INFORMED);
        AddSuccessMessage($"Trigger {emotion.name} Emotion of {character1.name} towards {character2.name}");
    }
    private void ChangeCharacterElementalDamage(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ChangeCharacterElementalDamage");
            return;
        }
        string characterParameterString = parameters[0];
        string elementalParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {

        }
        ELEMENTAL_TYPE elementalType = ELEMENTAL_TYPE.Normal;
        if(!System.Enum.TryParse(elementalParameterString, out elementalType)) {
            AddErrorMessage($"There is no elemental damage type {elementalParameterString}");
            return;
        }

        character.combatComponent.SetElementalType(elementalType);
        AddSuccessMessage($"Changed {character.name} elemental damage to {elementalParameterString}");
    }
    #endregion

    #region Faction
    //private void LogFactionLandmarkInfo(string[] parameters) {
    //    if (parameters.Length != 1) {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of /lfli");
    //        return;
    //    }
    //    string factionParameterString = parameters[0];
    //    int factionID;

    //    bool isFactionParameterNumeric = int.TryParse(factionParameterString, out factionID);
    //    Faction faction = null;
    //    if (isFactionParameterNumeric) {
    //        faction = FactionManager.Instance.GetFactionBasedOnID(factionID);
    //        if (faction == null) {
    //            AddErrorMessage("There was no faction with id " + factionID);
    //            return;
    //        }
    //    } else {
    //       faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);
    //        if (faction == null) {
    //            AddErrorMessage("There was no faction with name " + factionParameterString);
    //            return;
    //        }
    //    }

    //     string text = faction.name + "'s Landmark Info: ";
    //     for (int i = 0; i < faction.landmarkInfo.Count; i++) {
    //         BaseLandmark currLandmark = faction.landmarkInfo[i];
    //text += "\n" + currLandmark.landmarkName + " (" + currLandmark.tileLocation.name + ") ";
    //     }

    //AddSuccessMessage(text);
    //}
    #endregion

    #region NPCSettlement
    private void LogAreaCharactersHistory(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of LogAreaCharactersHistory");
            return;
        }

        //string areaParameterString = parameters[0];
        //int areaID;
        //bool isAreaParameterNumeric = int.TryParse(areaParameterString, out areaID);

        //NPCSettlement npcSettlement = null;
        //if (isAreaParameterNumeric) {
        //    npcSettlement = LandmarkManager.Instance.GetAreaByID(areaID);
        //} else {
        //    npcSettlement = LandmarkManager.Instance.GetAreaByName(areaParameterString);
        //}

        //string text = npcSettlement.name + "'s Characters History: ";
        //for (int i = 0; i < npcSettlement.charactersAtLocationHistory.Count; i++) {
        //    text += "\n" + npcSettlement.charactersAtLocationHistory[i];
        //}
        //AddSuccessMessage(text);
    }
    #endregion

    #region Interactions
    private List<INTERACTION_TYPE> typesSubscribedTo = new List<INTERACTION_TYPE>();
    private void ToggleSubscriptionToInteraction(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SubscribeToInteraction");
            return;
        }

        string typeParameterString = parameters[0];
        INTERACTION_TYPE type;
        if (typeParameterString.Equals("All")) {
            if (typesSubscribedTo.Count > 0) {
                typesSubscribedTo.Clear();
                AddSuccessMessage("Unsubscribed from ALL interactions");
            } else {
                typesSubscribedTo.AddRange(CollectionUtilities.GetEnumValues<INTERACTION_TYPE>());
                AddSuccessMessage("Subscribed to ALL interactions");
            }
        } else if (Enum.TryParse<INTERACTION_TYPE>(typeParameterString, out type)) {
            if (typesSubscribedTo.Contains(type)) {
                typesSubscribedTo.Remove(type);
                AddSuccessMessage($"Unsubscribed from {type} interactions");
            } else {
                typesSubscribedTo.Add(type);
                AddSuccessMessage($"Subscribed to {type} interactions");
            }
        } else {
            AddErrorMessage($"There is no interaction of type {typeParameterString}");
        }
    }
    #endregion

    #region Minion
    private void InitializeMinion() {
        levelInput.text = "1";
        ConstructRaceDropdown();
        ConstructClassDropdown();
    }
    private void ConstructRaceDropdown() {
        raceDropdown.ClearOptions();
        string[] races = System.Enum.GetNames(typeof(RACE));
        raceDropdown.AddOptions(races.ToList());
    }
    private void ConstructClassDropdown() {
        List<string> allClasses = new List<string>();
        string path = $"{UtilityScripts.Utilities.dataPath}CharacterClasses/";
        foreach (string file in Directory.GetFiles(path, "*.json")) {
            allClasses.Add(Path.GetFileNameWithoutExtension(file));
        }
        classDropdown.ClearOptions();
        classDropdown.AddOptions(allClasses);
    }
    public void AddMinion() {
        RACE race = (RACE)System.Enum.Parse(typeof(RACE), raceDropdown.options[raceDropdown.value].text);
        string className = classDropdown.options[classDropdown.value].text;
        int level = int.Parse(levelInput.text);
        if (race != RACE.NONE) {
            Minion minion = PlayerManager.Instance.player.CreateNewMinion(className, race);
            //if (level > 1) {
            //    minion.character.LevelUp(level - 1);
            //}
            PlayerManager.Instance.player.AddMinion(minion);
        }
    }
    #endregion

    #region Summons
    private void GainSummon(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of GainSummon");
            return;
        }

        string typeParameterString = parameters[0];
        SUMMON_TYPE type;
        //if (typeParameterString.Equals("All")) {
        //    SUMMON_TYPE[] types = CollectionUtilities.GetEnumValues<SUMMON_TYPE>();
        //    for (int i = 1; i < types.Length; i++) {
        //        PlayerManager.Instance.player.AddSummon(types[i]);
        //    }
        //} else if (Enum.TryParse(typeParameterString, out type)) {
        //    PlayerManager.Instance.player.AddSummon(type);
        //    AddSuccessMessage($"Gained new summon: {type}");
        //} else {
        //    AddErrorMessage($"There is no summon of type {typeParameterString}");
        //}
    }
    //private void GainSummonSlot (string[] parameters) {
    //    if (parameters.Length != 1) {
    //        AddCommandHistory(consoleLbl.text);
    //        AddErrorMessage("There was an error in the command format of GainSummonSlot");
    //        return;
    //    }
    //    string numParameterString = parameters[0];
    //    int num;
    //    if (int.TryParse(numParameterString, out num)) {
    //        for (int i = 0; i < num; i++) {
    //            PlayerManager.Instance.player.IncreaseSummonSlot();
    //        }
    //        AddSuccessMessage("Gained summon slot/s: " + num);
    //    } else {
    //        AddErrorMessage("Cannot parse input: " + numParameterString);
    //    }
    //}
    #endregion

    #region Artifacts
    private void GainArtifact(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of GainSummon");
            return;
        }

        string typeParameterString = parameters[0];
        ARTIFACT_TYPE type;
        if (typeParameterString.Equals("All")) {
            ARTIFACT_TYPE[] types = CollectionUtilities.GetEnumValues<ARTIFACT_TYPE>();
            for (int i = 1; i < types.Length; i++) {
                PlayerManager.Instance.player.AddArtifact(types[i]);
            }
        } else if (Enum.TryParse(typeParameterString, out type)) {
            PlayerManager.Instance.player.AddArtifact(type);
            AddSuccessMessage($"Gained new artifact: {type}");
        } else {
            AddErrorMessage($"There is no artifact of type {typeParameterString}");
        }

    }
    // private void GainArtifactSlot(string[] parameters) {
    //     if (parameters.Length != 1) {
    //         AddCommandHistory(consoleLbl.text);
    //         AddErrorMessage("There was an error in the command format of GainArtifactSlot");
    //         return;
    //     }
    //     string numParameterString = parameters[0];
    //     int num;
    //     if (int.TryParse(numParameterString, out num)) {
    //         for (int i = 0; i < num; i++) {
    //             PlayerManager.Instance.player.IncreaseArtifactSlot();
    //         }
    //         AddSuccessMessage("Gained artifact slot/s: " + num);
    //     } else {
    //         AddErrorMessage("Cannot parse input: " + numParameterString);
    //     }
    // }
    #endregion

    #region Player
    private void GainInterventionAbility(string[] parameters) {
        if (parameters.Length != 1) { //intervention ability
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of GainInterventionAbility");
            return;
        }
        string typeParameterString = parameters[0];
        SPELL_TYPE type;
        if (Enum.TryParse(typeParameterString, out type)) {
            PlayerManager.Instance.player.GainNewInterventionAbility(type, true);
            AddSuccessMessage($"Gained new Spell: {type}");
        } else {
            AddErrorMessage($"There is no spell of type {typeParameterString}");
        }

    }
    private void ChangeArchetype(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ChangeArchetype");
            return;
        }
        string typeParameterString = parameters[0];
        PLAYER_ARCHETYPE type;
        if (Enum.TryParse(typeParameterString, out type)) {
            //PlayerManager.Instance.player.SetArchetype(type);
            AddSuccessMessage($"Changed Player Archetype to: {type}");
        } else {
            AddErrorMessage($"There is no archetype {typeParameterString}");
        }

    }
    #endregion

    #region Tile Objects
    /// <summary>
    /// Parameters: TILE_OBJECT_TYPE, int id
    /// </summary>
    private void DestroyTileObj(string[] parameters) {
        if (parameters.Length != 2) { 
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of DestroyTileObj");
            return;
        }
        string typeParameterString = parameters[0];
        string idParameterString = parameters[1];
        int id = System.Int32.Parse(idParameterString);
        TILE_OBJECT_TYPE type;
        if (Enum.TryParse(typeParameterString, out type)) {
            for (int i = 0; i < GridMap.Instance.allRegions.Length; i++) {
                Region currRegion = GridMap.Instance.allRegions[i];
                List<TileObject> objs = currRegion.GetTileObjectsOfType(type);
                for (int j = 0; j < objs.Count; j++) {
                    TileObject currObj = objs[j];
                    if (currObj.id == id) {
                        AddSuccessMessage(
                            $"Removed {currObj} from {currObj.gridTileLocation} at {currObj.gridTileLocation.structure}");
                        currObj.gridTileLocation.structure.RemovePOI(currObj);
                        break;
                    }
                }
            }
        } else {
            AddErrorMessage($"There is no tile object of type {typeParameterString}");
        }
    }
    #endregion

    #region NPCSettlement Map
    private void HighlightStructureTiles(string[] parameters) {
        if (parameters.Length != 3) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of HighlightStructureTiles");
            return;
        }
        string typeParameterString = parameters[0];
        string idParameterString = parameters[1];
        string highlightParameterString = parameters[2];
        int id = System.Int32.Parse(idParameterString);
        bool highlight = System.Boolean.Parse(highlightParameterString);

        STRUCTURE_TYPE structureType;
        if (System.Enum.TryParse(typeParameterString, out structureType) == false) {
            AddErrorMessage($"There is no structure type named {typeParameterString}");
            return;
        }

        LocationStructure structure = InnerMapManager.Instance.currentlyShowingMap.region.GetStructureByID(structureType, id);
        if (structure == null) {
            AddErrorMessage($"There is no {structureType} with id {id}");
            return;
        }

        for (int i = 0; i < structure.tiles.Count; i++) {
            LocationGridTile tile = structure.tiles[i];
            if (highlight) {
                tile.HighlightTile();
            } else {
                tile.UnhighlightTile();
            }
            
        }
    }
    #endregion

    #region IPointOfInterest
    private void LogObjectAdvertisements(string[] parameters) {
        if (parameters.Length != 3) { //POI Type, Object Type (TILE_OBJECT, SPECIAL_TOKEN), id 
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of LogObjectAdvertisments");
            return;
        }

        string poiTypeStr = parameters[0];
        string objTypeStr = parameters[1];
        string idStr = parameters[2];

        POINT_OF_INTEREST_TYPE poiType;
        if (System.Enum.TryParse(poiTypeStr, out poiType) == false) {
            AddErrorMessage($"There is no poi of type {poiTypeStr}");
        }
        int id = Int32.Parse(idStr);
        if (poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            TILE_OBJECT_TYPE tileObjType;
            if (System.Enum.TryParse(objTypeStr, out tileObjType) == false) {
                AddErrorMessage($"There is no tile object of type {objTypeStr}");
            }

            TileObject tileObj = InnerMapManager.Instance.GetTileObject(tileObjType, id);
            string log = $"Advertised actions of {tileObj.name}:";
            if(tileObj.advertisedActions != null && tileObj.advertisedActions.Count > 0) {
                for (int i = 0; i < tileObj.advertisedActions.Count; i++) {
                    log += $"\n{tileObj.advertisedActions[i]}";
                }
            } else {
                log += $"\nNone";
            }

            AddSuccessMessage(log);
        } 
        // else if (poiType == POINT_OF_INTEREST_TYPE.ITEM) {
        //     SPECIAL_TOKEN specialTokenType;
        //     if (System.Enum.TryParse(objTypeStr, out specialTokenType) == false) {
        //         AddErrorMessage("There is no special token of type " + objTypeStr);
        //     }
        //     SpecialToken st = TokenManager.Instance.GetSpecialTokenByID(id);
        //     string log = $"Advertised actions of {st.name}:";
        //     for (int i = 0; i < st.advertisedActions.Count; i++) {
        //         log += "\n" + st.advertisedActions[i].ToString();
        //     }
        //     AddSuccessMessage(log);
        // }
    }
    #endregion

    #region Tutorial
    public void ResetTutorial() {
        TutorialManager.Instance.ResetTutorials();
    }
    #endregion
}
