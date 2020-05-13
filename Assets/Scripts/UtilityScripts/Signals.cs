using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using Inner_Maps;
using Traits;

public static class Signals {

    public static string TICK_STARTED = "OnTickStart";
    public static string TICK_ENDED = "OnTickEnd";
    public static string HOUR_STARTED = "OnHourStart";
    public static string DAY_STARTED = "OnDayStart";
    public static string MONTH_START = "OnMonthStart";
    public static string GAME_LOADED = "OnGameLoaded";
    public static string INSPECT_ALL = "InspectAll";
    /// <summary>
    /// Parameters: KeyCode (Pressed Key)
    /// </summary>
    public static string KEY_DOWN = "OnKeyDown";
    /// <summary>
    /// Parameters: GameObject (Destroyed Object)
    /// </summary>
    public static string POOLED_OBJECT_DESTROYED = "OnPooledObjectDestroyed";
    /// <summary>
    /// Parameters: BurningSource
    /// </summary>
    public static string BURNING_SOURCE_INACTIVE = "OnBurningSourceInactive";

    #region Tiles
    public static string TILE_LEFT_CLICKED = "OnTileLeftClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_RIGHT_CLICKED = "OnTileRightClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_HOVERED_OVER = "OnTileHoveredOver"; //Parameters (HexTile hoveredTile)
    public static string TILE_HOVERED_OUT = "OnTileHoveredOut"; //Parameters (HexTile hoveredTile)
    public static string ACTION_PERFORMED_ON_TILE_TRAITABLES = "OnActionPerformedOnTileTraitables";
    public static string TILE_DOUBLE_CLICKED = "OnTileDoubleClicked"; //Parameters (HexTile clickedTile)
    #endregion

    #region Areas/Regions
    public static string AREA_CREATED = "OnAreaCreated"; //Parameters (NPCSettlement newNpcSettlement)
    public static string LOCATION_MAP_OPENED = "OnAreaMapOpened"; //parameters (NPCSettlement npcSettlement)
    public static string LOCATION_MAP_CLOSED = "OnAreaMapClosed"; //parameters (NPCSettlement npcSettlement)
    public static string REGION_CHANGE_STORAGE = "OnRegionChangeStorage";
    /// <summary>
    /// Parameters (NPCSettlement)
    /// </summary>
    public static string SETTLEMENT_CHANGE_STORAGE = "OnSettlementChangeStorage";
    /// <summary>
    /// Parameters: TIME_IN_WORDS (current time of day)
    /// </summary>
    public static string UPDATE_INNER_MAP_LIGHT = "UpdateInnerMapLight";
    #endregion

    #region Landmarks
    public static string CHARACTER_ENTERED_REGION = "OnCharacterEnteredRegion"; //Parameters (Character, Region)
    public static string CHARACTER_EXITED_REGION = "OnCharacterExitedRegion"; //Parameters (Characte, Region)
    /// <summary>
    /// Parameters: BaseLandmark
    /// </summary>
    public static string LANDMARK_CREATED = "OnLandmarkCreated";
    /// <summary>
    /// Parameters: BaseLandmark, HexTile
    /// </summary>
    public static string LANDMARK_DESTROYED = "OnLandmarkDestroyed";
    #endregion

