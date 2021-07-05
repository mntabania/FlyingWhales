public static class PlayerSignals {
    public static string UPDATED_CURRENCIES = "OnUpdatesCurrencies";
    public static string PLAYER_OBTAINED_INTEL = "OnPlayerObtainedIntel"; //Parameters (InteractionIntel)
    public static string PLAYER_REMOVED_INTEL = "OnPlayerRemovedIntel"; //Parameters (InteractionIntel)
    public static string THREAT_UPDATED = "OnThreatUpdated";
    public static string THREAT_INCREASED = "OnThreadtIncreased";
    /// <summary>
    /// Parameters (List<Character> attacking characters)
    /// </summary>
    public static string THREAT_MAXED_OUT = "OnThreatMaxedOut";
    public static string THREAT_RESET = "OnThreatReset";
    public static string START_THREAT_EFFECT = "OnStartThreatEffect";
    public static string STOP_THREAT_EFFECT = "OnStopThreatEffect";
    /// <summary>
    /// Parameters (Summon placedSummon)
    /// </summary>
    public static string PLAYER_PLACED_SUMMON = "OnPlayerPlacedSummon";
    public static string PLAYER_GAINED_SUMMON = "OnPlayerGainedSummon";
    public static string PLAYER_LOST_SUMMON = "OnPlayerLostSummon";
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
    public static string MINION_ASSIGNED_PLAYER_LANDMARK = "OnMinionAssignedToPlayerLandmark";
    /// <summary>
    /// parameters (Minion, BaseLandmark)
    /// </summary>
    public static string MINION_UNASSIGNED_PLAYER_LANDMARK = "OnMinionUnassignedFromPlayerLandmark";
    /// <summary>
    /// parameters (int adjustedAmount, int newMana)
    /// </summary>
    public static string PLAYER_ADJUSTED_MANA = "OnPlayerAdjustedMana";
    public static string PLAYER_ADJUSTED_SPIRIT_ENERGY = "OnPlayerAdjustedEnergy";
    /// <summary>
    /// parameters (Vector3 worldPos, int orbCount, InnerTileMap mapLocation)
    /// </summary>
    public static string CREATE_CHAOS_ORBS = "CreateChaosOrbs";
    public static string CREATE_SPIRIT_ENERGY = "CreateSpiritEnergy";
    public static string PLAYER_HIT_CHARACTER_VIA_SPELL = "OnPlayerHitCharacterViaSpell";

    public static string PLAYER_NO_ACTIVE_ITEM = "OnPlayerNoActiveItem";
    public static string PLAYER_NO_ACTIVE_ARTIFACT = "OnPlayerNoActiveArtifact";
    /// <summary>
    /// Parameters: IIntel setIntel
    /// </summary>
    public static string ACTIVE_INTEL_SET = "OnPlayerActiveIntelSet";
    public static string ACTIVE_INTEL_REMOVED = "OnPlayerRemovedActiveIntel";
    public static string HARASS_ACTIVATED = "OnHarassActivated";
    public static string DEFEND_ACTIVATED = "OnDefendActivated";
    public static string INVADE_ACTIVATED = "OnInvadeActivated";
    public static string CHAOS_ORB_SPAWNED = "OnChaosOrbSpawned";
    public static string CHAOS_ORB_DESPAWNED = "OnChaosOrbDespawned";
    public static string CHAOS_ORB_COLLECTED = "OnChaosOrbCollected";
    public static string SPIRIT_ENERGY_SPAWNED = "OnChaosOrbSpawned";
    public static string SPIRIT_ENERGY_DESPAWNED = "OnChaosOrbDespawned";
    public static string SPIRIT_ENERGY_COLLECTED = "OnChaosOrbCollected";
    /// <summary>
    /// Parameters: Chaos Orb
    /// </summary>
    public static string CHAOS_ORB_EXPIRED = "OnChaosOrbExpired";
    public static string SPIRIT_ENERGY_EXPIRED = "OnSpiritEnergyExpired";
    public static string CHECK_IF_PLAYER_WINS = "CheckIfPlayerWins";
    /// <summary>
    /// ParametersL string winMessage
    /// </summary>
    public static string WIN_GAME = "PlayerWins";
    /// <summary>
    /// Parameters IPointOfInterest target
    /// </summary>
    public static string POISON_EXPLOSION_TRIGGERED_BY_PLAYER = "OnPoisonExplosionTriggeredByPlayer";
    public static string ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER = "OnElectricChainTriggeredByPlayer";
    public static string VAPOR_FROM_WIND_TRIGGERED_BY_PLAYER = "OnVaporFromWindTriggeredByPlayer";

    /// <summary>
    /// Parameters: Fatality
    /// </summary>
    public static string ADDED_PLAGUE_DISEASE_FATALITY = "AddedPlagueDiseaseFatality";

    /// <summary>
    /// Parameters: Plague Symptom
    /// </summary>
    public static string ADDED_PLAGUE_DISEASE_SYMPTOM = "AddedPlagueDiseaseSymptom";

    /// <summary>
    /// Parameters: Plague Death Effect
    /// </summary>
    public static string SET_PLAGUE_DEATH_EFFECT = "SetPlagueDeathEffect";
    public static string UNSET_PLAGUE_DEATH_EFFECT = "UnsetPlagueDeathEffect";

    public static string UPDATED_PLAGUE_POINTS = "UpdatedPlaguePoints";
    public static string PLAGUE_POINTS_ADJUSTED = "PlaguePointsAdjusted";

    /// <summary>
    /// Signals for MONSTER UNDERLINGS of PLAYER
    /// </summary>
    public static string UPDATED_MONSTER_UNDERLING = "UpdatedMonsterUnderling";

    /// <summary>
    /// Parameters: SkillData skill, int upgradeCost
    /// </summary>
    public static string PLAYER_CHOSE_SKILL_TO_UNLOCK = "OnPlayerChoseSkillToUnlock";
    /// <summary>
    /// Parameters: SkillData skill, int upgradeCost
    /// </summary>
    public static string PLAYER_FINISHED_SKILL_UNLOCK = "OnPlayerFinishedSkillUnlock";
    public static string PLAYER_SKILL_UNLOCK_CANCELLED = "OnPlayerSkillUnlockCancelled";
    
    public static string PLAYER_STARTED_PORTAL_UPGRADE = "PlayerStartedPortalUpgrade";
    public static string PLAYER_FINISHED_PORTAL_UPGRADE = "OnPlayerFinishedPortalUpgrade";
    public static string PORTAL_UPGRADE_CANCELLED = "PortalUpgradeCancelled";
    
    /// <summary>
    /// Parameters: PLAYER_SKILL_TYPE, int upgradeCost
    /// </summary>
    public static string PLAYER_CHOSE_STRUCTURE_TO_UNLOCK = "OnPlayerChoseStructureToUnlock";
    /// <summary>
    /// Parameters: PLAYER_SKILL_TYPE, int upgradeCost
    /// </summary>
    public static string PLAYER_FINISHED_STRUCTURE_UNLOCK = "OnPlayerFinishedStructureUnlock";
    public static string PLAYER_STRUCTURE_UNLOCK_CANCELLED = "OnStructureUnlockCancelled";
    
    /// <summary>
    /// Parameters: IStoredTarget
    /// </summary>
    public static string PLAYER_STORED_TARGET = "OnPlayerStoredTarget";
    public static string PLAYER_STORED_CHARACTER = "OnPlayerStoredCharacter";
    /// <summary>
    /// Parameters: IStoredTarget
    /// </summary>
    public static string PLAYER_REMOVED_STORED_TARGET = "OnPlayerRemovedStoredTarget";
    public static string PLAYER_REMOVED_STORED_CHARACTER = "OnPlayerRemovedStoredCharacter";

    public static string PLAYER_SUMMON_METER_UPDATE = "OnPlayerSummonMeterUpdate";
    /// <summary>
    /// Parameters: BookmarkCategory
    /// </summary>
    public static string BOOKMARK_CATEGORY_ADDED = "OnBookmarkCategoryAdded";
}