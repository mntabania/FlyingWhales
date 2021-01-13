using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Inner_Maps;
using Traits;
using Archetype;
using Locations.Settlements;
using Ruinarch;
using UtilityScripts;
using Random = UnityEngine.Random;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;
using Interrupts;
using Logs;
// ReSharper disable Unity.NoNullPropagation

public class Player : ILeader, IObjectManipulator {
    //public PlayerArchetype archetype { get; private set; }
    public Faction playerFaction { get; private set; }
    public PlayerSettlement playerSettlement { get; private set; }
    public int mana { get; private set; }
    public int experience { get; private set; }
    public List<IIntel> allIntel { get; private set; }
    public List<Minion> minions { get; private set; }
    public List<Summon> summons { get; private set; }
    public CombatAbility currentActiveCombatAbility { get; private set; }
    public IIntel currentActiveIntel { get; private set; }
    public HexTile portalTile { get; private set; }
    public TILE_OBJECT_TYPE currentActiveItem { get; private set; }
    public bool isCurrentlyBuildingDemonicStructure { get; private set; }
    public IPlayerActionTarget currentlySelectedPlayerActionTarget { get; private set; }
    
    //Components
    public SeizeComponent seizeComponent { get; }
    public ThreatComponent threatComponent { get; }
    public PlayerSkillComponent playerSkillComponent { get; }
    public PlagueComponent plagueComponent { get; }

    #region getters/setters
    public int id => -645;
    public string name => "Player";
    public RACE race => RACE.HUMANS;
    public GENDER gender => GENDER.MALE;
    public Region currentRegion => null;
    public Region homeRegion => null;
    public string persistentID => string.Empty;
    public OBJECT_TYPE objectType => OBJECT_TYPE.Player;
    public System.Type serializedData => typeof(SaveDataPlayer);
    #endregion

    public Player() {
        allIntel = new List<IIntel>();
        minions = new List<Minion>();
        summons = new List<Summon>();
        mana = EditableValuesManager.Instance.startingMana;
        seizeComponent = new SeizeComponent();
        threatComponent = new ThreatComponent(this);
        playerSkillComponent = new PlayerSkillComponent(this);
        plagueComponent = new PlagueComponent();
        currentActiveItem = TILE_OBJECT_TYPE.NONE;
        AddListeners();
    }
    public Player(SaveDataPlayerGame data) {
        allIntel = new List<IIntel>();
        minions = new List<Minion>();
        summons = new List<Summon>();
        seizeComponent = data.seizeComponent.Load();
        threatComponent = data.threatComponent.Load();
        playerSkillComponent = data.playerSkillComponent.Load();
        plagueComponent = new PlagueComponent(data.plagueComponent);

        threatComponent.SetPlayer(this);
        playerSkillComponent.SetPlayer(this);

        currentActiveItem = TILE_OBJECT_TYPE.NONE;
        AddListeners();
    }

    public void LoadPlayerData(SaveDataPlayer save) {
        if(save != null) {
            experience = save.exp;
            playerSkillComponent.LoadPlayerSkillTreeOrLoadout(save);
            //playerSkillComponent.LoadSummons(save);
        }
    }

    public void SetPortalTile(HexTile tile) {
        portalTile = tile;
    }

    #region Listeners
    private void AddListeners() {
        AddWinListener();
        //goap
        // Messenger.AddListener<string, ActualGoapNode>(Signals.AFTER_ACTION_STATE_SET, OnAfterActionStateSet);
        // Messenger.AddListener<Character, ActualGoapNode>(Signals.CHARACTER_DOING_ACTION, OnCharacterDoingAction);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_OPENED, OnInnerMapOpened);
        Messenger.AddListener<Region>(RegionSignals.REGION_MAP_CLOSED, OnInnerMapClosed);

        //minions
        Messenger.AddListener<Minion, BaseLandmark>(PlayerSignals.MINION_ASSIGNED_PLAYER_LANDMARK, OnMinionAssignedToPlayerLandmark);
        Messenger.AddListener<Minion, BaseLandmark>(PlayerSignals.MINION_UNASSIGNED_PLAYER_LANDMARK, OnMinionUnassignedFromPlayerLandmark);
        Messenger.AddListener<Minion>(SpellSignals.SUMMON_MINION, OnSummonMinion);
        Messenger.AddListener<Minion>(SpellSignals.UNSUMMON_MINION, OnUnsummonMinion);

        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
    }
    #endregion

