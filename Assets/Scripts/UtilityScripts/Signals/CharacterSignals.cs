public static class CharacterSignals {
    public static string CHARACTER_DEATH = "OnCharacterDied"; //Parameters (Character characterThatDied)
    public static string CHARACTER_CREATED = "OnCharacterCreated"; //Parameters (Character createdCharacter)
    public static string ROLE_CHANGED = "OnCharacterRoleChanged"; //Parameters (Character characterThatChangedRole)
    public static string CHARACTER_REMOVED = "OnCharacterRemoved"; //Parameters (Character removedCharacter)
    public static string CHARACTER_OBTAINED_ITEM = "OnCharacterObtainItem"; //Parameters (SpecialToken obtainedItem, Character characterThatObtainedItem)
    public static string CHARACTER_LOST_ITEM = "OnCharacterLostItem"; //Parameters (SpecialToken unobtainedItem, Character character)
    public static string CHARACTER_TRAIT_ADDED = "OnCharacterTraitAdded"; //Parameters (Character, Trait)
    public static string CHARACTER_TRAIT_REMOVED = "OnCharacterTraitRemoved"; //Parameters (Character character, Trait)
    public static string CHARACTER_TRAIT_STACKED = "OnCharacterTraitStacked";
    public static string CHARACTER_TRAIT_UNSTACKED = "OnCharacterTraitUnstacked";
    public static string CHARACTER_ADJUSTED_HP = "OnAdjustedHP";
    public static string STARTED_TRAVELLING_IN_WORLD = "OnStartedTravellingInWorld"; //Parameters (Character character)
    public static string FINISHED_TRAVELLING_IN_WORLD = "OnFinishedTravellingInWorld"; //Parameters (Character character)
    public static string CHARACTER_MIGRATED_HOME = "OnCharacterChangedHome"; //Parameters (Character, NPCSettlement previousHome, NPCSettlement newHome); 
    public static string CHARACTER_CHANGED_RACE = "OnCharacterChangedRace"; //Parameters (Character); 
    public static string CHARACTER_ARRIVED_AT_STRUCTURE = "OnCharacterArrivedAtStructure"; //Parameters (Character, LocationStructure); 
    public static string CHARACTER_LEFT_STRUCTURE = "OnCharacterLeftStructure"; //Parameters (Character, LocationStructure);
    public static string RELATIONSHIP_CREATED = "OnCharacterGainedRelationship"; //Parameters (Relatable, Relatable)
    public static string RELATIONSHIP_TYPE_ADDED = "OnCharacterGainedRelationshipType"; //Parameters (Relatable, Relatable)
    public static string RELATIONSHIP_REMOVED = "OnCharacterRemovedRelationship"; //Parameters (Relatable, RELATIONSHIP_TRAIT, Relatable)
    public static string FORCE_CANCEL_ALL_JOB_TYPES_TARGETING_POI = "OnForceCancelAllJobTypesTargetingPOI"; //Parameters (Character target, string cause, JOB_TYPE)
    public static string FORCE_CANCEL_ALL_JOBS_TARGETING_POI = "OnForceCancelAllJobsTargetingPOI"; //Parameters (Character target, string cause)
    public static string FORCE_CANCEL_ALL_JOBS_TARGETING_POI_EXCEPT_SELF = "OnForceCancelAllJobsTargetingPOIExceptSelf"; //Parameters (Character target, string cause)
    public static string FORCE_CANCEL_ALL_ACTIONS_TARGETING_POI = "OnForceCancelAllActionsTargetingPOI"; //Parameters (Character target, string cause)
    public static string STOP_CURRENT_ACTION_TARGETING_POI = "OnStopCurrentActionTargetingPOI";
    public static string STOP_CURRENT_ACTION_TARGETING_POI_EXCEPT_ACTOR = "OnStopCurrentActionTargetingPOIExceptActor";
    public static string CHARACTER_STARTED_STATE = "OnCharacterStartedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_PAUSED_STATE = "OnCharacterPausedState"; //Parameters (Character character, CharacterState state)
    public static string CHARACTER_ENDED_STATE = "OnCharacterEndedState"; //Parameters (Character character, CharacterState state)
    public static string DETERMINE_COMBAT_REACTION = "DetermineCombatReaction"; //Parameters (Character character)
    public static string START_FLEE = "OnStartFlee"; //Parameters (Character character)
    public static string CHARACTER_CLASS_CHANGE = "CharacterClassChange";
    public static string BEFORE_SEIZING_POI = "BeforeSeizingPOI";
    public static string ON_SEIZE_POI = "OnSeizePOI";
    public static string ON_UNSEIZE_POI = "OnUnseizePOI";
    public static string CHARACTER_MISSING = "OnCharacterMissing";
    public static string CHARACTER_PRESUMED_DEAD = "OnCharacterPresumedDead";
    public static string ON_SET_AS_FACTION_LEADER = "OnSetAsFactionLeader";
    public static string STARTED_TRAVELLING = "OnStartedTravelling"; //Parameters (Character character)

    /// <summary>
    /// Parameters (Faction, Character previousLeader)
    /// </summary>
    public static string ON_FACTION_LEADER_REMOVED = "OnFactionLeaderRemoved";
    public static string ON_SET_AS_SETTLEMENT_RULER = "OnSetAsSettlementLeader";
    /// <summary>
    /// Parameters (NPCSettlement, Character previousRuler)
    /// </summary>
    public static string ON_SETTLEMENT_RULER_REMOVED = "OnSettlementRulerRemoved";
    public static string ON_SWITCH_FROM_LIMBO = "OnSwitchFromLimbo";
    public static string INCREASE_THREAT_THAT_SEES_POI = "IncreaseThreatThatSeesPOI";
    public static string UPDATE_MOVEMENT_STATE = "OnUpdateMovementState"; //Parameters (Character character)
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
    public static string OPINION_INCREASED = "OnOpinionIncreased";
    public static string OPINION_DECREASED = "OnOpinionDecreased";
    public static string OPINION_ADDED = "OnOpinionAdded";
    public static string OPINION_REMOVED = "OnOpinionRemoved";
    /// <summary>
    /// Parameters (Character actor, Character targetCharacter, string newOpinionLabel)
    /// </summary>
    public static string OPINION_LABEL_DECREASED = "OnOpinionLabelDecreased";
    /// <summary>
    /// Parameters (Character character, Area enteredArea)
    /// </summary>
    public static string CHARACTER_ENTERED_AREA = "OnCharacterEnteredArea";
    /// <summary>
    /// Parameters (Character character, Area exitedArea)
    /// </summary>
    public static string CHARACTER_EXITED_AREA = "OnCharacterExitedArea";
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
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_CAN_NO_LONGER_COMBAT = "OnCharacterCanNoLongerCombat";
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_BECOME_CULTIST = "OnCharacterBecomeCultist";
    /// <summary>
    /// Parameters (Character)
    /// </summary>
    public static string CHARACTER_NO_LONGER_CULTIST = "OnCharacterNoLongerCultist";
    /// <summary>
    /// Parameters Character disguiser, Character target
    /// </summary>
    public static string CHARACTER_DISGUISED = "OnCharacterDisguised";
    /// <summary>
    /// Parameters Character 
    /// </summary>
    public static string CHARACTER_MARKER_DESTROYED = "OnCharacterMarkerDestroyed";
    /// <summary>
    /// Parameters Character 
    /// </summary>
    public static string CHARACTER_MARKER_EXPIRED = "OnCharacterMarkerExpired";
    /// <summary>
    /// Parameters: Character criminal, CRIME_TYPE crimeType, Character accuser
    /// </summary>
    public static string CHARACTER_ACCUSED_OF_CRIME = "OnCharacterAccusedOfCrime";
    /// <summary>
    /// Parameters: Character character
    /// </summary>
    public static string CHARACTER_CHANGED_NAME = "OnCharacterChangedName";
    public static string RENAME_CHARACTER = "OnRenameCharacter";
    /// <summary>
    /// Parameters (Character necromancer)
    /// </summary>
    public static string NECROMANCER_SPAWNED = "OnNecromancerSpawned";
    /// <summary>
    /// Parameters (Character, DemonicStructure)
    /// </summary>
    public static string CHARACTER_HIT_DEMONIC_STRUCTURE = "OnCharacterHitDemonicStructure";
    public static string HEALTH_CRITICALLY_LOW = "OnHealthCriticallyLow";
    public static string CHARACTER_TICK_ENDED_MOVEMENT = "OnTickEndedCharacterMovement";
    public static string PROCESS_ALL_UNPOROCESSED_POIS = "ProcessAllUnprocessedPOIS";
    public static string CHARACTER_TICK_ENDED = "OnCharacterTickEnded";

    public static string CHARACTER_INFO_REVEALED = "OnCharacterInfoRevealed";
    public static string TOGGLE_CHARACTER_MARKER_NAMEPLATE = "OnToggleCharacterMarkerNameplate";

    public static string ON_CHARACTER_RAISE_DEAD_BY_NECRO = "OnCharacterRaiseDeadByNecro";

    public static string ON_ELF_ABSORB_CRYSTAL = "OnElfAbsorbCrystal";
    public static string TRY_CREATE_BURY_JOBS = "CheckBuryJob";

    #region used for chaotic energy generation
    public static string CHARACTER_PRAY_SUCCESS = "OnCharacterPraySuccess";
    public static string CHARACTER_BECAME_VAMPIRE = "OnCharacterBecameVampire";
    public static string CHARACTER_MEDDLER_SCHEME_SUCCESSFUL = "OnCharacterMedlerSchemeSuccessful";
    public static string LYCANTHROPE_SHED_WOLF_PELT = "OnLycanthropeShedWolfPelt";
    #endregion

    #region Equipment
    public static string WEAPON_UNEQUIPPED = "OnWeaponUnequipped";
    public static string ARMOR_UNEQUIPPED = "OnArmorUnequipped";
    public static string ACCESSORY_UNEQUIPPED = "OnAccessoryUnequipped";
    public static string CHARACTER_EQUIPPED_ITEM = "OnCharacterEquippedItem";
    #endregion
}