    #region Character
    public static string CHARACTER_DEATH = "OnCharacterDied"; //Parameters (Character characterThatDied)
    public static string CHARACTER_CREATED = "OnCharacterCreated"; //Parameters (Character createdCharacter)
    public static string ROLE_CHANGED = "OnCharacterRoleChanged"; //Parameters (Character characterThatChangedRole)
    public static string CHARACTER_REMOVED = "OnCharacterRemoved"; //Parameters (Character removedCharacter)
    public static string CHARACTER_OBTAINED_ITEM = "OnCharacterObtainItem"; //Parameters (SpecialToken obtainedItem, Character characterThatObtainedItem)
    public static string CHARACTER_LOST_ITEM = "OnCharacterLostItem"; //Parameters (SpecialToken unobtainedItem, Character character)
    public static string CHARACTER_TRAIT_ADDED = "OnCharacterTraitAdded";
    public static string CHARACTER_TRAIT_REMOVED = "OnCharacterTraitRemoved"; //Parameters (Character character, Trait)
    public static string CHARACTER_TRAIT_STACKED = "OnCharacterTraitStacked";
    public static string CHARACTER_TRAIT_UNSTACKED = "OnCharacterTraitUnstacked";
    public static string ADJUSTED_HP = "OnAdjustedHP";
    public static string PARTY_STARTED_TRAVELLING = "OnPartyStartedTravelling"; //Parameters (Party travellingParty)
    public static string PARTY_DONE_TRAVELLING = "OnPartyDoneTravelling"; //Parameters (Party travellingParty)
    public static string CHARACTER_MIGRATED_HOME = "OnCharacterChangedHome"; //Parameters (Character, NPCSettlement previousHome, NPCSettlement newHome); 
    public static string CHARACTER_CHANGED_RACE = "OnCharacterChangedRace"; //Parameters (Character); 
    public static string CHARACTER_ARRIVED_AT_STRUCTURE = "OnCharacterArrivedAtStructure"; //Parameters (Character, LocationStructure); 
    public static string CHARACTER_LEFT_STRUCTURE = "OnCharacterLeftStructure"; //Parameters (Character, LocationStructure);
    public static string RELATIONSHIP_ADDED = "OnCharacterGainedRelationship"; //Parameters (Relatable, Relatable)
    public static string RELATIONSHIP_REMOVED = "OnCharacterRemovedRelationship"; //Parameters (Relatable, RELATIONSHIP_TRAIT, Relatable)
    public static string FORCE_CANCEL_ALL_JOBS_TARGETING_POI = "OnForceCancelAllJobsTargetingPOI"; //Parameters (Character target, string cause)
    public static string FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF = "OnForceCancelAllJobsTargetingPOIExceptSelf"; //Parameters (Character target, string cause)
    public static string STOP_CURRENT_ACTION_TARGETING_POI = "OnStopCurrentActionTargetingPOI";
    public static string STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR = "OnStopCurrentActionTargetingPOIExceptActor";
    public static string CHARACTER_STARTED_STATE = "OnCharacterStartedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_PAUSED_STATE = "OnCharacterPausedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_ENDED_STATE = "OnCharacterEndedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_SWITCHED_ALTER_EGO = "OnCharacterSwitchedAlterEgo"; //Parameters (Character character)
    public static string DETERMINE_COMBAT_REACTION = "DetermineCombatReaction"; //Parameters (Character character)
    public static string CHARACTER_CLASS_CHANGE = "CharacterClassChange";
    public static string BEFORE_SEIZING_POI = "BeforeSeizingPOI";
    public static string ON_SEIZE_POI = "OnSeizePOI";
    public static string ON_UNSEIZE_POI = "OnUnseizePOI";
    public static string CHARACTER_MISSING = "OnCharacterMissing";
    public static string CHARACTER_NO_LONGER_MISSING = "OnCharacterNoLongerMissing";
    public static string ON_SET_AS_FACTION_LEADER = "OnSetAsFactionLeader";
    public static string ON_SET_AS_SETTLEMENT_RULER = "OnSetAsFactionLeader";
    public static string ON_SWITCH_FROM_LIMBO = "OnSwitchFromLimbo";
    public static string INCREASE_THREAT_THAT_SEES_POI = "IncreaseThreatThatSeesPOI";
    public static string INCREASE_THREAT_THAT_SEES_TILE = "IncreaseThreatThatSeesTILE";
    /// <summary>
    /// Parameters (MoodComponent moodComponentModified)
    /// </summary>
    public static string MOOD_SUMMARY_MODIFIED = "OnMoodSummaryModified";
    /// <summary>
    /// Parameters (Character characterWithVision, Character characterRemovedFromVision)
    /// </summary>
    public static string CHARACTER_REMOVED_FROM_VISION = "OnCharacterRemovedFromVision";
    /// <summary>
    /// Parameters (Character characterHit, Character chaarcterHitBy)
    /// </summary>
    public static string CHARACTER_WAS_HIT = "OnCharacterHit";
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_RETURNED_TO_LIFE = "OnCharacterReturnedToLife";
    public static string CHARACTER_BECOMES_MINION_OR_SUMMON = "OnCharacterBecomesMinionOrSummon";
    public static string CHARACTER_BECOMES_NON_MINION_OR_SUMMON = "OnCharacterBecomesNonMinionOrSummon";
    /// <summary>
    /// Parameters (Character, JobQueueItem)
    /// </summary>
    public static string CHARACTER_FINISHED_JOB_SUCCESSFULLY = "OnCharacterFinishedJob";
    //Opinion
    public static string OPINION_INCREASED = "OnOpinionIncreased";
    public static string OPINION_DECREASED = "OnOpinionDecreased";
    public static string OPINION_ADDED = "OnOpinionAdded";
    public static string OPINION_REMOVED = "OnOpinionRemoved";
    /// <summary>
    /// Parameters (Character character, Hextile enteredTile)
    /// </summary>
    public static string CHARACTER_ENTERED_HEXTILE = "OnCharacterEnteredHexTile";
    /// <summary>
    /// Parameters (Character character, HexTile exitedTile)
    /// </summary>
    public static string CHARACTER_EXITED_HEXTILE = "OnCharacterExitedHexTile";
    public static string CHARACTER_CAN_NO_LONGER_MOVE = "OnCharacterCannotMove";
    public static string CHARACTER_CAN_MOVE_AGAIN = "OnCharacterCannotMove";
    /// <summary>
    /// Parameters (INTERRUPT finishedInterrupt, Character character)
    /// </summary>
    public static string INTERRUPT_FINISHED = "OnInterruptFinished";
    /// <summary>
    /// Parameters (Character character, IPointOfInterest whatCharacterSaw)
    /// </summary>
    public static string CHARACTER_SAW = "OnCharacterSaw";
    public static string CHARACTER_CAN_NO_LONGER_PERFORM = "OnCharacterCannotPerform";
    public static string CHARACTER_CAN_PERFORM_AGAIN = "OnCharacterCanPerform";
    /// <summary>
    /// Parameters (IPointOfInterest poiToReprocess)
    /// </summary>
    public static string REPROCESS_POI = "ReprocessPOI";
    /// <summary>
    /// Parameters (Character, CharacterBehaviourComponent)
    /// </summary>
    public static string CHARACTER_REMOVED_BEHAVIOUR = "OnCharacterRemovedBehaviour";
    #endregion

