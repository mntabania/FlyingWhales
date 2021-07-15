using System;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Locations.Settlements;
using Ruinarch;
using UtilityScripts;
using UnityEngine.Assertions;
using Interrupts;
using Object_Pools;
// ReSharper disable Unity.NoNullPropagation

public class Player : ILeader, IObjectManipulator {
    //public PlayerArchetype archetype { get; private set; }
    public Faction playerFaction { get; private set; }
    public PlayerSettlement playerSettlement { get; private set; }
    public int mana { get; private set; }

    public int spiritEnergy { get; private set; }
    public int experience { get; private set; }
    public List<IIntel> allIntel { get; private set; }
    public CombatAbility currentActiveCombatAbility { get; private set; }
    public IIntel currentActiveIntel { get; private set; }
    //public HexTile portalTile { get; private set; }
    public Area portalArea { get; private set; }
    public TILE_OBJECT_TYPE currentActiveItem { get; private set; }
    public bool isCurrentlyBuildingDemonicStructure { get; private set; }
    public IPlayerActionTarget currentlySelectedPlayerActionTarget { get; private set; }
    public List<string> charactersThatHaveReportedDemonicStructure { get; private set; }

    //Components
    public SeizeComponent seizeComponent { get; }
    public ThreatComponent threatComponent { get; }
    public PlayerSkillComponent playerSkillComponent { get; }
    public PlagueComponent plagueComponent { get; }
    public PlayerUnderlingsComponent underlingsComponent { get; private set; }
    public PlayerTileObjectComponent tileObjectComponent { get; private set; }
    public StoredTargetsComponent storedTargetsComponent { get; }
    public BookmarkComponent bookmarkComponent { get; }
    public SummonMeterComponent summonMeterComponent { get; private set; }
    public ManaRegenComponent manaRegenComponent { get; set; }
    public PlayerDamageAccumulator damageAccumulator { get; private set; }
    public PlayerRetaliationComponent retaliationComponent { get; private set; }

    public bool hasAlreadyWon { get; set; }

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
    public int chaoticEnergy => plagueComponent.plaguePoints;
    #endregion

    public Player() {
        allIntel = new List<IIntel>();
        charactersThatHaveReportedDemonicStructure = new List<string>();
        mana = EditableValuesManager.Instance.startingMana;
        currentActiveItem = TILE_OBJECT_TYPE.NONE;

        //Components
        seizeComponent = new SeizeComponent();
        threatComponent = new ThreatComponent(this);
        playerSkillComponent = new PlayerSkillComponent();
        plagueComponent = new PlagueComponent();
        underlingsComponent = new PlayerUnderlingsComponent();
        storedTargetsComponent = new StoredTargetsComponent();
        manaRegenComponent = new ManaRegenComponent(this);
        tileObjectComponent = new PlayerTileObjectComponent();
        summonMeterComponent = new SummonMeterComponent();
        bookmarkComponent = new BookmarkComponent();
        damageAccumulator = new PlayerDamageAccumulator();
        retaliationComponent = new PlayerRetaliationComponent();
        summonMeterComponent.Initialize();

        hasAlreadyWon = false;
        if (WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed()) {
            bookmarkComponent.AddBookmark(retaliationComponent.retaliationProgress, BOOKMARK_CATEGORY.Major_Events);    
        }
        // bookmarkComponent.AddBookmark(summonMeterComponent.progress, BOOKMARK_CATEGORY.Portal);

        SubscribeListeners();
        
    }
    public Player(SaveDataPlayerGame data) {
        allIntel = new List<IIntel>();
        seizeComponent = data.seizeComponent.Load();
        threatComponent = data.threatComponent.Load();
        playerSkillComponent = data.playerSkillComponent.Load();
        underlingsComponent = data.underlingsComponent.Load();
        tileObjectComponent = data.tileObjectComponent.Load();
        summonMeterComponent = data.summonMeterComponent.Load();
        damageAccumulator = data.damageAccumulator.Load();
        retaliationComponent = data.retaliationComponent.Load();
        bookmarkComponent = new BookmarkComponent();
        plagueComponent = new PlagueComponent(data.plagueComponent);
        threatComponent.SetPlayer(this);

        currentActiveItem = TILE_OBJECT_TYPE.NONE;
        storedTargetsComponent = new StoredTargetsComponent();
        manaRegenComponent = new ManaRegenComponent(this, data.manaRegenComponent);
        summonMeterComponent.Initialize();

        // bookmarkComponent.AddBookmark(summonMeterComponent.progress, BOOKMARK_CATEGORY.Portal);
        hasAlreadyWon = data.hasAlreadyWon;
        
        charactersThatHaveReportedDemonicStructure = new List<string>(data.charactersThatHaveReportedDemonicStructure);
    }

