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
using Locations.Settlements;
using Object_Pools;
using Character_Talents;
using Random = System.Random;

public class ConsoleBase : InfoUIBase {

    private Dictionary<string, Action<string[]>> _consoleActions;

    public static bool showPOIHoverData = true;
    
    private List<string> commandHistory;
    //private int currentHistoryIndex;

    [SerializeField] private GameObject consoleGO;
    [SerializeField] private Text consoleLbl;
    [SerializeField] private InputField consoleInputField;

    [SerializeField] private GameObject commandHistoryGO;
    [SerializeField] private TextMeshProUGUI commandHistoryLbl;

    [SerializeField] private GameObject fullDebugGO;
    [SerializeField] private TextMeshProUGUI fullDebugLbl;
    [SerializeField] private TextMeshProUGUI fullDebug2Lbl;

    [SerializeField] private Toggle tglAlwaysSuccessScheme;
    [SerializeField] private Toggle tglShowPOIHoverData;
    [SerializeField] private ChanceTheWrapper chanceTheWrapper;

    void Awake() {
        Initialize();
    }

    internal override void Initialize() {
        commandHistory = new List<string>();
        _consoleActions = new Dictionary<string, Action<string[]>>() {
            {"/help", ShowHelp},
            {"/set_faction_rel", ChangeFactionRelationshipStatus},
            {"/kill",  KillCharacter},
            //{"/lfli", LogFactionLandmarkInfo},
            {"/center_character", CenterOnCharacter},
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
            {"/set_comfort", SetStamina },
            {"/set_hope", SetHope },
            {"/gain_i_ability", GainInterventionAbility },
            {"/destroy_tile_obj", DestroyTileObj },
            {"/force_update_animation", ForceUpdateAnimation },
            {"/log_obj_advertisements", LogObjectAdvertisements },
            {"/adjust_opinion", AdjustOpinion },
            {"/join_faction", JoinFaction },
            {"/emotion", TriggerEmotion },
            // {"/adjust_resource", TriggerEmotion },
            {"/change_archetype", ChangeArchetype },
            {"/elemental_damage", ChangeCharacterElementalDamage },
            {"/add_item", AddItemToCharacter },
            {"/null_home", ChangeCharacterHomeToNull },
            {"/damage_tile", DamageTile },
            {"/create_faction", CreateFaction },
            {"/cancel_job", CancelJob },
            {"/save_scenario", SaveScenarioMap },
            {"/save_manual", SaveManual },
            {"/set_party_state", SwitchPartyState },
            {"/raid", StartRaid },
            {"/save_db", SaveDatabaseInMemory},
            {"/find_object", FindTileObject},
            {"/change_name", ChangeName},
            {"/adjust_mana", AdjustMana},
            {"/adjust_pp", AdjustPlaguePoints},
            {"/remove_needed_class", RemoveNeededClassFromSettlement},
            {"/activate_settlement_event", ActivateSettlementEvent},
            {"/trigger_quarantine", TriggerQuarantine},
            {"/add_ideology", AddFactionIdeology},
            {"/check_tiles", CheckTiles},
            {"/reveal_all", RevealAll},
            {"/enable_dig", EnableDigging},
            {"/bonus_charge", BonusCharges},
            {"/log_alive_villagers", LogAliveVillagers},
            {"/kill_villagers", KillAllVillagers},
            {"/adjust_se", AdjustSpiritEnergy},
            {"/adjust_mm", AdjustMigrationMeter},
            {"/toggle_vs", ToggleVillageSpots},
            {"/coins", AdjustCoins},
            {"/talent_level_up", TalentLevelUp},
            {"/adjust_resistance", AdjustResistance},
            {"/log_structure_connectors", LogStructureConnectors}
        };
        
        SchemeData.alwaysSuccessScheme = false;
        tglAlwaysSuccessScheme.SetIsOnWithoutNotify(SchemeData.alwaysSuccessScheme);
        tglAlwaysSuccessScheme.onValueChanged.RemoveAllListeners();
        tglAlwaysSuccessScheme.onValueChanged.AddListener(OnToggleAlwaysSuccessScheme);
        chanceTheWrapper.Initialize();
        
        tglShowPOIHoverData.SetIsOnWithoutNotify(showPOIHoverData);
        tglShowPOIHoverData.onValueChanged.RemoveAllListeners();
        tglShowPOIHoverData.onValueChanged.AddListener(OnToggleShowPOIHoverData);
        
        // Messenger.AddListener(Signals.TICK_STARTED, OnTickStarted);
    }
    // private void OnTickStarted() {
    //     for (int i = 0; i < DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements.Count; i++) {
    //         NPCSettlement settlement = DatabaseManager.Instance.settlementDatabase.allNonPlayerSettlements[i];
    //         if (settlement.SettlementResources != null) {
    //             for (int j = 0; j < settlement.SettlementResources.resourcePiles.Count; j++) {
    //                 ResourcePile resourcePile = settlement.SettlementResources.resourcePiles[j];
    //                 if (resourcePile.gridTileLocation == null) {
    //                     Debug.LogError($"Resource pile {resourcePile.nameWithID} is in resource list of {settlement.name} but has no grid tile location!");
    //                 }
    //             }
    //         }
    //     }
    // }
    private void Update() {
        if (!isShowing) {
            return;
        }
        fullDebugLbl.text = string.Empty;
        fullDebug2Lbl.text = string.Empty;
        string worldSettingsText = $"World Settings:";
        worldSettingsText = $"{worldSettingsText}\nMigration: {WorldSettings.Instance.worldSettingsData.villageSettings.migrationSpeed.ToString()}";
        worldSettingsText = $"{worldSettingsText}\nCooldown: {WorldSettings.Instance.worldSettingsData.playerSkillSettings.cooldownSpeed.ToString()}";
        worldSettingsText = $"{worldSettingsText}\nCosts: {WorldSettings.Instance.worldSettingsData.playerSkillSettings.costAmount.ToString()}";
        worldSettingsText = $"{worldSettingsText}\nCharges: {WorldSettings.Instance.worldSettingsData.playerSkillSettings.chargeAmount.ToString()}";
        worldSettingsText = $"{worldSettingsText}\nThreat: {WorldSettings.Instance.worldSettingsData.playerSkillSettings.retaliation.ToString()}";

        worldSettingsText = $"{worldSettingsText}\nPathfinding:";
        if (AstarPath.active.graphs.Length > 0) {
            worldSettingsText = $"{worldSettingsText}\nTotal Nodes: {AstarPath.active.graphs[0].CountNodes().ToString()}";    
        }
        
        worldSettingsText = $"{worldSettingsText}\n\nObject Pooling:";
        worldSettingsText = $"{worldSettingsText}\n\tLogs in Pool:";
        worldSettingsText = $"{worldSettingsText} {LogPool.GetCurrentLogsInPool().ToString()}";
        
        
        
        
        // if (GameManager.Instance.showFullDebug) {
        //     FullDebugInfo();
        // }
        fullDebugLbl.text = worldSettingsText;
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
        text += $"\n\t<b>Previous Grid Location:</b> {character.marker?.previousGridTile.localPlace}" ?? "None";


        text += $"\n<b>Faction:</b> {character.faction?.name}" ?? "None";
        text += $"\n<b>Current Action:</b> {character.currentActionNode?.goapName}" ?? "None";
        //if (character.currentActionNode != null) {
        //    text += "\n<b>Current Plan:</b> " + character.currentActionNode.parentPlan.GetGoalSummary();
        //}
        text += $"\n<b>Is Travelling In World:</b> {character.carryComponent.masterCharacter.movementComponent.isTravellingInWorld}";

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
    public void ToggleShowAccumulatedDamage(bool state) {
        PlayerUI.Instance.accumulatedDamageGO.SetActive(state);
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

        Faction faction1 = null;
        Faction faction2 = null;

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

        AddSuccessMessage($"Changed relationship status of {faction1.name} and {faction2.name} to {rel.relationshipStatus}");
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
    private void SetStamina(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SetStamina");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string staminaParameterString = parameters[1];

        float stamina = character.needsComponent.stamina;
        if (!float.TryParse(staminaParameterString, out stamina)) {
            AddErrorMessage($"Stamina parameter is not a float: {staminaParameterString}");
            return;
        }
        character.needsComponent.SetStamina(stamina);
        AddSuccessMessage($"Set Stamina Value of {character.name} to {stamina}");
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
    private void CreateFaction(string[] parameters) {
        if (parameters.Length != 1) { //parameters character
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of JoinFaction");
            return;
        }
        string character1ParameterString = parameters[0];

        Character character1 = CharacterManager.Instance.GetCharacterByName(character1ParameterString);

        if (character1 == null) {
            AddErrorMessage($"There is no character named {character1ParameterString}");
            return;
        }
        character1.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Faction, character1);
        AddSuccessMessage($"{character1.name} created faction {character1.faction.name}");
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
    private void ChangeCharacterHomeToNull(string[] parameters) {
        if (parameters.Length != 1) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ChangeCharacterHomeToNull");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        character.MigrateHomeTo(null);
        AddSuccessMessage($"Changed {character.name} home to null");
    }
    private void CancelJob(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of CancelJob");
            return;
        }
        string characterParameterString = parameters[0];
        string jobParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        //if (AttributeManager.Instance.allTraits.ContainsKey(traitParameterString)) {
        JobQueueItem job = character.jobQueue.GetJobByName(jobParameterString);
        if(job != null) {
            job.CancelJob();
            AddSuccessMessage($"Cancelled job {jobParameterString} of {character.name}");
        }
    }
    private void ChangeName(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of ChangeName");
            return;
        }
        string nameParameterString = parameters[0];
        string newName = parameters[1];
        Character character = CharacterManager.Instance.GetCharacterByName(nameParameterString);
        if (character != null) {
            string previousName = character.name;
            character.SetFirstAndLastName(newName, character.surName);
            AddSuccessMessage($"Successfully set name of {previousName} to {character.name}");
        } else {
            AddErrorMessage($"Could not find character named {nameParameterString}");
        }
    }
    private void LogAliveVillagers(string[] parameters) {
        string message = DatabaseManager.Instance.characterDatabase.aliveVillagersList.ComafyList();
        AddSuccessMessage(message);
        Debug.Log(message);
    }
    private void KillAllVillagers(string[] obj) {
        int count = 0;
        for (int i = 0; i < DatabaseManager.Instance.characterDatabase.aliveVillagersList.Count; i++) {
            Character villager = DatabaseManager.Instance.characterDatabase.aliveVillagersList[i];
            if (!villager.isDead) {
                villager.Death();
                count++;
                i--;    
            }
        }
        AddSuccessMessage($"Killed {count} villagers!");
    }
    private void AdjustCoins(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AdjustCoins");
            return;
        }
        string characterParameterString = parameters[0];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }
        string amountParameterString = parameters[1];

