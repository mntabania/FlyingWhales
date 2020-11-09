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
    /// <summary>
    /// parameters (Vector3 worldPos, int orbCount, InnerTileMap mapLocation)
    /// </summary>
    public static string CREATE_CHAOS_ORBS = "CreateChaosOrbs";
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
    /// <summary>
    /// Parameters: Chaos Orb
    /// </summary>
    public static string CHAOS_ORB_EXPIRED = "OnChaosOrbExpired";
    public static string CHARGES_ADJUSTED = "OnChargesAdjusted";
    public static string CHECK_IF_PLAYER_WINS = "CheckIfPlayerWins";
    public static string WIN_GAME = "PlayerWins";
    /// <summary>
    /// Parameters IPointOfInterest target
    /// </summary>
    public static string POISON_EXPLOSION_TRIGGERED_BY_PLAYER = "OnPoisonExplosionTriggeredByPlayer";
    public static string ELECTRIC_CHAIN_TRIGGERED_BY_PLAYER = "OnElectricChainTriggeredByPlayer";
    public static string VAPOR_FROM_WIND_TRIGGERED_BY_PLAYER = "OnVaporFromWindTriggeredByPlayer";
}