    #region UI
    public static string SHOW_POPUP_MESSAGE = "ShowPopupMessage"; //Parameters (string message, MESSAGE_BOX_MODE mode, bool expires)
    public static string HIDE_POPUP_MESSAGE = "HidePopupMessage";
    public static string UPDATE_UI = "UpdateUI";
    /// <summary>
    /// Parameters (string text, int expiry, UnityAction onClickAction)
    /// </summary>
    public static string SHOW_DEVELOPER_NOTIFICATION = "ShowNotification";
    public static string LOG_ADDED = "OnLogAdded"; //Parameters (object itemThatHadHistoryAdded) either a character or a landmark
    public static string PAUSED = "OnPauseChanged"; //Parameters (bool isGamePaused)
    public static string PROGRESSION_SPEED_CHANGED = "OnProgressionSpeedChanged"; //Parameters (PROGRESSION_SPEED progressionSpeed)
    public static string BEFORE_MENU_OPENED = "BeforeMenuOpened"; //Parameters (UIMenu openedMenu)
    public static string MENU_OPENED = "OnMenuOpened"; //Parameters (UIMenu openedMenu)
    public static string MENU_CLOSED = "OnMenuClosed"; //Parameters (UIMenu closedMenu)
    public static string INTERACTION_MENU_OPENED = "OnInteractionMenuOpened"; //Parameters ()
    public static string INTERACTION_MENU_CLOSED = "OnInteractionMenuClosed"; //Parameters ()
    public static string HIDE_MENUS = "HideMenus";
    public static string DRAG_OBJECT_CREATED = "OnDragObjectCreated"; //Parameters (DragObject obj)
    public static string DRAG_OBJECT_DESTROYED = "OnDragObjectDestroyed"; //Parameters (DragObject obj)
    public static string SHOW_INTEL_NOTIFICATION = "ShowIntelNotification"; //Parameters (Intel)
    public static string SHOW_PLAYER_NOTIFICATION = "ShowPlayerNotification"; //Parameters (Log)
    public static string ON_OPEN_SHARE_INTEL = "OnOpenShareIntel";
    public static string ON_CLOSE_SHARE_INTEL = "OnCloseShareIntel";
    public static string REGION_INFO_UI_UPDATE_APPROPRIATE_CONTENT = "OnAreaInfoUIUpdateAppropriateContent";
    public static string UPDATE_THOUGHT_BUBBLE = "OnUpdateThoughtBubble";
    /// <summary>
    /// Parameters: PopupMenuBase
    /// </summary>
    public static string POPUP_MENU_OPENED = "OnPopupMenuOpened";
    /// <summary>
    /// Parameters: PopupMenuBase
    /// </summary>
    public static string POPUP_MENU_CLOSED = "OnPopupMenuClosed";
    /// <summary>
    /// Parameters: string mainNameplateText
    /// </summary>
    public static string NAMEPLATE_CLICKED = "OnNameplateClicked";
    public static string SPELLS_MENU_SHOWN = "OnSpellsMenuShown";
    /// <summary>
    /// Parameters: string activatedToggle
    /// </summary>
    public static string TOGGLE_TURNED_ON = "ToggleTurnedOn";
    /// <summary>
    /// Parameters: Region selectedRegion
    /// </summary>
    public static string REGION_SELECTED = "OnRegionSelected";
    #endregion