        int value = 0;
        if (!int.TryParse(amountParameterString, out value)) {
            AddErrorMessage($"Amount parameter is not an integer: {amountParameterString}");
            return;
        }
        character.moneyComponent.AdjustCoins( value);
        AddSuccessMessage($"Adjusted Coins of {character.name} by {value}");
    }
    private void TalentLevelUp(string[] parameters) {
        if (parameters.Length != 2) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of TalentLevelUp");
            return;
        }
        string characterParameterString = parameters[0];
        string talentParameterString = parameters[1];

        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        CHARACTER_TALENT type;
        if (!Enum.TryParse(talentParameterString, out type)) {
            AddErrorMessage($"There is no talent {talentParameterString}");
        }
        if (character.HasTalents()) {
            CharacterTalent talent = character.talentComponent.GetTalent(type);
            talent.LevelUp(character);
            AddSuccessMessage($"{character.name}'s {talentParameterString} is leveled up!");
        }
    }
    private void Distance(string[] parameters) {
        string parameter1 = parameters[0];
        string parameter2 = parameters[1];
        string parameter3 = parameters[2];
        string parameter4 = parameters[3];

        LocationGridTile tile1 = InnerMapManager.Instance.currentlyShowingLocation.innerMap.map[int.Parse(parameter1), int.Parse(parameter2)];
        LocationGridTile tile2 = InnerMapManager.Instance.currentlyShowingLocation.innerMap.map[int.Parse(parameter3), int.Parse(parameter4)];

        float distance = tile1.GetDistanceTo(tile2);
        AddSuccessMessage($"Distance: {distance}");
    }
    #endregion

	#region adjust resistance
	private void AdjustResistance(string[] parameters) {
        if (parameters.Length != 3) { //parameters command, item
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of adjust resistance");
            return;
        }
        string characterParameterString = parameters[0];
        string element = parameters[1];
        float count = 0;
        Character character = CharacterManager.Instance.GetCharacterByName(characterParameterString);

        if (character == null) {
            AddErrorMessage($"There is no character named {characterParameterString}");
            return;
        }

        if (!float.TryParse(parameters[2], out count)) {
            AddErrorMessage($"3rd parameter should be a number");
            return;
        }

		switch (element) {
            //case "normal":
            //character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Normal, count);
            //break;
            case "fire":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Fire, count);
            break;
            case "water":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Water, count);
            break;
            case "wind":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Wind, count);
            break;
            case "poison":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Poison, count);
            break;
            case "mental":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Mental, count);
            break;
            case "physical":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Physical, count);
            break;
            case "ice":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Ice, count);
            break;
            case "earth":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Earth, count);
            break;
            case "electric":
            character.piercingAndResistancesComponent.AdjustResistance(RESISTANCE.Electric, count);
            break;
            default:
            AddErrorMessage($"There is no such element {element}");
            return;
        }

        AddSuccessMessage($"{character.name}'s {element} Resistance added {count}");
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
	private void AddFactionIdeology(string[] parameters) {
        if (parameters.Length < 2) { //Faction, Ideology
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /add_ideology");
            return;
        }
        
        string factionParameterString = parameters[0];
        string ideologyStr = parameters[1];
        Faction faction = FactionManager.Instance.GetFactionBasedOnName(factionParameterString);
        if (faction == null) {
            AddErrorMessage($"Could not find faction with name {factionParameterString}");
            return;
        }
        if (Enum.TryParse(ideologyStr, out FACTION_IDEOLOGY ideology)) {
            FactionIdeology factionIdeology = FactionManager.Instance.CreateIdeology<FactionIdeology>(ideology);
            if (factionIdeology is Exclusive exclusive) {
                exclusive.SetRequirement(GENDER.MALE);
                faction.factionType.RemoveIdeology(FACTION_IDEOLOGY.Exclusive);
            }
            faction.factionType.AddIdeology(factionIdeology);
            //check if faction characters still meets ideology requirements
            List<Character> charactersToCheck = ObjectPoolManager.Instance.CreateNewCharactersList();
            charactersToCheck.AddRange(faction.characters);
            for (int i = 0; i < charactersToCheck.Count; i++) {
                Character factionMember = charactersToCheck[i];
                faction.CheckIfCharacterStillFitsIdeology(factionMember);
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(charactersToCheck);
        } else {
            AddErrorMessage($"Could not find ideology named {ideologyStr}");
        }
        
    }
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
            //for (int i = 1; i < types.Length; i++) {
            //    PlayerManager.Instance.player.AddArtifact(types[i]);
            //}
        } else if (Enum.TryParse(typeParameterString, out type)) {
            //PlayerManager.Instance.player.AddArtifact(type);
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
        PLAYER_SKILL_TYPE type;
        if (Enum.TryParse(typeParameterString, out type)) {
            //PlayerManager.Instance.player.GainNewInterventionAbility(type, true);
            //AddSuccessMessage($"Gained new Spell: {type}");
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
    private void AdjustMana(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AdjustMana");
            return;
        }
        string valueParameterStr = parameters[0];
        if (Int32.TryParse(valueParameterStr, out var value)) {
            PlayerManager.Instance.player.AdjustManaNoLimit(value);
            AddSuccessMessage($"Adjusted mana by {value}. New Mana is {PlayerManager.Instance.player.mana}");
        } else {
            AddErrorMessage($"Could not parse value {valueParameterStr} to an integer.");
        }

    }
    private void AdjustPlaguePoints(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AdjustMana");
            return;
        }
        string valueParameterStr = parameters[0];
        if (Int32.TryParse(valueParameterStr, out var value)) {
            PlayerManager.Instance.player.plagueComponent.AdjustPlaguePointsNoLimit(value);
            AddSuccessMessage($"Adjusted Chaotic Energy by {value}. New Chaotic Energy is {PlayerManager.Instance.player.plagueComponent.plaguePoints}");
        } else {
            AddErrorMessage($"Could not parse value {valueParameterStr} to an integer.");
        }

    }
    private void AdjustSpiritEnergy(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of AdjustSpiritEnergy");
            return;
        }
        string valueParameterStr = parameters[0];
        if (Int32.TryParse(valueParameterStr, out var value)) {
            PlayerManager.Instance.player.AdjustSpiritEnergy(value);
            AddSuccessMessage($"Adjusted spirit energy by {value.ToString()}. New Spirit Energy is {PlayerManager.Instance.player.spiritEnergy.ToString()}");
        } else {
            AddErrorMessage($"Could not parse value {valueParameterStr} to an integer.");
        }
    }
    private void RevealAll(string[] parameters) {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            character.isInfoUnlocked = true;
        }
        for (int i = 0; i < FactionManager.Instance.allFactions.Count; i++) {
            Faction faction = FactionManager.Instance.allFactions[i];
            if (faction.isMajorFaction) {
                faction.isInfoUnlocked = true;
            }
        }
        AddSuccessMessage($"Revealed all Character and Faction Info");
    }
    private void EnableDigging(string[] parameters) {
        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
            Character character = CharacterManager.Instance.allCharacters[i];
            character.movementComponent.SetEnableDigging(!character.movementComponent.enableDigging);
        }
        for (int i = 0; i < CharacterManager.Instance.limboCharacters.Count; i++) {
            Character character = CharacterManager.Instance.limboCharacters[i];
            character.movementComponent.SetEnableDigging(!character.movementComponent.enableDigging);
        }
        AddSuccessMessage($"Enabled Digging all Characters");
    }
    private void BonusCharges(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of BonusCharges");
            return;
        }
        string typeParameterString = parameters[0];
        string amount = parameters[1];
        PLAYER_SKILL_TYPE type;
        if (Enum.TryParse(typeParameterString, out type)) {
            SkillData data = PlayerSkillManager.Instance.GetSkillData(type);
            data.AdjustBonusCharges(int.Parse(amount));
        } else {
            AddErrorMessage($"There is no skill of type {typeParameterString}");
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
                List<TileObject> objs = RuinarchListPool<TileObject>.Claim();
                currRegion.PopulateTileObjectsOfType(objs, type);
                for (int j = 0; j < objs.Count; j++) {
                    TileObject currObj = objs[j];
                    if (currObj.id == id) {
                        AddSuccessMessage(
                            $"Removed {currObj} from {currObj.gridTileLocation} at {currObj.gridTileLocation.structure}");
                        currObj.gridTileLocation.structure.RemovePOI(currObj);
                        break;
                    }
                }
                RuinarchListPool<TileObject>.Release(objs);
            }
        } else {
            AddErrorMessage($"There is no tile object of type {typeParameterString}");
        }
    }
    private void FindTileObject(string[] parameters) {
        if (parameters.Length != 1) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of FindTileObject");
            return;
        }
        string nameParameterString = parameters[0];
        Region currentRegion = InnerMapManager.Instance.currentlyShowingLocation;
        if(currentRegion != null) {
            List<LocationStructure> structures = currentRegion.allStructures;
            for (int i = 0; i < structures.Count; i++) {
                LocationStructure structure = structures[i];
                TileObject tileObj = structure.GetFirstTileObjectOfTypeWithName<TileObject>(nameParameterString);

                if (tileObj != null) {
                    UIManager.Instance.ShowTileObjectInfo(tileObj);
                }
            }
        }
    }
    private void SwitchPartyState(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of SwitchPartyState");
            return;
        }
        string nameParameterString = parameters[0];
        string stateParameterString = parameters[1];
        Party party = DatabaseManager.Instance.partyDatabase.GetPartyByName(nameParameterString);
        PARTY_STATE partyState;
        if (System.Enum.TryParse(stateParameterString, out partyState) == false) {
            AddErrorMessage($"There is no poi of type {stateParameterString}");
        } else {
            party.SetPartyState(partyState);
        }
    }
    private void StartRaid(string[] parameters) {
        if (parameters.Length != 2) {
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of StartRaid");
            return;
        }
        string targetParameterString = parameters[0];
        string characterNameParameterString = parameters[1];
        Character character = CharacterManager.Instance.GetCharacterByName(characterNameParameterString);
        if(character != null) {
            BaseSettlement targetSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(targetParameterString);
            character.interruptComponent.SetRaidTargetSettlement(targetSettlement);
            character.interruptComponent.TriggerInterrupt(INTERRUPT.Declare_Raid, character);
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
        // TutorialManager.Instance.ResetTutorials();
    }
    #endregion

    #region Tiles
    private void DamageTile(string[] parameters) {
        if (parameters.Length != 3) { //region, tile coordinates (x, y)
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of DamageTile");
            return;
        }
        
        string regionName = parameters[0];
        Region region = GridMap.Instance.GetRegionByName(regionName);
        if (region == null) {
            AddErrorMessage($"There is no region named {regionName}!");
            return;
        }
        string xString = parameters[1];
        int x;
        if (Int32.TryParse(xString, out x) == false) {
            AddErrorMessage($"{xString} is not an integer!");
            return;
        }
        
        string yString = parameters[2];
        int y;
        if (Int32.TryParse(yString, out y) == false) {
            AddErrorMessage($"{yString} is not an integer!");
            return;
        }

        if (UtilityScripts.Utilities.IsInRange(x, 0, region.innerMap.width) && 
            UtilityScripts.Utilities.IsInRange(y, 0, region.innerMap.height)) {
            LocationGridTile tile = region.innerMap.map[x, y];
            tile.tileObjectComponent.genericTileObject.AdjustHP(-tile.tileObjectComponent.genericTileObject.maxHP, ELEMENTAL_TYPE.Normal);
            AddSuccessMessage($"Successfully damaged {tile.localPlace.ToString()}!");    
        }
        else {
            AddErrorMessage($"No tile with coordinates {x.ToString()},{y.ToString()} at {region.name} was found!");
        }
    }
    private void CheckTiles(string[] parameters) {
        for (int i = 0; i < GridMap.Instance.mainRegion.innerMap.allTiles.Count; i++) {
            LocationGridTile tile = GridMap.Instance.mainRegion.innerMap.allTiles[i];
            if (tile.structure == null) {
                AddErrorMessage($"{tile.ToString()} has no structure!");
            }
        }
    }
    private void ToggleVillageSpots(string[] obj) {
        if (GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.activeInHierarchy) {
            //deactivate
            GridMap.Instance.mainRegion.innerMap.perlinTilemap.ClearAllTiles();
            GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.SetActive(false);
        } else {
            //activate
            GridMap.Instance.mainRegion.innerMap.perlinTilemap.gameObject.SetActive(true);
            for (int i = 0; i < GridMap.Instance.mainRegion.villageSpots.Count; i++) {
                VillageSpot villageSpot = GridMap.Instance.mainRegion.villageSpots[i];
                Color color;
                if (i == 0) {
                    color = Color.red;
                } else if (i == 1) {
                    color = Color.cyan;
                } else if (i == 2) {
                    color = Color.magenta;
                } else if (i == 3) {
                    color = Color.yellow;
                } else if (i == 4) {
                    color = Color.blue;
                } else if (i == 5) {
                    color = Color.green;
                } else if (i == 6) {
                    color = Color.gray;
                } else {
                    color = UnityEngine.Random.ColorHSV();
                }
                villageSpot.ColorVillageSpots(color);
                // Color color = Color.black;
                // color.a = 0.5f;
                // villageSpot.ColorArea(villageSpot.mainSpot, color);
            }
        }
    }
    #endregion

    #region Saving
    private void SaveScenarioMap(string[] parameters) {
        string customFileName = string.Empty;
        if (parameters.Length > 0) {
            customFileName = parameters[0];
        }
        SaveManager.Instance.SaveScenario(customFileName);
    }
    private void SaveManual(string[] parameters) {
        string customFileName = string.Empty;
        if (parameters.Length > 0) {
            customFileName = parameters[0];
        }
        if (string.IsNullOrEmpty(customFileName)) {
            customFileName = SaveCurrentProgressManager.savedCurrentProgressFileName;
        }
        
        SaveManager.Instance.saveCurrentProgressManager.DoManualSave(customFileName);
    }
    private void SaveDatabaseInMemory(string[] parameters) {
        DatabaseManager.Instance.mainSQLDatabase.SaveInMemoryDatabaseToFile($"{UtilityScripts.Utilities.gameSavePath}/Temp/gameDB.db");
    }
    #endregion

    #region Settlements
    private void LogStructureConnectors(string[] parameters) {
        if (parameters.Length != 2) { //Settlement, STRUCTURE_TYPE
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /log_structure_connectors");
            return;
        }
        string settlementName = parameters[0];
        string structureName = parameters[1];
        BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(settlementName);
        if (settlement is NPCSettlement npcSettlement) {
            if (Enum.TryParse(structureName, true, out STRUCTURE_TYPE structureType)) {
                List<StructureConnector> connectors = RuinarchListPool<StructureConnector>.Claim();
                npcSettlement.PopulateStructureConnectorsForStructureType(connectors, structureType);
                if (structureType == STRUCTURE_TYPE.MINE) {
                    connectors = connectors.OrderBy(c => Vector2.Distance(c.transform.position, 
                        npcSettlement.cityCenter.tiles.ElementAt(0).centeredWorldLocation)).ToList();
                }
                Debug.Log($"Found structure connectors for {structureType.ToString()} at {npcSettlement.name} are:\n {connectors.ComafyList()}");
                RuinarchListPool<StructureConnector>.Release(connectors);
                AddSuccessMessage($"Logged structure connectors for {structureType.ToString()} at {settlement.name}. Check your console.");
            } else {
                AddErrorMessage($"Could not parse {structureName} into a STRUCTURE_TYPE");  
            }
        } else {
            AddErrorMessage($"Could not find NPCSettlement with name {settlementName}");
        }
    }
    private void RemoveNeededClassFromSettlement(string[] parameters) {
        if (parameters.Length != 2) { //Settlement, class name
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /remove_needed_class");
            return;
        }
        string settlementName = parameters[0];
        string className = parameters[1];
        BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(settlementName);
        if (settlement is NPCSettlement npcSettlement) {
            //npcSettlement.settlementClassTracker.RemoveNeededClass(className);
            AddSuccessMessage($"Removed needed class {className} from {settlement.name}'s needed classes");
        }
        else {
            AddErrorMessage($"Could not find NPCSettlement with name {settlementName}");
        }
    }
    private void ActivateSettlementEvent(string[] parameters) {
        if (parameters.Length != 2) { //Settlement, SETTLEMENT_EVENT
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /activate_settlement_event");
            return;
        }
        string settlementName = parameters[0];
        string settlementEventTypeStr = parameters[1];
        if (Enum.TryParse(settlementEventTypeStr, out SETTLEMENT_EVENT settlementEvent)) {
            BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(settlementName);
            if (settlement is NPCSettlement npcSettlement) {
                npcSettlement.eventManager.AddNewActiveEvent(settlementEvent);
                AddSuccessMessage($"Activated event {settlementEvent.ToString()} at {settlement.name}");
            } else {
                AddErrorMessage($"Could not find NPCSettlement with name {settlementName}");
            }    
        } else {
            AddErrorMessage($"No Settlement Event Type {settlementEventTypeStr}");
        }
    }
    private void TriggerQuarantine(string[] parameters) {
        if (parameters.Length != 2) { //Settlement, Character
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /trigger_quarantine");
            return;
        }
        
        string settlementName = parameters[0];
        string characterName = parameters[1];
        BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(settlementName);
        if (settlement is NPCSettlement npcSettlement) {
            Character character = CharacterManager.Instance.GetCharacterByName(characterName);
            if (character != null) {
                npcSettlement.settlementJobTriggerComponent.TriggerQuarantineJob(character);    
            } else {
                AddErrorMessage($"Could not find character with name {characterName}");
            }
        } else {
            AddErrorMessage($"Could not find NPCSettlement with name {settlementName}");
        }
        
    }
    private void AdjustMigrationMeter(string[] parameters) {
        if (parameters.Length != 2) { //Settlement, amount
            AddCommandHistory(consoleLbl.text);
            AddErrorMessage("There was an error in the command format of /adjust_mm");
            return;
        }
        string settlementName = parameters[0];
        string migrationAdjustmentStr = parameters[1];
        BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByName(settlementName);
        if (settlement is NPCSettlement npcSettlement) {
            if (Int32.TryParse(migrationAdjustmentStr, out var adjustment)) {
                if (adjustment > 0) {
                    npcSettlement.migrationComponent.IncreaseVillageMigrationMeter(adjustment);
                } else {
                    npcSettlement.migrationComponent.ReduceVillageMigrationMeter(adjustment * -1);
                }
                AddSuccessMessage($"{settlement.name} migration meter is now {npcSettlement.migrationComponent.GetMigrationMeterValueInText()}");
            } else {
                AddErrorMessage($"{migrationAdjustmentStr} could not be parsed into an integer");    
            }
        } else {
            AddErrorMessage($"Could not find NPCSettlement with name {settlementName}");
        }
    }
    #endregion

    #region Scheme
    private void OnToggleAlwaysSuccessScheme(bool p_isOn) {
        SchemeData.alwaysSuccessScheme = p_isOn;
    }
    #endregion
    
    #region Hover Data
    private void OnToggleShowPOIHoverData(bool p_isOn) {
        showPOIHoverData = p_isOn;
    }
    #endregion
}
