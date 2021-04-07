public static class PlayerSkillSignals {
    public static string ADDED_PLAYER_MINION_SKILL = "OnAddPlayerMinionSkill";
    public static string ADDED_PLAYER_SUMMON_SKILL = "OnAddPlayerSummonSkill";
    public static string METEOR_FELL = "OnMeteorFell";
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
    public static string ON_EXECUTE_PLAYER_SKILL = "OnExecutePlayerSkill";
    /// <summary>
    /// parameters (SpellData)
    /// </summary>
    public static string SPELL_COOLDOWN_FINISHED = "OnSpellCooldownFinished";
    /// <summary>
    /// parameters (SpellData)
    /// </summary>
    public static string SPELL_COOLDOWN_STARTED = "OnSpellCooldownStarted";
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
    public static string SUMMON_MINION = "OnSummonMinion";
    public static string UNSUMMON_MINION = "OnUnsummonMinion";
    public static string PLAYER_NO_ACTIVE_SPELL = "OnPlayerNoActiveSpell";
    public static string PLAYER_SET_ACTIVE_SPELL = "OnPlayerSetActiveSpell";
    public static string PLAYER_GAINED_SPELL = "OnPlayerGainedSpell";
    public static string PLAYER_LOST_SPELL = "OnPlayerLostSpell";
    /// <summary>
    /// Parameters: PlayerAction activatedAction
    /// </summary>
    public static string PLAYER_ACTION_ACTIVATED = "OnPlayerActionActivated";
    /// <summary>
    /// Parameters: PlayerAction executedAction, IPointOfInterest targetObject
    /// </summary>
    public static string PLAYER_ACTION_EXECUTED_TOWARDS_POI = "OnPlayerActionExecutedTowardsPOI";
    public static string FLAW_TRIGGERED_BY_PLAYER = "OnFlawTriggeredByPlayer";
    /// <summary>
    /// parameters (SpellData)
    /// </summary>
    //public static string PLAYER_SKILL_UPGRADED_ON_SPIRE = "OnPlayerSkillUpgradedOnSpire";
    public static string PLAYER_GAINED_DEMONIC_STRUCTURE = "OnPlayerGainedDemonicStructure";
    public static string PLAYER_SKILL_LEVEL_UP = "OnPlayerSkillLevelUp";
    public static string ON_PLAGUE_POISON_CLOUD_ACTIVATED = "OnPlaguePoisonCloudActivated";

    public static string CHARGES_ADJUSTED = "OnChargesAdjusted";
    public static string BONUS_CHARGES_ADJUSTED = "OnBonusChargesAdjusted";
}