    #region Quest Signals
    public static string CHARACTER_SNATCHED = "OnCharacterSnatched"; //Parameters (Character snatchedCharacter)
    public static string ADD_QUEST_JOB = "OnAddQuestJob"; //Parameter (Quest quest, JobQueueItem job)
    public static string REMOVE_QUEST_JOB = "OnRemoveQuestJob"; //Parameter (Quest quest, JobQueueItem job)
    #endregion

    #region Party
    public static string CHARACTER_JOINED_PARTY = "OnCharacterJoinedParty"; //Parameters (ICharacter characterThatJoined, NewParty affectedParty)
    public static string CHARACTER_LEFT_PARTY = "OnCharacterLeftParty"; //Parameters (ICharacter characterThatLeft, NewParty affectedParty)
    #endregion

    #region Factions
    public static string FACTION_CREATED = "OnFactionCreated"; //Parameters (Faction createdFaction)
    public static string CHARACTER_ADDED_TO_FACTION = "OnCharacterAddedToFaction"; //Parameters (Character addedCharacter, Faction affectedFaction)
    public static string CHARACTER_REMOVED_FROM_FACTION = "OnCharacterRemovedFromFaction"; //Parameters (Character addedCharacter, Faction affectedFaction)
    public static string FACTION_SET = "OnFactionSet"; //Parameters (Character characterThatSetFaction)
    public static string FACTION_LEADER_DIED = "OnFactionLeaderDied"; //Parameters (Faction affectedFaction)
    public static string FACTION_OWNED_REGION_ADDED = "OnFactionOwnedAreaAdded"; //Parameters (Faction affectedFaction, NPCSettlement addedArea)
    public static string FACTION_OWNED_REGION_REMOVED = "OnFactionOwnedAreaRemoved"; //Parameters (Faction affectedFaction, NPCSettlement removedArea)
    public static string FACTION_RELATIONSHIP_CHANGED = "OnFactionRelationshipChanged"; //Parameters (FactionRelationship rel)
    public static string FACTION_ACTIVE_CHANGED = "OnFactionActiveChanged"; //Parameters (Faction affectedFaction)
    public static string CHANGE_FACTION_RELATIONSHIP = "OnChangeFactionRelationship";
    #endregion

    #region Actions
    public static string ACTION_PERFORMED = "OnActionPerformed";
    public static string SCREAM_FOR_HELP = "OnScreamForHelp"; //Parameters (Character characterThatScreamed)
    #endregion