    public void LoadPlayerData(SaveDataPlayer save) {
        if(save != null) {
            experience = save.exp;
            playerSkillComponent.LoadPlayerSkillTreeOrLoadout(save);
            //playerSkillComponent.LoadSummons(save);
        }
    }

    public void SetPortalTile(Area tile) {
        portalArea = tile;
    }

    #region Listeners
    private void SubscribeListeners() {
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_ADDED_TO_FACTION, OnCharacterAddedToFaction);
        Messenger.AddListener<Character, Faction>(FactionSignals.CHARACTER_REMOVED_FROM_FACTION, OnCharacterRemovedFromFaction);
        Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        
        underlingsComponent.SubscribeListeners();
        bookmarkComponent.SubscribeListeners();
        storedTargetsComponent.SubscribeListeners();
    }
    #endregion

    #region Settlement
    //public PlayerSettlement CreatePlayerSettlement(BaseLandmark portal) {
    //    PlayerSettlement npcSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(portal.tileLocation);
    //    npcSettlement.SetName("Demonic Intrusion");
    //    SetPlayerArea(npcSettlement);
    //    // portal.tileLocation.InstantlyCorruptAllOwnedInnerMapTiles();
    //    return npcSettlement;
    //}
    public void LoadPlayerArea(SaveDataPlayerGame saveDataPlayerGame) {
        BaseSettlement settlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(saveDataPlayerGame.settlementID);
        PlayerSettlement pSettlement = settlement as PlayerSettlement;
        Assert.IsNotNull(pSettlement, $"Could not load player settlement because it is either null or not a PlayerSettlement type {settlement?.ToString() ?? "Null"}");
        SetPlayerArea(pSettlement);
    }
    public void SetPlayerArea(PlayerSettlement npcSettlement) {
        playerSettlement = npcSettlement;
    }
    #endregion

    #region Faction
    public void CreatePlayerFaction() {
        Faction faction = FactionManager.Instance.CreateNewFaction(FACTION_TYPE.Demons, "Demons");
        faction.SetLeader(this);
        SetPlayerFaction(faction);
        if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Pitto) {
            //https://trello.com/c/hZOagpaZ/3537-pitto-tweaks-v2
            FactionRelationship relationship = faction.GetRelationshipWith(FactionManager.Instance.neutralFaction);
            relationship.SetRelationshipStatus(FACTION_RELATIONSHIP_STATUS.Hostile);
        }
    }
    private void SetPlayerFaction(Faction faction) {
        playerFaction = faction;
    }
    #endregion

    #region Role Actions
    public SkillData currentActivePlayerSpell { get; private set; }
    public void SetCurrentlyActivePlayerSpell(SkillData action) {
        if(currentActivePlayerSpell != action) {
            SkillData previousActiveAction = currentActivePlayerSpell;
            currentActivePlayerSpell = action;
            if (currentActivePlayerSpell == null) {
                previousActiveAction.OnNoLongerCurrentActiveSpell();
                PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.spellInputModule);
                UIManager.Instance.SetTempDisableShowInfoUI(false); //allow UI clicks again after active spell has been set to null
                Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnSpellCast);
            	InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                previousActiveAction.UnhighlightAffectedTiles();
                UIManager.Instance.HideSmallInfo(); //This is to hide the invalid messages.
                Messenger.Broadcast(PlayerSkillSignals.PLAYER_NO_ACTIVE_SPELL, previousActiveAction);
            } else {
                action.OnSetAsCurrentActiveSpell();
                PlayerManager.Instance.AddPlayerInputModule(PlayerManager.spellInputModule);
            	InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
                Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnSpellCast);
                Messenger.Broadcast(PlayerSkillSignals.PLAYER_SET_ACTIVE_SPELL, currentActivePlayerSpell);
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
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(hoveredTile, out var cannotPerformReason)) {
                            currentActivePlayerSpell.ActivateAbility(hoveredTile);
                            activatedAction = true;
                        } else {
                            if (!string.IsNullOrEmpty(cannotPerformReason)) {
                                InnerMapManager.Instance.ShowAreaMapTextPopup(cannotPerformReason, hoveredTile.centeredWorldLocation, Color.white);
                            }
                        } 
                        UIManager.Instance.SetTempDisableShowInfoUI(true);
                    }
                    break;
                case SPELL_TARGET.AREA:
                    hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    if (hoveredTile != null) {
                        if (currentActivePlayerSpell.CanPerformAbilityTowards(hoveredTile.area)) {
                            currentActivePlayerSpell.ActivateAbility(hoveredTile.area);
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
    public bool IsCurrentActiveSpell(PLAYER_SKILL_TYPE p_skillType) {
        return currentActivePlayerSpell != null && currentActivePlayerSpell.type == p_skillType;
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
    public void SetCurrentActiveIntel(IIntel intel) {
        if (currentActiveIntel == intel) {
            //Do not process when setting the same intel
            return;
        }
        Debug.Log($"Will set current active intel to {intel?.log.logText ?? "null"}");
        IIntel previousIntel = currentActiveIntel;
        currentActiveIntel = intel;
        if(previousIntel != null) {
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(previousIntel);
            intelItem?.SetClickedState(false);
            Messenger.RemoveListener<KeyCode>(ControlsSignals.KEY_DOWN, OnIntelCast);
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        }
        if (currentActiveIntel != null) {
            PlayerManager.Instance.AddPlayerInputModule(PlayerManager.intelInputModule);
            Messenger.Broadcast(PlayerSignals.ACTIVE_INTEL_SET, currentActiveIntel);
            IntelItem intelItem = PlayerUI.Instance.GetIntelItemWithIntel(currentActiveIntel);
            intelItem?.SetClickedState(true);
            InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Cross);
            Messenger.AddListener<KeyCode>(ControlsSignals.KEY_DOWN, OnIntelCast);
        } else {
            PlayerManager.Instance.RemovePlayerInputModule(PlayerManager.intelInputModule);
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
        if (CanShareIntelTo(InnerMapManager.Instance.currentlyHoveredPoi, ref hoverText, currentActiveIntel)) {
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
    public bool CanShareIntelTo(IPointOfInterest poi, ref string hoverText, IIntel p_intel) {
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
            if (!p_intel.CanShareIntelTo(character)) {
                return false;
            }
            if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race)) { //character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER
                return true;
            }
        }
        return false;
    }
    public bool CanShareIntelTo(IPointOfInterest poi, IIntel p_intel) {
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
            if (!p_intel.CanShareIntelTo(character)) {
                return false;
            }
            if (!character.faction.isPlayerFaction && !GameUtilities.IsRaceBeast(character.race)) { //character.role.roleType != CHARACTER_ROLE.BEAST && character.role.roleType != CHARACTER_ROLE.PLAYER
                return true;
            }
        }
        return false;
    }
    public bool HasHostageIntel(Character p_hostage) {
        for (int i = 0; i < allIntel.Count; i++) {
            IIntel intel = allIntel[i];
            if (intel is ActionIntel actionIntel && actionIntel.reactable is ActualGoapNode action && action.actor == p_hostage && action.goapType == INTERACTION_TYPE.IS_IMPRISONED) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Player Notifications
    private bool ShouldShowNotificationFrom(LocationGridTile location) {
        return location != null && location.tileObjectComponent.isSeenByEyeWard;
    }
    private bool ShouldShowNotificationFrom(Character character) {
        return ShouldShowNotificationFrom(character.gridTileLocation);
    }
    private bool ShouldShowNotificationFrom(Character character, in Log log) {
        if (ShouldShowNotificationFrom(character)) {
            return true;
        } else {
            for (int i = 0; i < log.fillers.Count; i++) {
                object fillerObject = log.fillers[i].GetObjectForFiller();
                if (fillerObject is Character fillerCharacter) {
                    if (ShouldShowNotificationFrom(fillerCharacter)) {
                        return true;
                    }
                } else if (fillerObject is LocationGridTile fillerGridTile) {
                    if (ShouldShowNotificationFrom(fillerGridTile)) {
                        return true;
                    }
                }
            }
            //NOTE: Replaced with the loop above to avoid garbage
            //return ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Character).Select(x => x.GetObjectForFiller() as Character).ToArray())
            //    || ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Region).Select(x => x.GetObjectForFiller() as Region).ToArray());
        }
        return false;
    }
    private bool ShouldShowNotificationFrom(LocationGridTile location, in Log log) {
        if (ShouldShowNotificationFrom(location)) {
            return true;
        } else {
            for (int i = 0; i < log.fillers.Count; i++) {
                object fillerObject = log.fillers[i].GetObjectForFiller();
                if (fillerObject is Character fillerCharacter) {
                    if (ShouldShowNotificationFrom(fillerCharacter)) {
                        return true;
                    }
                } else if (fillerObject is LocationGridTile fillerGridTile) {
                    if (ShouldShowNotificationFrom(fillerGridTile)) {
                        return true;
                    }
                }
            }
            //NOTE: Replaced with the loop above to avoid garbage
            //return ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Character).Select(x => x.GetObjectForFiller() as Character).ToArray())
            //       || ShouldShowNotificationFrom(log.fillers.Where(x => x.GetObjectForFiller() is Region).Select(x => x.GetObjectForFiller() as Region).ToArray());
        }
        return false;
    }
    //private bool ShouldShowNotificationFrom(Character[] characters) {
    //    for (int i = 0; i < characters.Length; i++) {
    //        if (ShouldShowNotificationFrom(characters[i])) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    //private bool ShouldShowNotificationFrom(LocationGridTile[] locations) {
    //    for (int i = 0; i < locations.Length; i++) {
    //        if (ShouldShowNotificationFrom(locations[i])) {
    //            return true;
    //        }
    //    }
    //    return false;
    //}
    public bool ShowNotificationFrom(LocationGridTile location, Log log, bool releaseLogAfter = false) {
        if (ShouldShowNotificationFrom(location, log)) {
            ShowNotification(log, releaseLogAfter);
            return true;
        }
        return false;
    }
    public bool ShowNotificationFrom(Character character, Log log, bool releaseLogAfter = false) {
        if (ShouldShowNotificationFrom(character, log)) {
            ShowNotification(log, releaseLogAfter);
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
    public void ShowNotificationFromPlayer(Log log, bool releaseLogAfter = false) {
        //Removed adding to database here because this function should only be for showing notification, if we want to add it to database, it should be called outside this function
        //This is also redundant because all the outside calls of ShowNotificationFromPlayer already calls AddLogToDatabase
        //log.AddLogToDatabase();
        ShowNotification(log, releaseLogAfter);
    }
    
    private void ShowNotification(Log log, bool releaseLogAfter = false) {
        Messenger.Broadcast(UISignals.SHOW_PLAYER_NOTIFICATION, log);
        if (releaseLogAfter) {
            LogPool.Release(log);
        }
    }
    private void ShowNotification(IIntel intel) {
        Messenger.Broadcast(UISignals.SHOW_INTEL_NOTIFICATION, intel);
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
        if (hoveredTile != null && hoveredTile.tileObjectComponent.objHere == null) {
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
                    if(highlightedTiles[i].tileObjectComponent.objHere != null) {
                        poisInHighlightedTiles.Add(highlightedTiles[i].tileObjectComponent.objHere);
                    }
                }
                currentActiveCombatAbility.ActivateAbility(poisInHighlightedTiles);
            }
        }
        //Debug.Log(GameManager.Instance.TodayLogString() + summary);
    }
    #endregion

    #region spirit energy
    public void AdjustSpiritEnergy(int amount) {
        spiritEnergy += amount;
        spiritEnergy = Mathf.Clamp(spiritEnergy, 0, 100000);
        Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, amount, spiritEnergy);
        Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
    }
    #endregion
    
    #region Mana
    public void AdjustMana(int amount) {
        mana += amount;
        mana = Mathf.Clamp(mana, 0, EditableValuesManager.Instance.maximumMana);
        Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_MANA, amount, mana);
        Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
    }
    public void AdjustManaNoLimit(int amount) {
        mana += amount;
        mana = Mathf.Max(0, mana);
        Messenger.Broadcast(PlayerSignals.PLAYER_ADJUSTED_MANA, amount, mana);
        Messenger.Broadcast(PlayerSkillSignals.FORCE_RELOAD_PLAYER_ACTIONS);
    }
    public int GetManaCostForInterventionAbility(PLAYER_SKILL_TYPE ability) {
        int tier = PlayerManager.Instance.GetSpellTier(ability);
        return PlayerManager.Instance.GetManaCostForSpell(tier);
    }
    #endregion

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

    #region Characters
    private void OnCharacterAddedToFaction(Character character, Faction faction) {
        if (faction == playerFaction) {
            //if(character.minion != null) {
            //    AddMinion(character.minion);
            //} 
            //else if(character is Summon summon) {
            //    AddSummon(summon);
            //}
            string bredBehaviour;
            if (character is Summon summon) {
                //Note: only Disabled bred behaviour for now, will remove it completely when change has been confirmed
                bredBehaviour = string.Empty;
                // bredBehaviour = summon.bredBehaviour;
            } else {
                bredBehaviour = character.characterClass.traitNameOnTamedByPlayer;
            }
            if (!string.IsNullOrEmpty(bredBehaviour)) {
                character.traitContainer.AddTrait(character, bredBehaviour);
            }
            underlingsComponent.OnCharacterAddedToPlayerFaction(character);
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
                //Note: only Disabled bred behaviour for now, will remove it completely when change has been confirmed
                bredBehaviour = string.Empty;
                // bredBehaviour = summon.bredBehaviour;
            } else {
                bredBehaviour = character.characterClass.traitNameOnTamedByPlayer;
            }
            if (!string.IsNullOrEmpty(bredBehaviour)) {
                character.traitContainer.RemoveTrait(character, bredBehaviour);
            }
            underlingsComponent.OnCharacterRemovedFromPlayerFaction(character);
        }
    }
    private void OnCharacterDied(Character p_character) {
        if(p_character.faction == playerFaction) {
            underlingsComponent.OnFactionMemberDied(p_character);
        }
        retaliationComponent.OnCharacterDeath(p_character);
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
        if (hoveredTile != null && hoveredTile.tileObjectComponent.objHere == null) {
            //DatabaseManager.Instance.tileObjectDatabase.destroyedTileObjects.Clear();
            //TileObject ex = new FireCrystal();
            //DatabaseManager.Instance.tileObjectDatabase.UnRegisterTileObject(ex);
            TileObject item = InnerMapManager.Instance.CreateNewTileObject<TileObject>(currentActiveItem);
            hoveredTile.structure.AddPOI(item, hoveredTile);
            //hoveredTile.structure.RemovePOI(item);
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
    #endregion

    #region Loading
    public void LoadReferences(SaveDataPlayerGame data) {
        //for (int i = 0; i < data.minionIDs.Count; i++) {
        //    Character character = CharacterManager.Instance.GetCharacterByPersistentID(data.minionIDs[i]);
        //    minions.Add(character.minion);
        //}
        //for (int i = 0; i < data.summonIDs.Count; i++) {
        //    Summon summon = CharacterManager.Instance.GetCharacterByPersistentID(data.summonIDs[i]) as Summon;
        //    summons.Add(summon);
        //}

        AdjustMana(data.mana);
        AdjustSpiritEnergy(data.spiritEnergy);
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
        playerSkillComponent.LoadReferences(data.playerSkillComponent);
        storedTargetsComponent.LoadReferences(data.storedTargetsComponent);
        summonMeterComponent.LoadReferences(data.summonMeterComponent);
        underlingsComponent.LoadReferences(data.underlingsComponent);
        retaliationComponent.LoadReferences(data.retaliationComponent);
    }
    public void LoadReferencesMainThread(SaveDataPlayerGame data) {
        SubscribeListeners();
        if (WorldSettings.Instance.worldSettingsData.IsRetaliationAllowed()) {
            bookmarkComponent.AddBookmark(retaliationComponent.retaliationProgress, BOOKMARK_CATEGORY.Major_Events);
        }
        PlayerUI.Instance.UpdateUI();
    }
    #endregion

    #region Building
    public void SetIsCurrentlyBuildingDemonicStructure(bool state) {
    isCurrentlyBuildingDemonicStructure = state;
    }
    #endregion

    #region Currencies
    private void AdjustCurrency(CURRENCY p_currency, int p_amount, bool affectSpiritEnergy = true) {
        switch (p_currency) {
            case CURRENCY.Mana:
                AdjustMana(p_amount);
                break;
            case CURRENCY.Chaotic_Energy:
                plagueComponent.AdjustPlaguePoints(p_amount);
                break;
            case CURRENCY.Spirit_Energy:
                AdjustSpiritEnergy(p_amount);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(p_currency), p_currency, null);
        }
    }
    public void AddCurrency(Cost p_cost) {
        AdjustCurrency(p_cost.currency, p_cost.processedAmount);
    }
    public void ReduceCurrency(Cost p_cost) {
        AdjustCurrency(p_cost.currency, -p_cost.processedAmount);
    }
    public bool CanAfford(CURRENCY p_currency, int p_amount) {
        switch (p_currency) {
            case CURRENCY.Mana:
                return mana >= p_amount;
            case CURRENCY.Chaotic_Energy:
                return plagueComponent.plaguePoints >= p_amount;
            case CURRENCY.Spirit_Energy:
                return spiritEnergy >= p_amount;
            default:
                throw new ArgumentOutOfRangeException(nameof(p_currency), p_currency, null);
        }
    }
    public bool CanAfford(Cost p_cost) {
        return CanAfford(p_cost.currency, p_cost.processedAmount);
    }
    public bool CanAfford(Cost[] p_cost) {
        bool canAfford = true;
        for (int i = 0; i < p_cost.Length; i++) {
            Cost cost = p_cost[i];
            if (!CanAfford(cost)) {
                canAfford = false;
                break;
            }
        }
        return canAfford;
    }
    #endregion

    #region Reporting
    public void AddCharacterThatHasReported(Character p_character) {
        if (!charactersThatHaveReportedDemonicStructure.Contains(p_character.persistentID)) {
            charactersThatHaveReportedDemonicStructure.Add(p_character.persistentID);
        }
    }
    public void ClearCharactersThatHaveReported() {
        charactersThatHaveReportedDemonicStructure.Clear();
    }
    public bool HasAlreadyReportedADemonicStructure(Character p_character) {
        return charactersThatHaveReportedDemonicStructure.Contains(p_character.persistentID);
    }
    #endregion
}