    #region ILeader
    //public void LevelUp() { }
    #endregion

    #region Settlement
    public PlayerSettlement CreatePlayerSettlement(BaseLandmark portal) {
        PlayerSettlement npcSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(portal.tileLocation);
        npcSettlement.SetName("Demonic Intrusion");
        SetPlayerArea(npcSettlement);
        // portal.tileLocation.InstantlyCorruptAllOwnedInnerMapTiles();
        return npcSettlement;
    }
    public void LoadPlayerArea(SaveDataPlayerGame saveDataPlayerGame) {
        BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataPlayerGame.settlementID);
        PlayerSettlement pSettlement = settlement as PlayerSettlement;
        Assert.IsNotNull(pSettlement, $"Could not load player settlement because it is either null or not a PlayerSettlement type {settlement?.ToString() ?? "Null"}");
        SetPlayerArea(pSettlement);
    }
    public void SetPlayerArea(PlayerSettlement npcSettlement) {
        playerSettlement = npcSettlement;
    }
    private void OnInnerMapOpened(Region area) {
        //for (int i = 0; i < minions.Count; i++) {
        //    minions[i].ResetCombatAbilityCD();
        //}
        //ResetInterventionAbilitiesCD();
        //currentTargetFaction = npcSettlement.owner;
    }
    private void OnInnerMapClosed(Region area) {
        //currentTargetFaction = null;
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction faction = FactionManager.Instance.CreateNewFaction(FACTION_TYPE.Demons, "Demons");
        faction.SetLeader(this);
        SetPlayerFaction(faction);
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    //public void SetPlayerTargetFaction(Faction faction) {
    //    currentTargetFaction = faction;
    //}
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if(faction == playerFaction) {
            //if(character.minion != null) {
            //    AddMinion(character.minion);
            //} 
            //else if(character is Summon summon) {
            //    AddSummon(summon);
            //}
            string bredBehaviour;
            if (character is Summon summon) {
                bredBehaviour = summon.bredBehaviour;
            } else {
                bredBehaviour = character.characterClass.traitNameOnTamedByPlayer;
            }
            if (!string.IsNullOrEmpty(bredBehaviour)) {
                character.traitContainer.AddTrait(character, bredBehaviour);
            }
        }
    }
    private void OnCharacterRemovedFromFaction(Character character, Faction faction) {
        if (faction == playerFaction) {
            //if (character.minion != null) {
            //    RemoveMinion(character.minion);
            //} 
            //else if (character is Summon summon) {
            //    RemoveSummon(summon);
            //}
            string bredBehaviour;
            if (character is Summon summon) {
                bredBehaviour = summon.bredBehaviour;
            } else {
                bredBehaviour = character.characterClass.traitNameOnTamedByPlayer;
            }
            if (!string.IsNullOrEmpty(bredBehaviour)) {
                character.traitContainer.RemoveTrait(character, bredBehaviour);
            }
        }
    }
    #endregion

    #region Minions
    public void AddMinion(Minion minion) {
        if (!minions.Contains(minion)) {
            minions.Add(minion);
            Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_MINION, minion);
        }
    }
    public void RemoveMinion(Minion minion) {
        if (minions.Remove(minion)) {
            Messenger.Broadcast(PlayerSignals.PLAYER_LOST_MINION, minion);
        }
    }
    private void OnSummonMinion(Minion minion) {
        AddMinion(minion);
    }
    private void OnUnsummonMinion(Minion minion) {
        RemoveMinion(minion);
    }
    //public void SetMinionLeader(Minion minion) {
    //    currentMinionLeader = minion;
    //}
    //private void ReplaceMinion(object objToReplace, object objToAdd) {
    //    Minion minionToBeReplaced = objToReplace as Minion;
    //    Minion minionToBeAdded = objToAdd as Minion;

    //    for (int i = 0; i < minions.Count; i++) {
    //        if(minions[i] == minionToBeReplaced) {
    //            minionToBeAdded.SetIndexDefaultSort(i);
    //            minions[i] = minionToBeAdded;
    //            if(currentMinionLeader == minionToBeReplaced) {
    //                SetMinionLeader(minionToBeAdded);
    //            }
    //            break;
    //        }
    //    }
    //}
    private void RejectMinion(object obj) { }
    #endregion

    #region Win/Lose Conditions
    private void AddWinListener() {
        Messenger.AddListener<Faction>(FactionSignals.FACTION_LEADER_DIED, OnFactionLeaderDied);
    }
    private void OnFactionLeaderDied(Faction faction) {
        List<Faction> allUndestroyedFactions = FactionManager.Instance.allFactions.Where(
            x => x.factionType.type != FACTION_TYPE.Wild_Monsters
            && !x.isPlayerFaction
            && x.isActive && !x.isDisbanded).ToList();
        if (allUndestroyedFactions.Count == 0) {
            Debug.LogError("All factions are destroyed! Player won!");
        }        
    }
    #endregion

    #region Role Actions
    public SkillData currentActivePlayerSpell { get; private set; }
    public void SetCurrentlyActivePlayerSpell(SkillData action) {
        if(currentActivePlayerSpell != action) {
            SkillData previousActiveAction = currentActivePlayerSpell;
            currentActivePlayerSpell = action;
            if (currentActivePlayerSpell == null) {
                UIManager.Instance.SetTempDisableShowInfoUI(false); //allow UI clicks again after active spell has been set to null
                Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnSpellCast);
            	InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                previousActiveAction.UnhighlightAffectedTiles();
                UIManager.Instance.HideSmallInfo(); //This is to hide the invalid messages.
                Messenger.Broadcast(SpellSignals.PLAYER_NO_ACTIVE_SPELL, previousActiveAction);
            } else {
            	InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
                Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnSpellCast);
                Messenger.Broadcast(SpellSignals.PLAYER_SET_ACTIVE_SPELL, currentActivePlayerSpell);
            }
        }
    }
    private void OnSpellCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TryExecuteCurrentActiveSpell();
        }
    }

    private void TryExecuteCurrentActiveSpell() {
        if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
            return; //clicked on UI;
        }
        bool activatedAction = false;
        for (int i = 0; i < currentActivePlayerSpell.targetTypes.Length; i++) {
            LocationGridTile hoveredTile = null;
            switch (currentActivePlayerSpell.targetTypes[i]) {
                case SPELL_TARGET.NONE:
                    break;
                case SPELL_TARGET.CHARACTER:
                    if (InnerMapManager.Instance.currentlyShowingMap != null && InnerMapManager.Instance.currentlyHoveredPoi is Character) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(InnerMapManager.Instance.currentlyHoveredPoi)) {
                            currentActivePlayerSpell.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
                            activatedAction = true;
                        } else {
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.TILE_OBJECT:
                    if (InnerMapManager.Instance.currentlyHoveredPoi is TileObject) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(InnerMapManager.Instance.currentlyHoveredPoi)) {
                            currentActivePlayerSpell.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
                            activatedAction = true;
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.TILE:
                    hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(hoveredTile)) {
                            currentActivePlayerSpell.ActivateAbility(hoveredTile);
                            activatedAction = true;
                        } 
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.HEX:
                    hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null && hoveredTile.collectionOwner.isPartOfParentRegionMap && hoveredTile.collectionOwner.partOfHextile.hexTileOwner) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(hoveredTile.collectionOwner.partOfHextile.hexTileOwner)) {
                            currentActivePlayerSpell.ActivateAbility(hoveredTile.collectionOwner.partOfHextile.hexTileOwner);
                            activatedAction = true;
                        } 
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.SETTLEMENT:
                    hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    BaseSettlement settlement = null;
                    if (hoveredTile != null && hoveredTile.IsPartOfSettlement(out settlement)) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(settlement)) {
                            currentActivePlayerSpell.ActivateAbility(settlement);
                            activatedAction = true;
                        }
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                default:
                    break;
            }
            if (activatedAction) {
                break;
            }
        }
        
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        if (currentActivePlayerSpell.CanPerformAbility() == false || 
            (currentActivePlayerSpell is DemonicStructurePlayerSkill && activatedAction)) {
            //if player can no longer cast spell after casting it, set active spell to null.
            SetCurrentlyActivePlayerSpell(null);
        }
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region Intel
    public void AddIntel(IIntel newIntel) {
        if (!allIntel.Contains(newIntel)) {
            allIntel.Add(newIntel);
            if (allIntel.Count > PlayerDB.MAX_INTEL) {
                RemoveIntel(allIntel[0]);
            }
            Messenger.Broadcast(PlayerSignals.PLAYER_OBTAINED_INTEL, newIntel);
        }
    }
    public void RemoveIntel(IIntel intel) {
        if (allIntel.Remove(intel)) {
            Messenger.Broadcast(PlayerSignals.PLAYER_REMOVED_INTEL, intel);
            intel.OnIntelRemoved();
        }
    }
    public void LoadIntels(SaveDataPlayer data) {
        //for (int i = 0; i < data.allIntel.Count; i++) {
        //    AddIntel(data.allIntel[i].Load());
        //}
    }
    public void SetCurrentActiveIntel(IIntel intel) {
        if (currentActiveIntel == intel) {
            //Do not process when setting the same intel
            return;
        }
        IIntel previousIntel = currentActiveIntel;
        currentActiveIntel = intel;
        if(previousIntel != null) {
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(previousIntel);
            intelItem?.SetClickedState(false);
            Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnIntelCast);
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        }
        if (currentActiveIntel != null) {
            Messenger.Broadcast(PlayerSignals.ACTIVE_INTEL_SET, currentActiveIntel);
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(currentActiveIntel);
            intelItem?.SetClickedState(true);
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
            Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnIntelCast);
        } else {
            Messenger.Broadcast(PlayerSignals.ACTIVE_INTEL_REMOVED);
        }
    }
    private void OnIntelCast(KeyCode keyCode) {
        if (keyCode == KeyCode.Mouse0) {
            TryExecuteCurrentActiveIntel();
        }
    }
    private void TryExecuteCurrentActiveIntel() {
        string hoverText = string.Empty;
        if (CanShareIntel(InnerMapManager.Instance.currentlyHoveredPoi, ref hoverText)) {
            Character targetCharacter = InnerMapManager.Instance.currentlyHoveredPoi as Character;
            List<ConversationData> conversationList = ObjectPoolManager.Instance.CreateNewConversationDataList();
            ConversationData targetOpeningLine = ObjectPoolManager.Instance.CreateNewConversationData("What do you want from me?", targetCharacter, DialogItem.Position.Left);
            ConversationData demonOpeningLine = ObjectPoolManager.Instance.CreateNewConversationData(currentActiveIntel.log.logText, null, DialogItem.Position.Right);

            string targetCharacterEmotions = targetCharacter.reactionComponent.ReactToIntel(currentActiveIntel);
            string emotionText = UtilityScripts.Utilities.FormulateTextFromEmotions(targetCharacterEmotions, currentActiveIntel.actor, currentActiveIntel.target, targetCharacter);

            ConversationData targetEmotionResponse = ObjectPoolManager.Instance.CreateNewConversationData(emotionText, targetCharacter, DialogItem.Position.Left);

            conversationList.Add(targetOpeningLine);
            conversationList.Add(demonOpeningLine);
            conversationList.Add(targetEmotionResponse);

            UIManager.Instance.OpenConversationMenu(conversationList, $"Share Intel with {targetCharacter.name}");
            Messenger.Broadcast(UISignals.ON_SHARE_INTEL);
            RemoveIntel(currentActiveIntel);
            SetCurrentActiveIntel(null);

            ObjectPoolManager.Instance.ReturnConversationDataToPool(targetOpeningLine);
            ObjectPoolManager.Instance.ReturnConversationDataToPool(demonOpeningLine);
            ObjectPoolManager.Instance.ReturnConversationDataToPool(targetEmotionResponse);
            ObjectPoolManager.Instance.ReturnConversationDataListToPool(conversationList);
        }
    }
    public bool CanShareIntel(IPointOfInterest poi, ref string hoverText) {
        if(poi is Character character) {
            if (!character.isNormalCharacter) {
                return false;
            }
            hoverText = string.Empty;
            if(character.traitContainer.HasTrait("Catatonic")) {
                hoverText = "Catatonic characters cannot be targeted.";
                return false;
            }
            if(character.traitContainer.HasTrait("Resting")) { 
                hoverText = "Sleeping characters cannot be targeted.";
                return false;
            }
            if (!character.limiterComponent.canWitness) {
                return false;
            }
            if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race)) { //character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER
                return true;
            }
        }
        return false;
    }
    public bool CanShareIntel(IPointOfInterest poi) {
        if(poi is Character character) {
            if (!character.isNormalCharacter) {
                return false;
            }
            if(character.traitContainer.HasTrait("Catatonic")) {
                return false;
            }
            if(character.traitContainer.HasTrait("Resting")) { 
                return false;
            }
            if (!character.limiterComponent.canWitness) {
                return false;
            }
            if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race)) { //character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Player Notifications
    private bool ShouldShowNotificationFrom(Region location) {
        return location.canShowNotifications;
    }
    private bool ShouldShowNotificationFrom(Character character) {
        return character.currentRegion != null && ShouldShowNotificationFrom(character.currentRegion);
    }
    private bool ShouldShowNotificationFrom(Character character, in Log log) {
        if (ShouldShowNotificationFrom(character)) {
            return true;
        } else {
            return ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Character).Select(x => x.GetObjectForFiller() as Character).ToArray())
                || ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Region).Select(x => x.GetObjectForFiller() as Region).ToArray());
        }
    }
    private bool ShouldShowNotificationFrom(Region location, in Log log) {
        if (ShouldShowNotificationFrom(location)) {
            return true;
        } else {
            return ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Character).Select(x => x.GetObjectForFiller() as Character).ToArray())
                   || ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Region).Select(x => x.GetObjectForFiller() as Region).ToArray());
        }
    }
    private bool ShouldShowNotificationFrom(Character[] characters) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i])) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Region[] locations) {
        for (int i = 0; i < locations.Length; i++) {
            if (ShouldShowNotificationFrom(locations[i])) {
                return true;
            }
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(Character[] characters, in Log log) {
        for (int i = 0; i < characters.Length; i++) {
            if (ShouldShowNotificationFrom(characters[i], log)) {
                return true;
            }
        }
        return false;
    }
    
    public bool ShowNotificationFrom(Region location, Log log) {
        if (ShouldShowNotificationFrom(location, log)) {
            ShowNotification(log);
            return true;
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, Log log) {
        if (ShouldShowNotificationFrom(character, log)) {
            ShowNotification(log);
            return true;
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, IIntel intel) {
        if (ShouldShowNotificationFrom(character)) {
            ShowNotification(intel);
            return true;
        }
        return false;
    }
    public void ShowNotificationFrom(Log log, params Character[] characters) {
        if (ShouldShowNotificationFrom(characters, log)) {
            ShowNotification(log);
        }
    }
    public void ShowNotificationFromPlayer(Log log) {
        //Removed adding to database here because this function should only be for showing notification, if we want to add it to database, it should be called outside this function
        //This is also redundant because all the outside calls of ShowNotificationFromPlayer already calls AddLogToDatabase
        //log.AddLogToDatabase();
        ShowNotification(log);
    }
    
    private void ShowNotification(Log log) {
        Messenger.Broadcast(UISignals.SHOW_PLAYER_NOTIFICATION, log);
    }
    private void ShowNotification(IIntel intel) {
        Messenger.Broadcast(UISignals.SHOW_INTEL_NOTIFICATION, intel);
    }
    #endregion

    #region Summons
    private void AddSummon(Summon summon) {
        if (!summons.Contains(summon)) {
            summons.Add(summon);
            Messenger.Broadcast(PlayerSignals.PLAYER_GAINED_SUMMON, summon);
        }
    }
    private void RemoveSummon(Summon summon) {
        if (summons.Remove(summon)) {
            Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
        }
    }
    private void OnCharacterDied(Character character) {
        if (character.faction == playerFaction && character is Summon summon) {
            // RemoveSummon(summon);
            Messenger.Broadcast(PlayerSignals.PLAYER_LOST_SUMMON, summon);
        }
    }
    #endregion

    #region Artifacts
    public ARTIFACT_TYPE currentActiveArtifact { get; private set; }
    public void SetCurrentlyActiveArtifact(ARTIFACT_TYPE artifact) {
        if (currentActiveArtifact != artifact) {
            ARTIFACT_TYPE previousActiveArtifact = currentActiveArtifact;
            currentActiveArtifact = artifact;
            if (currentActiveArtifact == ARTIFACT_TYPE.None) {
                Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnArtifactCast);
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                Messenger.Broadcast(PlayerSignals.PLAYER_NO_ACTIVE_ARTIFACT, previousActiveArtifact);
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Check);
                Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnArtifactCast);
            }
        }
    }
    private void OnArtifactCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TrySpawnArtifact();
        }
    }
    private void TrySpawnArtifact() {
        if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
            return; //clicked on UI;
        }
        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if (hoveredTile != null && hoveredTile.objHere == null) {
            Artifact artifact = InnerMapManager.Instance.CreateNewArtifact(currentActiveArtifact);
            hoveredTile.structure.AddPOI(artifact, hoveredTile);
        }
    }
    //public bool HasArtifact(string artifactName) {
    //    for (int i = 0; i < artifacts.Count; i++) {
    //        Artifact currArtifact = artifacts[i];
    //        if (currArtifact.name == artifactName) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //private Artifact GetArtifactOfType(ARTIFACT_TYPE type) {
    //    for (int i = 0; i < artifacts.Count; i++) {
    //        Artifact currArtifact = artifacts[i];
    //        if (currArtifact.type == type) {
    //            return currArtifact;
    //        }
    //    }
    //    return null;
    //}
    #endregion

    #region Combat Ability
    public void SetCurrentActiveCombatAbility(CombatAbility ability) {
        if(currentActiveCombatAbility == ability) {
            //Do not process when setting the same combat ability
            return;
        }
        CombatAbility previousAbility = currentActiveCombatAbility;
        currentActiveCombatAbility = ability;
        if (currentActiveCombatAbility == null) {
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
            // InputManager.Instance.ClearLeftClickActions();
            //GameManager.Instance.SetPausedState(false);
        } else {
            //change the cursor
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
            // InputManager.Instance.AddLeftClickAction(TryExecuteCurrentActiveCombatAbility);
            // InputManager.Instance.AddLeftClickAction(() => SetCurrentActiveCombatAbility(null));
            //GameManager.Instance.SetPausedState(true);
        }
    }
    private void TryExecuteCurrentActiveCombatAbility() {
        //string summary = "Mouse was clicked. Will try to execute " + currentActiveCombatAbility.name;
        if (currentActiveCombatAbility.abilityRadius == 0) {
            if (currentActiveCombatAbility.CanTarget(InnerMapManager.Instance.currentlyHoveredPoi)) {
                currentActiveCombatAbility.ActivateAbility(InnerMapManager.Instance.currentlyHoveredPoi);
            }
        } else {
            List<LocationGridTile> highlightedTiles = InnerMapManager.Instance.currentlyHighlightedTiles;
            if (highlightedTiles != null) {
                List<IPointOfInterest> poisInHighlightedTiles = new List<IPointOfInterest>();
                for (int i = 0; i < InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation.Count; i++) {
                    Character currCharacter = InnerMapManager.Instance.currentlyShowingLocation.charactersAtLocation[i];
                    if (highlightedTiles.Contains(currCharacter.gridTileLocation)) {
                        poisInHighlightedTiles.Add(currCharacter);
                    }
                }
                for (int i = 0; i < highlightedTiles.Count; i++) {
                    if(highlightedTiles[i].objHere != null) {
                        poisInHighlightedTiles.Add(highlightedTiles[i].objHere);
                    }
                }
                currentActiveCombatAbility.ActivateAbility(poisInHighlightedTiles);
            }
        }
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region The Eye
    private void OnMinionAssignedToPlayerLandmark(Minion minion, BaseLandmark landmark) { }
    private void OnMinionUnassignedFromPlayerLandmark(Minion minion, BaseLandmark landmark) { }
    #endregion

    #region Mana
    public void AdjustMana(int amount) {
        mana += amount;
        mana = Mathf.Clamp(mana, 0, EditableValuesManager.Instance.maximumMana);
        Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_MANA, amount, mana);
        Messenger.Broadcast(SpellSignals.FORCE_RELOAD_PLAYER_ACTIONS);
    }
    public int GetManaCostForInterventionAbility(PLAYER_SKILL_TYPE ability) {
        int tier = PlayerManager.Instance.GetSpellTier(ability);
        return PlayerManager.Instance.GetManaCostForSpell(tier);
    }
    #endregion

    //#region Archetype
    //public void SetArchetype(PLAYER_ARCHETYPE type) {
    //    if(archetype == null || archetype.type != type) {
    //        archetype = PlayerManager.CreateNewArchetype(type);
    //        for (int i = 0; i < archetype.spells.Count; i++) {
    //            unlearnedSpells.Remove(archetype.spells[i]);
    //        }
    //        for (int i = 0; i < archetype.afflictions.Count; i++) {
    //            unlearnedAfflictions.Remove(archetype.afflictions[i]);
    //        }
    //    }
    //}
    //public void LearnSpell(SPELL_TYPE type) {
    //    archetype.AddSpell(type);
    //    unlearnedSpells.Remove(type);
    //}
    //public void LearnAffliction(SPELL_TYPE affliction) {
    //    archetype.AddAffliction(affliction);
    //    unlearnedAfflictions.Remove(affliction);
    //}
    //#endregion

    #region Utilities
    /// <summary>
    /// Is the player currently doing something?
    /// (Casting a spell, seizing an object, etc.)
    /// </summary>
    /// <returns>True or false.</returns>
    public bool IsPerformingPlayerAction() {
        return PlayerManager.Instance.player.currentActivePlayerSpell != null
               || PlayerManager.Instance.player.seizeComponent.hasSeizedPOI
               || PlayerManager.Instance.player.currentActiveIntel != null
               || PlayerManager.Instance.player.currentActiveItem != TILE_OBJECT_TYPE.NONE
               || PlayerManager.Instance.player.currentActiveArtifact != ARTIFACT_TYPE.None;
    }
    public void SetCurrentPlayerActionTarget(IPlayerActionTarget p_target) {
        currentlySelectedPlayerActionTarget = p_target;
    }
    #endregion

    #region Tile Objects
    public void SetCurrentlyActiveItem(TILE_OBJECT_TYPE item) {
        if (currentActiveItem != item) {
            TILE_OBJECT_TYPE previousActiveItem = currentActiveItem;
            currentActiveItem = item;
            if (currentActiveItem == TILE_OBJECT_TYPE.NONE) {
                Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnItemCast);
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                Messenger.Broadcast(PlayerSignals.PLAYER_NO_ACTIVE_ITEM, previousActiveItem);
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Check);
                Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnItemCast);
            }
        }
    }
    private void OnItemCast(KeyCode key) {
        if (key == KeyCode.Mouse0) {
            TrySpawnItem();
        }
    }
    private void TrySpawnItem() {
        if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
            return; //clicked on UI;
        }
        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
        if (hoveredTile != null && hoveredTile.objHere == null) {
            TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(currentActiveItem);
            hoveredTile.structure.AddPOI(item, hoveredTile);
        }
    }
    #endregion

    #region Experience
    public void AdjustExperience(int amount) {
        experience += amount;
        if(experience < 0) {
            experience = 0;
        }
        SaveExp();
    }
    public void SetExperience(int amount) {
        experience = amount;
        if (experience < 0) {
            experience = 0;
        }
        SaveExp();
    }
    #endregion

    #region Saving
    private void SaveExp() {
        SaveManager.Instance.currentSaveDataPlayer.SetExp(experience);
    }
    //public void SaveSummons() {
    //    List<LocationStructure> kennels = playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.KENNEL);
    //    List<Summon> kennelSummons = null;
    //    if(kennels != null) {
    //        for (int i = 0; i < kennels.Count; i++) {
    //            LocationStructure currKennel = kennels[i];
    //            for (int j = 0; j < currKennel.charactersHere.Count; j++) {
    //                Character character = currKennel.charactersHere[j];
    //                if(character is Summon summon) {
    //                    if(summon.gridTileLocation != null && summon.marker) {
    //                        if(kennelSummons == null) {
    //                            kennelSummons = new List<Summon>();
    //                        }
    //                        kennelSummons.Add(summon);
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    if(kennelSummons != null) {
    //        SaveManager.Instance.currentSaveDataPlayer.SaveSummons(kennelSummons);
    //    }
    //}
    // public void SaveTileObjects() {
    //     List<LocationStructure> crypts = playerSettlement.GetStructuresOfType(STRUCTURE_TYPE.CRYPT);
    //     List<TileObject> cryptTileObjects = null;
    //     if (crypts != null) {
    //         for (int i = 0; i < crypts.Count; i++) {
    //             LocationStructure currCrypt = crypts[i];
    //             foreach (TileObjectsAndCount tileObjectsAndCount in currCrypt.groupedTileObjects.Values) {
    //                 if(tileObjectsAndCount.tileObjects != null && tileObjectsAndCount.tileObjects.Count > 0) {
    //                     for (int j = 0; j < tileObjectsAndCount.tileObjects.Count; j++) {
    //                         TileObject tileObject = tileObjectsAndCount.tileObjects[j];
    //                         if(tileObject.gridTileLocation != null 
    //                             && tileObject.tileObjectType != TILE_OBJECT_TYPE.BLOCK_WALL
    //                             && tileObject.preplacedLocationStructure != currCrypt
    //                             && !tileObject.isSaved) {
    //                             if (cryptTileObjects == null) {
    //                                 cryptTileObjects = new List<TileObject>();
    //                             }
    //                             cryptTileObjects.Add(tileObject);
    //                         }
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     if (cryptTileObjects != null) {
    //         SaveManager.Instance.currentSaveDataPlayer.SaveTileObjects(cryptTileObjects);
    //     }
    // }
    #endregion

    #region Loading
    public void LoadReferences(SaveDataPlayerGame data) {
        for (int i = 0; i < data.minionIDs.Count; i++) {
            Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.minionIDs[i]);
            minions.Add(character.minion);
        }
        for (int i = 0; i < data.summonIDs.Count; i++) {
            Summon summon = CharacterManager.Instance.GetCharacterByPersistentID(data.summonIDs[i]) as Summon;
            summons.Add(summon);
        }

        AdjustMana(data.mana);
        SetPortalTile(GridMap.Instance.map[data.portalTileXCoordinate, data.portalTileYCoordinate]);

        Faction faction = FactionManager.Instance.GetFactionByPersistentID(data.factionID);
        SetPlayerFaction(faction);

        playerSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(data.settlementID) as PlayerSettlement;

        for (int i = 0; i < data.actionIntels.Count; i++) {
            SaveDataActionIntel savedIntel = data.actionIntels[i];
            ActualGoapNode node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(savedIntel.node);
            ActionIntel actionIntel = new ActionIntel(node);
            allIntel.Add(actionIntel);
        }
        for (int i = 0; i < data.interruptIntels.Count; i++) {
            SaveDataInterruptIntel savedIntel = data.interruptIntels[i];
            InterruptHolder interrupt = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(savedIntel.interruptHolder);
            InterruptIntel interruptIntel = new InterruptIntel(interrupt);
            allIntel.Add(interruptIntel);
        }
        for (int i = 0; i < data.allNotifs.Count; i++) {
            data.allNotifs[i].Load();
        }
        for (int i = 0; i < data.allChaosOrbs.Count; i++) {
            data.allChaosOrbs[i].Load();
        }
        PlayerUI.Instance.UpdateUI();
    }
    #endregion

    #region Building
    public void SetIsCurrentlyBuildingDemonicStructure(bool state) {
        isCurrentlyBuildingDemonicStructure = state;
    }
    #endregion
}

[System.Serializable]
public struct DemonicLandmarkBuildingData {
    public LANDMARK_TYPE landmarkType;
    public string landmarkName;
    public int buildDuration;
    public int currentDuration;
}

[System.Serializable]
public struct DemonicLandmarkInvasionData {
    public bool beingInvaded;
    public int currentDuration;
}