    #region Player
    public static string UPDATED_CURRENCIES = "OnUpdatesCurrencies";
    public static string MINION_ASSIGNED_TO_JOB = "OnCharacterAssignedToJob"; //Parameters (JOB job, Character character);
    public static string MINION_UNASSIGNED_FROM_JOB = "OnCharacterUnassignedFromJob"; //Parameters (JOB job, Character character);
    public static string JOB_ACTION_COOLDOWN_ACTIVATED = "OnJobActionCooldownActivated"; //Parameters (PlayerJobAction action);
    public static string JOB_ACTION_COOLDOWN_DONE = "OnJobActionCooldownDone"; //Parameters (PlayerJobAction action);
    public static string JOB_SLOT_LOCK_CHANGED = "OnJobSlotLockChanged"; //Parameters (JOB job, bool lockedState);
    public static string PLAYER_OBTAINED_INTEL = "OnPlayerObtainedIntel"; //Parameters (InteractionIntel)
    public static string PLAYER_REMOVED_INTEL = "OnPlayerRemovedIntel"; //Parameters (InteractionIntel)
    public static string THREAT_UPDATED = "OnThreatUpdated";
    public static string THREAT_INCREASED = "OnThreadtIncreased";
    /// <summary>
    /// Parameters (List<Character> attacking characters)
    /// </summary>
    public static string THREAT_MAXED_OUT = "OnThreatMaxedOut";
    /// <summary>
    /// Parameters (Summon placedSummon)
    /// </summary>
    public static string PLAYER_PLACED_SUMMON = "OnPlayerPlacedSummon";
    /// <summary>
    /// Parameters (Artifact newArtifact)
    /// </summary>
    public static string PLAYER_GAINED_ARTIFACT = "OnPlayerGainedArtifact";
    public static string PLAYER_GAINED_ARTIFACT_LEVEL = "OnPlayerGainedArtifactLevel";
    /// <summary>
    /// Parameters (Artifact removedArtifact)
    /// </summary>
    public static string PLAYER_REMOVED_ARTIFACT = "OnPlayerRemovedArtifact";
    public static string PLAYER_USED_ARTIFACT = "OnPlayerUsedArtifact";
    /// <summary>
    /// Parameters (NPCSettlement invadedArea)
    /// </summary>
    public static string SUCCESS_INVASION_AREA = "OnPlayerSuccessInvadeArea";
    /// <summary>
    /// parameters (Minion affectedMinion, PlayerJobAction)
    /// </summary>
    public static string PLAYER_LEARNED_INTERVENE_ABILITY = "OnMinionLearnedInterveneAbility";
    public static string PLAYER_CONSUMED_INTERVENE_ABILITY = "OnPlayerConsumedInterveneAbility";
    public static string PLAYER_GAINED_INTERVENE_LEVEL = "OnPlayerGainedInterveneLevel";
    /// <summary>
    /// parameters (Minion)
    /// </summary>
    public static string PLAYER_GAINED_MINION = "OnPlayerGainedMinion";
    /// <summary>
    /// parameters (Minion)
    /// </summary>
    public static string PLAYER_LOST_MINION = "OnPlayerLostMinion";
    /// <summary>
    /// parameters (Minion, BaseLandmark)
    /// </summary>
    public static string MINION_CHANGED_ASSIGNED_REGION = "OnMinionChangedInvadingLandmark";
    /// <summary>
    /// parameters (Minion, BaseLandmark)
    /// </summary>
    public static string MINION_ASSIGNED_PLAYER_LANDMARK = "OnMinionAssignedToPlayerLandmark";
    /// <summary>
    /// parameters (Minion, BaseLandmark)
    /// </summary>
    public static string MINION_UNASSIGNED_PLAYER_LANDMARK = "OnMinionUnassignedFromPlayerLandmark";
    public static string PLAYER_ADJUSTED_MANA = "OnPlayerAdjustedMana";
    /// <summary>
    /// parameters (Minion)
    /// </summary>
    public static string MINION_CHANGED_COMBAT_ABILITY = "OnMinionChangedCombatAbility";
    /// <summary>
    /// parameters (PlayerAction)
    /// </summary>
    public static string ON_EXECUTE_PLAYER_ACTION = "OnExecutePlayerAction";
    /// <summary>
    /// parameters (Affliction)
    /// </summary>
    public static string ON_EXECUTE_AFFLICTION = "OnExecuteAffliction";
    /// <summary>
    /// parameters (Spell)
    /// </summary>
    public static string ON_EXECUTE_SPELL = "OnExecuteSpell";
    /// <summary>
    /// parameters (IPlayerActionTarget)
    /// </summary>
    public static string RELOAD_PLAYER_ACTIONS = "ReloadPlayerActions";
    public static string FORCE_RELOAD_PLAYER_ACTIONS = "ForceReloadPlayerActions";
    /// <summary>
    /// parameters (PlayerAction, IPlayerActionTarget)
    /// </summary>
    public static string PLAYER_ACTION_ADDED_TO_TARGET = "OnPlayerActionAddedToTarget";
    /// <summary>
    /// parameters (PlayerAction, IPlayerActionTarget)
    /// </summary>
    public static string PLAYER_ACTION_REMOVED_FROM_TARGET = "OnPlayerActionRemovedFromTarget";
    /// <summary>
    /// parameters (Vector3 worldPos, int orbCount, InnerTileMap mapLocation)
    /// </summary>
    public static string CREATE_CHAOS_ORBS = "CreateChaosOrbs";
    public static string SUMMON_MINION = "OnSummonMinion";
    public static string UNSUMMON_MINION = "OnUnsummonMinion";
    public static string PLAYER_NO_ACTIVE_SPELL = "OnPlayerNoActiveSpell";
    public static string PLAYER_SET_ACTIVE_SPELL = "OnPlayerSetActiveSpell";
    public static string PLAYER_NO_ACTIVE_MONSTER = "OnPlayerNoActiveMonster";
    public static string PLAYER_NO_ACTIVE_ITEM = "OnPlayerNoActiveItem";
    public static string PLAYER_NO_ACTIVE_ARTIFACT = "OnPlayerNoActiveArtifact";
    public static string PLAYER_GAINED_SPELL = "OnPlayerGainedSpell";
    public static string PLAYER_LOST_SPELL = "OnPlayerLostSpell";
    /// <summary>
    /// Parameters: PlayerAction activatedAction
    /// </summary>
    public static string PLAYER_ACTION_ACTIVATED = "OnPlayerActionActivated";
    /// <summary>
    /// Parameters: IIntel setIntel
    /// </summary>
    public static string ACTIVE_INTEL_SET = "OnPlayerActiveIntelSet";
    public static string HARASS_ACTIVATED = "OnHarassActivated";
    public static string DEFEND_ACTIVATED = "OnDefendActivated";
    #endregion

    #region Interaction
    public static string UPDATED_INTERACTION_STATE = "OnUpdatedInteractionState"; //Parameters (Interaction interaction)
    public static string CHANGED_ACTIVATED_STATE = "OnChangedInteractionState"; //Parameters (Interaction interaction)
    public static string ADDED_INTERACTION = "OnAddedInteraction"; //Parameters (Interaction interaction)
    public static string REMOVED_INTERACTION = "OnRemovedInteraction"; //Parameters (Interaction interaction)
    public static string INTERACTION_ENDED = "OnInteractionEnded"; //Parameters (Interaction interaction)
    public static string MINION_STARTS_INVESTIGATING_AREA = "OnMinionStartInvestigateArea"; //Parameters (Minion minion, NPCSettlement npcSettlement)
    public static string INTERACTION_INITIALIZED = "OnInteractionInitialized"; //Parameters (Interaction interaction)
    public static string EVENT_POPPED_UP = "OnEventPopup"; //Parameters (EventPopup)
    #endregion

    #region Tokens
    //public static string SPECIAL_TOKEN_RAN_OUT = "OnSpecialTokenRanOut"; //Parameters (SpecialToken specialToken)
    public static string SPECIAL_TOKEN_CREATED = "OnSpecialTokenCreated"; //Parameters (SpecialToken specialToken)
    public static string TOKEN_CONSUMED = "OnTokenConsumed"; //Parameters (SpecialToken specialToken)
    #endregion

    #region Jobs/Actions
    public static string CHARACTER_WILL_DO_PLAN = "OnCharacterRecievedPlan"; //Parameters (Character, GoapPlan)
    public static string CHARACTER_DID_ACTION_SUCCESSFULLY = "OnCharacterDidActionSuccessfully"; //Parameters (Character, ActualGoapNode)
    public static string STOP_ACTION = "OnStopAction"; //Parameters (GoapAction)
    public static string CHARACTER_FINISHED_ACTION = "OnCharacterFinishedAction"; //Parameters (Character, GoapAction, String result)
    public static string CHARACTER_DOING_ACTION = "OnCharacterDoingAction"; //Parameters (Character, GoapAction)
    public static string ACTION_STATE_SET = "OnActionStateSet"; //Parameters (Character, GoapAction, GoapActionState)
    public static string AFTER_ACTION_STATE_SET = "OnAfterActionStateSet"; //Parameters (Character, GoapAction, GoapActionState)
    public static string CHARACTER_PERFORMING_ACTION = "OnCharacterPerformingAction"; //Parameters (Character, GoapAction)
    public static string ON_SET_JOB = "OnSetJob"; //Parameters (GoapPlanJob)
    public static string CHECK_JOB_APPLICABILITY = "OnCheckJobApplicability"; //Parameters (JOB_TYPE, IPointOfInterest)
    public static string CHECK_APPLICABILITY_OF_ALL_JOBS_TARGETING = "OnCheckAllJobsTargetingApplicability"; //Parameters (IPointOfInterest)
    /// <summary>
    /// Parameters (JobQueueItem, Character)
    /// </summary>
    public static string JOB_REMOVED_FROM_QUEUE = "OnJobRemovedFromQueue";
    #endregion

    #region Location Grid Tile
    public static string TILE_OCCUPIED = "OnTileOccupied"; //Parameters (LocationGridTile, IPointOfInterest)
    public static string CHECK_GHOST_COLLIDER_VALIDITY = "CheckGhostColliderValidity"; //Parameters (IPointOfInterest, List<LocationGridTile>)
    public static string OBJECT_PLACED_ON_TILE = "OnObjectPlacedOnTile"; //Parameters (LocationGridTile, IPointOfInterest)
    public static string TILE_OBJECT_REMOVED = "OnTileObjectDestroyed"; //Parameters (TileObject, Character removedBy)
    public static string TILE_OBJECT_PLACED = "OnTileObjectPlaced"; //Parameters (TileObject, LocationGridTile)
    public static string TILE_OBJECT_DISABLED = "OnTileObjectDisabled"; //Parameters (TileObject, Character removedBy)
    public static string ITEM_REMOVED_FROM_TILE = "OnItemRemovedFromTile"; //Parameters (SpecialToken, LocationGridTile)
    public static string ITEM_PLACED_ON_TILE = "OnItemPlacedOnTile"; //Parameters (SpecialToken, LocationGridTile)
    #endregion

    #region Combat Ability
    public static string COMBAT_ABILITY_UPDATE_BUTTON = "OnCombatAbilityStopCooldown";
    #endregion

    #region ITraitables
    /// <summary>
    /// Parameters (ITraitable, Trait)
    /// </summary>
    public static string TRAITABLE_GAINED_TRAIT = "OnTraitableGainedTrait";
    /// <summary>
    /// Parameters (ITraitable, Trait, Character removedBy)
    /// </summary>
    public static string TRAITABLE_LOST_TRAIT = "OnTraitableLostTrait";
    #endregion

    #region Structures
    public static string WALL_DAMAGED = "OnWallDamaged";
    public static string WALL_REPAIRED = "OnWallRepaired";
    /// <summary>
    /// parameters:
    /// LocationStructure placedStructure
    /// </summary>
    public static string STRUCTURE_OBJECT_PLACED = "OnStructureObjectPlaced";
    public static string STRUCTURE_OBJECT_REMOVED = "OnStructureObjectRemoved";
    #endregion

    #region POI
    /// <summary>
    /// Parameters (IPointOfInterest damagedObj, int damageAmount)
    /// </summary>
    public static string OBJECT_DAMAGED = "OnObjectDamaged";
    /// <summary>
    /// Parameters (IPointOfInterest repairedObj, int repairAmount)
    /// </summary>
    public static string OBJECT_REPAIRED = "OnObjectRepaired";
    /// <summary>
    /// Parameters (IPointOfInterest repairedObj)
    /// </summary>
    public static string OBJECT_FULLY_REPAIRED = "OnObjectFullyRepaired";
    public static string SPIRIT_OBJECT_NO_DESTINATION = "OnSpiritObjectNoDestination";
    #endregion

    #region Settlements
    /// <summary>
    /// Parameters (NPCSettlement affectedSettlement, bool siegeState)
    /// </summary>
    public static string SETTLEMENT_UNDER_SIEGE_STATE_CHANGED = "OnSettlementSiegeStateChanged";
    /// <summary>
    /// Parameters (ResourcePile resource)
    /// </summary>
    public static string RESOURCE_IN_PILE_CHANGED = "OnResourceInPileChanged";
    /// <summary>
    /// Parameters (Table table)
    /// </summary>
    public static string FOOD_IN_DWELLING_CHANGED = "OnFoodInDwellingChanged";
    public static string NO_ABLE_CHARACTER_INSIDE_SETTLEMENT = "OnNoAbleCharacterInsideSettlement";
    /// <summary>
    /// Parameters: LocationStructure
    /// </summary>
    public static string STRUCTURE_DESTROYED = "OnStructureDestroyed";
    #endregion

    #region Interrupt
    public static string INTERRUPT_STARTED = "OnInterruptStarted";
    #endregion

    #region Particle System
    public static string PARTICLE_EFFECT_DONE = "OnParticleEffectDone";
    #endregion

    #region Tile Objects
    public static string TILE_OBJECT_TRAIT_ADDED = "OnTileObjectTraitAdded";
    public static string TILE_OBJECT_TRAIT_REMOVED = "OnTileObjectTraitRemoved"; //Parameters (Character character, Trait)
    public static string TILE_OBJECT_TRAIT_STACKED = "OnTileObjectTraitStacked";
    public static string TILE_OBJECT_TRAIT_UNSTACKED = "OnTileObjectTraitUnstacked";
    /// <summary>
    /// Parameters: Region
    /// </summary>
    public static string CHECK_UNBUILT_OBJECT_VALIDITY = "CheckUnbuiltObjectValidity";
    #endregion

    #region Tile Object
    public static string ADD_TILE_OBJECT_USER = "OnAddTileObjectUser";
    public static string REMOVE_TILE_OBJECT_USER = "OnAddTileObjectUser";
    #endregion

    #region Quests
    /// <summary>
    /// Parameters: QuestStep completedStep
    /// </summary>
    public static string QUEST_STEP_COMPLETED = "QuestStepCompleted";
    /// <summary>
    /// Parameters: QuestStep failedStep
    /// </summary>
    public static string QUEST_STEP_FAILED = "OnQuestStepFailed";
    /// <summary>
    /// Parameters: QuestStepCollection completedCollection
    /// </summary>
    public static string STEP_COLLECTION_COMPLETED = "StepCollectionCompleted";
    /// <summary>
    /// Parameters: List[Character]
    /// </summary>
    public static string ANGELS_ATTACKING_DEMONIC_STRUCTURE = "OnAngelsAttackingDemonicStructure";
    #endregion
    
    #region Tutorial
    public static string CAMERA_MOVED_BY_PLAYER = "CameraMovedByPlayer";
    /// <summary>
    /// Parameters: ISelectable clickedObject
    /// </summary>
    public static string SELECTABLE_LEFT_CLICKED = "SelectableLeftClicked";
    /// <summary>
    /// Parameters: TutorialQuest completedQuest
    /// </summary>
    public static string TUTORIAL_QUEST_COMPLETED = "TutorialQuestCompleted";
    /// <summary>
    /// Parameters: string identifier
    /// </summary>
    public static string OBJECT_PICKER_SHOWN = "ObjectPickerShown";
    public static string INTEL_MENU_OPENED = "OnIntelMenuOpened";
    /// <summary>
    /// Parameters (TutorialQuestCriteria)
    /// </summary>
    public static string TUTORIAL_QUEST_CRITERIA_MET = "OnTutorialQuestCriteriaMet";
    /// <summary>
    /// Parameters (TutorialQuestCriteria)
    /// </summary>
    public static string TUTORIAL_QUEST_CRITERIA_UNMET = "OnTutorialQuestCriteriaUnMet";
    public static string METEOR_FELL = "OnMeteorFell";
    /// <summary>
    /// Parameters (QuestStep)
    /// </summary>
    public static string UPDATE_QUEST_STEP_ITEM = "UpdateQuestStepItem";
    /// <summary>
    /// Parameters (List[Character], DemonicStructure)
    /// </summary>
    public static string CHARACTERS_ATTACKING_DEMONIC_STRUCTURE = "CharactersAttackingDemonicStructure";
    /// <summary>
    /// Parameters (LocationStructure, Character, GoapPlanJob)
    /// </summary>
    public static string DEMONIC_STRUCTURE_DISCOVERED = "DemonicStructureDiscovered";
    #endregion

    #region Elements
    /// <summary>
    /// Parameters IPointOfInterest target
    /// </summary>
    public static string POISON_EXPLOSION_TRIGGERED = "OnPoisonExplosionTriggered";
    #endregion

    public static Dictionary<string, SignalMethod[]> orderedSignalExecution = new Dictionary<string, SignalMethod[]>() {
        { HOUR_STARTED, new[] {
            new SignalMethod() { methodName = "HourlyJobActions", objectType = typeof(NPCSettlement) },
            new SignalMethod() { methodName = "DecreaseNeeds", objectType = typeof(Character) },
            new SignalMethod() { methodName = "PerHour", objectType = typeof(Infected) },
        }},
        { TICK_STARTED, new[] {
            new SignalMethod() { methodName = "CheckSupply", objectType = typeof(WoodPile) },
            new SignalMethod() { methodName = "CheckFood", objectType = typeof(FoodPile) },
            new SignalMethod() { methodName = "PerTick", objectType = typeof(TimerHubUI) },
            new SignalMethod() { methodName = string.Empty, objectType = typeof(Trait) },
            new SignalMethod() { methodName = "PerTickEffect", objectType = typeof(GoapActionState) },
            new SignalMethod() { methodName = "PerTickGoapPlanGeneration", objectType = typeof(Character) },
            new SignalMethod() { methodName = "PerTickInterventionAbility", objectType = typeof(Player) },
        }},
        { TICK_ENDED, new[] {
            new SignalMethod() { methodName = "CheckSchedule", objectType = typeof(SchedulingManager) },
            new SignalMethod() { methodName = string.Empty, objectType = typeof(Trait) },
            new SignalMethod() { methodName = string.Empty, objectType = typeof(Artifact) },
            new SignalMethod() { methodName = "PerTickMovement", objectType = typeof(CharacterMarker) },
            new SignalMethod() { methodName = "PerTickInState", objectType = typeof(CharacterState) },
            new SignalMethod() { methodName = "PerTickInvasion", objectType = typeof(Player) },
            new SignalMethod() { methodName = "ProcessAllUnprocessedVisionPOIs", objectType = typeof(CharacterMarker) },
            new SignalMethod() { methodName = "OnTickEnded", objectType = typeof(Character) },
        }},
    };
    
    public static bool TryGetMatchingSignalMethod(string eventType, Callback method, out SignalMethod matching) {
        for (int i = 0; i < orderedSignalExecution[eventType].Length; i++) {
            SignalMethod sm = orderedSignalExecution[eventType][i];
            if (sm.Equals(method)) {
                matching = sm;
                return true;
            }
        }
        matching = default(SignalMethod);
        return false;
    }
}

public struct SignalMethod {
    public string methodName;
    public System.Type objectType;

    public bool Equals(Delegate d) {
        if (d.Method.Name.Contains(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType)) {
            return true;
        }
        if (string.IsNullOrEmpty(methodName) && (d.Target.GetType() == objectType || d.Target.GetType().BaseType == objectType)) {
            //if the required method name is null, and the provided object is of the same type, consider it a match
            return true;
        }

        return false;
    }
}