using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Object_Scripts;
using UnityEngine;
using UnityEngine.Assertions;

public class PlayerSkillManager : MonoBehaviour {
    public static PlayerSkillManager Instance;

    public PlayerSkillTree[] allSkillTrees;
    public PlayerArchetypeLoadoutDictionary allSkillLoadouts;

    public PLAYER_ARCHETYPE selectedArchetype { get; private set; }

    [SerializeField] private bool _unlockAllSkills;
    [SerializeField] private bool _unlimitedCast;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public bool unlimitedCast => _unlimitedCast;
    public bool unlockAllSkills => _unlockAllSkills || WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled;
#else
    public bool unlimitedCast => false;
    public bool unlockAllSkills => false || WorldSettings.Instance.worldSettingsData.playerSkillSettings.omnipotentMode == OMNIPOTENT_MODE.Enabled;
#endif

    [SerializeField] private PlayerSkillDataDictionary _playerSkillDataDictionary;

    public Dictionary<PLAYER_SKILL_TYPE, SkillData> allSpellsData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, PlayerAction> allPlayerActionsData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, AfflictData> allAfflictionsData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, SchemeData> allSchemesData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, DemonicStructurePlayerSkill> allDemonicStructureSkillsData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, MinionPlayerSkill> allMinionPlayerSkillsData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, SummonPlayerSkill> allSummonPlayerSkillsData { get; private set; }
    public Dictionary<PLAYER_SKILL_TYPE, SkillData> allPlayerSkillsData { get; private set; }
    public Dictionary<PASSIVE_SKILL, PassiveSkill> passiveSkillsData { get; private set; }

    public Dictionary<string, PLAYER_SKILL_TYPE> afflictionsNameSkillTypeDictionary = new Dictionary<string, PLAYER_SKILL_TYPE>()
    {
        { "Agoraphobic", PLAYER_SKILL_TYPE.AGORAPHOBIA },
        { "Alcoholic", PLAYER_SKILL_TYPE.ALCOHOLIC },
        { "Cannibal", PLAYER_SKILL_TYPE.CANNIBALISM },
        { "Coward", PLAYER_SKILL_TYPE.COWARDICE },
        { "Glutton", PLAYER_SKILL_TYPE.GLUTTONY },
        { "Hothead", PLAYER_SKILL_TYPE.HOTHEADED },
        { "Kleptomaniac", PLAYER_SKILL_TYPE.KLEPTOMANIA },
        { "Lazy", PLAYER_SKILL_TYPE.GLUTTONY },
        { "Lycanthrope", PLAYER_SKILL_TYPE.LYCANTHROPY },
        { "Music Hater", PLAYER_SKILL_TYPE.MUSIC_HATER },
        { "Narcoleptic", PLAYER_SKILL_TYPE.NARCOLEPSY },
        { "Paralyzed", PLAYER_SKILL_TYPE.PARALYSIS },
        { "Plagued", PLAYER_SKILL_TYPE.PLAGUE },
        { "Psychopath", PLAYER_SKILL_TYPE.PSYCHOPATHY },
        { "Pyrophobic", PLAYER_SKILL_TYPE.PYROPHOBIA },
        { "Unfaithful", PLAYER_SKILL_TYPE.UNFAITHFULNESS },
        { "Vampire", PLAYER_SKILL_TYPE.VAMPIRISM },
    };

    #region getters
    public PlayerSkillDataDictionary playerSkillDataDictionary => _playerSkillDataDictionary;
    #endregion

    [NonSerialized]
    public List<PLAYER_SKILL_TYPE> constantSkills = new List<PLAYER_SKILL_TYPE> { PLAYER_SKILL_TYPE.AFFLICT, PLAYER_SKILL_TYPE.BUILD_DEMONIC_STRUCTURE/*, PLAYER_SKILL_TYPE.UNSUMMON*/, 
        /*PLAYER_SKILL_TYPE.BREED_MONSTER,*/ PLAYER_SKILL_TYPE.TORTURE, PLAYER_SKILL_TYPE.BRAINWASH, PLAYER_SKILL_TYPE.EVANGELIZE,/*, SPELL_TYPE.CULTIST_TRANSFORM,*/ PLAYER_SKILL_TYPE.CULTIST_POISON,
        PLAYER_SKILL_TYPE.SACRIFICE, PLAYER_SKILL_TYPE.REPAIR, PLAYER_SKILL_TYPE.FOUND_CULT, PLAYER_SKILL_TYPE.SPREAD_RUMOR, PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP, PLAYER_SKILL_TYPE.UPGRADE,
        PLAYER_SKILL_TYPE.INSTIGATE_WAR, PLAYER_SKILL_TYPE.RESIGN, PLAYER_SKILL_TYPE.LEAVE_FACTION, PLAYER_SKILL_TYPE.LEAVE_HOME, PLAYER_SKILL_TYPE.LEAVE_VILLAGE, PLAYER_SKILL_TYPE.BREAK_UP,
        PLAYER_SKILL_TYPE.JOIN_FACTION, PLAYER_SKILL_TYPE.REBELLION, PLAYER_SKILL_TYPE.SCHEME, PLAYER_SKILL_TYPE.OVERTHROW_LEADER, PLAYER_SKILL_TYPE.STIFLE_MIGRATION, PLAYER_SKILL_TYPE.INDUCE_MIGRATION,
        /*PLAYER_SKILL_TYPE.EXPEL,*/ PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION, PLAYER_SKILL_TYPE.SPAWN_EYE_WARD, PLAYER_SKILL_TYPE.DESTROY_EYE_WARD, PLAYER_SKILL_TYPE.DRAIN_SPIRIT,
        PLAYER_SKILL_TYPE.LET_GO, PLAYER_SKILL_TYPE.FULL_HEAL, PLAYER_SKILL_TYPE.CREATE_BLACKMAIL, PLAYER_SKILL_TYPE.RELEASE_ABILITIES, PLAYER_SKILL_TYPE.SNATCH_VILLAGER, PLAYER_SKILL_TYPE.SNATCH_MONSTER,
        PLAYER_SKILL_TYPE.RAID, PLAYER_SKILL_TYPE.UPGRADE_ABILITIES, PLAYER_SKILL_TYPE.DEFEND, PLAYER_SKILL_TYPE.UPGRADE_PORTAL, PLAYER_SKILL_TYPE.DESTROY_STRUCTURE, PLAYER_SKILL_TYPE.SPAWN_PARTY, PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL, PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL,
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allSpells = { PLAYER_SKILL_TYPE.METEOR
            , PLAYER_SKILL_TYPE.TORNADO, PLAYER_SKILL_TYPE.RAVENOUS_SPIRIT, PLAYER_SKILL_TYPE.FEEBLE_SPIRIT, PLAYER_SKILL_TYPE.FORLORN_SPIRIT
            , PLAYER_SKILL_TYPE.LIGHTNING, PLAYER_SKILL_TYPE.POISON_CLOUD, PLAYER_SKILL_TYPE.EARTHQUAKE
            , PLAYER_SKILL_TYPE.WATER_BOMB, PLAYER_SKILL_TYPE.MANIFEST_FOOD
            , PLAYER_SKILL_TYPE.BRIMSTONES, PLAYER_SKILL_TYPE.SPLASH_POISON, PLAYER_SKILL_TYPE.LOCUST_SWARM, PLAYER_SKILL_TYPE.BLIZZARD, PLAYER_SKILL_TYPE.RAIN
            , PLAYER_SKILL_TYPE.BALL_LIGHTNING, PLAYER_SKILL_TYPE.ELECTRIC_STORM, PLAYER_SKILL_TYPE.FROSTY_FOG, PLAYER_SKILL_TYPE.VAPOR, PLAYER_SKILL_TYPE.FIRE_BALL
            , PLAYER_SKILL_TYPE.POISON_BLOOM, PLAYER_SKILL_TYPE.LANDMINE, PLAYER_SKILL_TYPE.TERRIFYING_HOWL, PLAYER_SKILL_TYPE.FREEZING_TRAP, PLAYER_SKILL_TYPE.SNARE_TRAP, PLAYER_SKILL_TYPE.WIND_BLAST, PLAYER_SKILL_TYPE.ICE_BLAST, PLAYER_SKILL_TYPE.EARTH_SPIKE, PLAYER_SKILL_TYPE.WATER_SPIKE
            , PLAYER_SKILL_TYPE.ICETEROIDS, PLAYER_SKILL_TYPE.HEAT_WAVE, PLAYER_SKILL_TYPE.SPLASH_WATER, PLAYER_SKILL_TYPE.WALL, PLAYER_SKILL_TYPE.PROTECTION, PLAYER_SKILL_TYPE.PLAGUED_RAT, PLAYER_SKILL_TYPE.SPAWN_NECRONOMICON, PLAYER_SKILL_TYPE.SPAWN_RATMAN,
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allPlayerActions = { PLAYER_SKILL_TYPE.ZAP, PLAYER_SKILL_TYPE.RAISE_DEAD, PLAYER_SKILL_TYPE.DESTROY, PLAYER_SKILL_TYPE.IGNITE, PLAYER_SKILL_TYPE.POISON
            , PLAYER_SKILL_TYPE.TORTURE, PLAYER_SKILL_TYPE.SEIZE_OBJECT, PLAYER_SKILL_TYPE.SEIZE_CHARACTER, PLAYER_SKILL_TYPE.SEIZE_MONSTER
            , PLAYER_SKILL_TYPE.BUILD_DEMONIC_STRUCTURE, PLAYER_SKILL_TYPE.AFFLICT/*, PLAYER_SKILL_TYPE.BREED_MONSTER*/ //, SPELL_TYPE.ACTIVATE
            , PLAYER_SKILL_TYPE.AGITATE, PLAYER_SKILL_TYPE.HEAL/*, SPELL_TYPE.ANIMATE, SPELL_TYPE.EMPOWER*/
            , PLAYER_SKILL_TYPE.BRAINWASH/*, PLAYER_SKILL_TYPE.UNSUMMON*/, PLAYER_SKILL_TYPE.TRIGGER_FLAW/*, SPELL_TYPE.CULTIST_TRANSFORM*/, PLAYER_SKILL_TYPE.CULTIST_POISON
            , PLAYER_SKILL_TYPE.CULTIST_BOOBY_TRAP, /*PLAYER_SKILL_TYPE.SNATCH,*/ PLAYER_SKILL_TYPE.SACRIFICE, PLAYER_SKILL_TYPE.REPAIR, PLAYER_SKILL_TYPE.SPREAD_RUMOR, PLAYER_SKILL_TYPE.EVANGELIZE
            , PLAYER_SKILL_TYPE.FOUND_CULT, PLAYER_SKILL_TYPE.UPGRADE, PLAYER_SKILL_TYPE.SCHEME, PLAYER_SKILL_TYPE.RELEASE, PLAYER_SKILL_TYPE.EXPEL
            , PLAYER_SKILL_TYPE.REMOVE_BUFF, PLAYER_SKILL_TYPE.REMOVE_FLAW, PLAYER_SKILL_TYPE.CULTIST_JOIN_FACTION, PLAYER_SKILL_TYPE.SPAWN_EYE_WARD, PLAYER_SKILL_TYPE.DESTROY_EYE_WARD
            , PLAYER_SKILL_TYPE.DRAIN_SPIRIT, PLAYER_SKILL_TYPE.LET_GO, PLAYER_SKILL_TYPE.FULL_HEAL, PLAYER_SKILL_TYPE.CREATE_BLACKMAIL, PLAYER_SKILL_TYPE.RELEASE_ABILITIES
            , PLAYER_SKILL_TYPE.SNATCH_VILLAGER, PLAYER_SKILL_TYPE.SNATCH_MONSTER, PLAYER_SKILL_TYPE.RAID, PLAYER_SKILL_TYPE.UPGRADE_ABILITIES, PLAYER_SKILL_TYPE.DEFEND, PLAYER_SKILL_TYPE.UPGRADE_PORTAL
            , PLAYER_SKILL_TYPE.DESTROY_STRUCTURE, PLAYER_SKILL_TYPE.EMPOWER, PLAYER_SKILL_TYPE.SPAWN_PARTY, PLAYER_SKILL_TYPE.INDUCE_MIGRATION, PLAYER_SKILL_TYPE.STIFLE_MIGRATION, PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL, PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL,
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allAfflictions = { PLAYER_SKILL_TYPE.CANNIBALISM
            , PLAYER_SKILL_TYPE.LYCANTHROPY, PLAYER_SKILL_TYPE.VAMPIRISM, PLAYER_SKILL_TYPE.KLEPTOMANIA
            , PLAYER_SKILL_TYPE.UNFAITHFULNESS, PLAYER_SKILL_TYPE.ALCOHOLIC
            , PLAYER_SKILL_TYPE.AGORAPHOBIA, PLAYER_SKILL_TYPE.PARALYSIS/*, SPELL_TYPE.ZOMBIE_VIRUS*/
            , PLAYER_SKILL_TYPE.PLAGUE, PLAYER_SKILL_TYPE.PSYCHOPATHY, PLAYER_SKILL_TYPE.COWARDICE, PLAYER_SKILL_TYPE.PYROPHOBIA
            , PLAYER_SKILL_TYPE.NARCOLEPSY, PLAYER_SKILL_TYPE.HOTHEADED, PLAYER_SKILL_TYPE.LAZINESS, PLAYER_SKILL_TYPE.MUSIC_HATER, PLAYER_SKILL_TYPE.GLUTTONY
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allSchemes = { PLAYER_SKILL_TYPE.INSTIGATE_WAR
            , PLAYER_SKILL_TYPE.RESIGN, PLAYER_SKILL_TYPE.LEAVE_FACTION, PLAYER_SKILL_TYPE.LEAVE_HOME
            , PLAYER_SKILL_TYPE.LEAVE_VILLAGE, PLAYER_SKILL_TYPE.BREAK_UP
            , PLAYER_SKILL_TYPE.JOIN_FACTION, PLAYER_SKILL_TYPE.REBELLION, PLAYER_SKILL_TYPE.OVERTHROW_LEADER
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allDemonicStructureSkills = { PLAYER_SKILL_TYPE.MEDDLER, PLAYER_SKILL_TYPE.WATCHER, PLAYER_SKILL_TYPE.CRYPT,
        PLAYER_SKILL_TYPE.KENNEL, /*SPELL_TYPE.OSTRACIZER,*/ PLAYER_SKILL_TYPE.TORTURE_CHAMBERS, /*SPELL_TYPE.DEMONIC_PRISON,*/ PLAYER_SKILL_TYPE.DEFILER, PLAYER_SKILL_TYPE.BIOLAB, 
        PLAYER_SKILL_TYPE.SPIRE, PLAYER_SKILL_TYPE.MANA_PIT, PLAYER_SKILL_TYPE.MARAUD, PLAYER_SKILL_TYPE.DEFENSE_POINT, PLAYER_SKILL_TYPE.IMP_HUT,
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allMinionPlayerSkills = { PLAYER_SKILL_TYPE.DEMON_WRATH, PLAYER_SKILL_TYPE.DEMON_PRIDE, PLAYER_SKILL_TYPE.DEMON_LUST
        , PLAYER_SKILL_TYPE.DEMON_GLUTTONY, PLAYER_SKILL_TYPE.DEMON_SLOTH, PLAYER_SKILL_TYPE.DEMON_ENVY, PLAYER_SKILL_TYPE.DEMON_GREED,
    };

    [NonSerialized]
    public PLAYER_SKILL_TYPE[] allSummonPlayerSkills = { PLAYER_SKILL_TYPE.SKELETON, PLAYER_SKILL_TYPE.WOLF, PLAYER_SKILL_TYPE.GOLEM, PLAYER_SKILL_TYPE.INCUBUS, PLAYER_SKILL_TYPE.SUCCUBUS, PLAYER_SKILL_TYPE.FIRE_ELEMENTAL, PLAYER_SKILL_TYPE.KOBOLD, PLAYER_SKILL_TYPE.GHOST,
    PLAYER_SKILL_TYPE.ABOMINATION, PLAYER_SKILL_TYPE.MIMIC, PLAYER_SKILL_TYPE.PIG, PLAYER_SKILL_TYPE.CHICKEN, PLAYER_SKILL_TYPE.SHEEP, PLAYER_SKILL_TYPE.SLUDGE,
    PLAYER_SKILL_TYPE.WATER_NYMPH, PLAYER_SKILL_TYPE.WIND_NYMPH, PLAYER_SKILL_TYPE.ICE_NYMPH,
    PLAYER_SKILL_TYPE.ELECTRIC_WISP, PLAYER_SKILL_TYPE.EARTHEN_WISP, PLAYER_SKILL_TYPE.FIRE_WISP,
    PLAYER_SKILL_TYPE.GRASS_ENT, PLAYER_SKILL_TYPE.SNOW_ENT, PLAYER_SKILL_TYPE.CORRUPT_ENT, PLAYER_SKILL_TYPE.DESERT_ENT, PLAYER_SKILL_TYPE.FOREST_ENT,
    PLAYER_SKILL_TYPE.GIANT_SPIDER, PLAYER_SKILL_TYPE.SMALL_SPIDER,
    PLAYER_SKILL_TYPE.VENGEFUL_GHOST, PLAYER_SKILL_TYPE.WURM, PLAYER_SKILL_TYPE.TROLL, PLAYER_SKILL_TYPE.REVENANT, PLAYER_SKILL_TYPE.BONE_GOLEM, PLAYER_SKILL_TYPE.SCORPION, PLAYER_SKILL_TYPE.HARPY };

    [NonSerialized]
    public PASSIVE_SKILL[] allPassiveSkillTypes = { PASSIVE_SKILL.Prayer_Chaos_Orb, PASSIVE_SKILL.Auto_Absorb_Chaos_Orb, PASSIVE_SKILL.Spell_Damage_Chaos_Orb, PASSIVE_SKILL.Mental_Break_Chaos_Orb,
        PASSIVE_SKILL.Plague_Chaos_Orb, PASSIVE_SKILL.Player_Success_Raid_Chaos_Orb, PASSIVE_SKILL.Dark_Ritual_Chaos_Orb, PASSIVE_SKILL.Raid_Chaos_Orb, PASSIVE_SKILL.Night_Creature_Chaos_Orb,
        PASSIVE_SKILL.Meddler_Chaos_Orb, PASSIVE_SKILL.Trigger_Flaw_Chaos_Orb, PASSIVE_SKILL.Lycanthrope_Chaos_Orb, PASSIVE_SKILL.Trap_Chaos_Orb, PASSIVE_SKILL.Skill_Base_Chaos_Orb,
    };
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }
    private void Start() {
        Initialize();
    }

    public void Initialize() {
        // SPELL_TYPE[] allSpellTypes = UtilityScripts.CollectionUtilities.GetEnumValues<SPELL_TYPE>();
        allPlayerSkillsData = new Dictionary<PLAYER_SKILL_TYPE, SkillData>();
        ConstructAllSpellsData();
        ConstructAllPlayerActionsData();
        ConstructAllAfflictionsData();
        ConstructAllSchemesData();
        ConstructAllDemonicStructureSkillsData();
        ConstructAllMinionPlayerSkillsData();
        ConstructAllSummonPlayerSkillsData();
        ConstructPassiveSkills();
    }

    #region Utilities
    private void ConstructAllSpellsData() {
        allSpellsData = new Dictionary<PLAYER_SKILL_TYPE, SkillData>();
        for (int i = 0; i < allSpells.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allSpells[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                SkillData spellData = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as SkillData;
                allSpellsData.Add(spellType, spellData);
                allPlayerSkillsData.Add(spellType, spellData);
            }
        }
    }
    private void ConstructAllPlayerActionsData() {
        allPlayerActionsData = new Dictionary<PLAYER_SKILL_TYPE, PlayerAction>();
        //SPELL_TYPE[] allPlayerActions = { SPELL_TYPE.ZAP, SPELL_TYPE.RAISE_DEAD, SPELL_TYPE.DESTROY, SPELL_TYPE.IGNITE, SPELL_TYPE.POISON
        //    , SPELL_TYPE.TORTURE, SPELL_TYPE.SUMMON_MINION, SPELL_TYPE.STOP, SPELL_TYPE.SEIZE_OBJECT, SPELL_TYPE.SEIZE_CHARACTER, SPELL_TYPE.SEIZE_MONSTER
        //    /*, SPELL_TYPE.RETURN_TO_PORTAL*/, SPELL_TYPE.DEFEND, SPELL_TYPE.HARASS, SPELL_TYPE.INVADE, SPELL_TYPE.LEARN_SPELL/*, SPELL_TYPE.CHANGE_COMBAT_MODE*/
        //    , SPELL_TYPE.BUILD_DEMONIC_STRUCTURE, SPELL_TYPE.AFFLICT, SPELL_TYPE.ACTIVATE_TILE_OBJECT, SPELL_TYPE.BREED_MONSTER
        //    /*, SPELL_TYPE.END_RAID, SPELL_TYPE.END_HARASS, SPELL_TYPE.END_INVADE*/, SPELL_TYPE.INTERFERE, SPELL_TYPE.PLANT_GERM
        //    , SPELL_TYPE.AGITATE, SPELL_TYPE.KNOCKOUT, SPELL_TYPE.KILL, SPELL_TYPE.HEAL, SPELL_TYPE.ABDUCT, SPELL_TYPE.ANIMATE, SPELL_TYPE.EMPOWER
        //    , SPELL_TYPE.BRAINWASH, SPELL_TYPE.UNSUMMON,
        //};
        for (int i = 0; i < allPlayerActions.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allPlayerActions[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";

                PlayerAction playerAction = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as PlayerAction;
                allPlayerActionsData.Add(spellType, playerAction);
                allPlayerSkillsData.Add(spellType, playerAction);
            }
        }
    }
    private void ConstructAllDemonicStructureSkillsData() {
        allDemonicStructureSkillsData = new Dictionary<PLAYER_SKILL_TYPE, DemonicStructurePlayerSkill>();
        for (int i = 0; i < allDemonicStructureSkills.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allDemonicStructureSkills[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                DemonicStructurePlayerSkill demonicStructureSkill = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as DemonicStructurePlayerSkill;
                allDemonicStructureSkillsData.Add(spellType, demonicStructureSkill);
                allPlayerSkillsData.Add(spellType, demonicStructureSkill);
            }
        }
    }
    private void ConstructAllAfflictionsData() {
        allAfflictionsData = new Dictionary<PLAYER_SKILL_TYPE, AfflictData>();
        for (int i = 0; i < allAfflictions.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allAfflictions[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                AfflictData affliction = System.Activator.CreateInstance(System.Type.GetType(typeName) ?? throw new Exception($"Problem with creating spell data for {typeName}")) as AfflictData;
                allAfflictionsData.Add(spellType, affliction);
                allPlayerSkillsData.Add(spellType, affliction);
            }
        }
    }
    private void ConstructAllSchemesData() {
        allSchemesData = new Dictionary<PLAYER_SKILL_TYPE, SchemeData>();
        for (int i = 0; i < allSchemes.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allSchemes[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                SchemeData scheme = System.Activator.CreateInstance(System.Type.GetType(typeName) ?? throw new Exception($"Problem with creating spell data for {typeName}")) as SchemeData;
                allSchemesData.Add(spellType, scheme);
                allPlayerSkillsData.Add(spellType, scheme);
            }
        }
    }
    private void ConstructAllMinionPlayerSkillsData() {
        allMinionPlayerSkillsData = new Dictionary<PLAYER_SKILL_TYPE, MinionPlayerSkill>();
        for (int i = 0; i < allMinionPlayerSkills.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allMinionPlayerSkills[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                MinionPlayerSkill minionPlayerSkill = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as MinionPlayerSkill;
                allMinionPlayerSkillsData.Add(spellType, minionPlayerSkill);
                allPlayerSkillsData.Add(spellType, minionPlayerSkill);
            }
        }
    }
    private void ConstructAllSummonPlayerSkillsData() {
        allSummonPlayerSkillsData = new Dictionary<PLAYER_SKILL_TYPE, SummonPlayerSkill>();
        for (int i = 0; i < allSummonPlayerSkills.Length; i++) {
            PLAYER_SKILL_TYPE spellType = allSummonPlayerSkills[i];
            if (spellType != PLAYER_SKILL_TYPE.NONE) {
                var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
                SummonPlayerSkill summonPlayerSkill = System.Activator.CreateInstance(System.Type.GetType(typeName) ?? throw new Exception($"Problem with creating spell data for {typeName}")) as SummonPlayerSkill;
                allSummonPlayerSkillsData.Add(spellType, summonPlayerSkill);
                allPlayerSkillsData.Add(spellType, summonPlayerSkill);
            }
        }
    }
    public bool IsSpell(PLAYER_SKILL_TYPE type) {
        return allSpells.Contains(type);
    }
    public bool IsAffliction(PLAYER_SKILL_TYPE type) {
        return allAfflictions.Contains(type);
    }
    public bool IsScheme(PLAYER_SKILL_TYPE type) {
        return allSchemes.Contains(type);
    }
    public bool IsMinion(PLAYER_SKILL_TYPE type) {
        return allMinionPlayerSkills.Contains(type);
    }
    public bool IsPlayerAction(PLAYER_SKILL_TYPE type) {
        return allPlayerActions.Contains(type);
    }
    public bool IsDemonicStructure(PLAYER_SKILL_TYPE type) {
        return allDemonicStructureSkills.Contains(type);
    }
    public SkillData GetSkillData(PLAYER_SKILL_TYPE type) {
        if (allPlayerSkillsData.ContainsKey(type)) {
            return allPlayerSkillsData[type];
        }
        return null;
    }
    public SkillData GetSpellData(PLAYER_SKILL_TYPE type) {
        if (allSpellsData.ContainsKey(type)) {
            return allSpellsData[type];
        }
        return null;
    }
    public AfflictData GetAfflictionData(PLAYER_SKILL_TYPE type) {
        if (allAfflictionsData.ContainsKey(type)) {
            return allAfflictionsData[type];
        }
        return null;
    }
    public SchemeData GetSchemeData(PLAYER_SKILL_TYPE type) {
        if (allSchemesData.ContainsKey(type)) {
            return allSchemesData[type];
        }
        return null;
    }
    public PlayerAction GetPlayerActionData(PLAYER_SKILL_TYPE type) {
        if (allPlayerActionsData.ContainsKey(type)) {
            return allPlayerActionsData[type];
        }
        return null;
    }
    public DemonicStructurePlayerSkill GetDemonicStructureSkillData(PLAYER_SKILL_TYPE type) {
        if (allDemonicStructureSkillsData.ContainsKey(type)) {
            return allDemonicStructureSkillsData[type];
        }
        return null;
    }
    public DemonicStructurePlayerSkill GetDemonicStructureSkillData(STRUCTURE_TYPE type) {
        Assert.IsTrue(type.IsPlayerStructure());
        PLAYER_SKILL_TYPE skillType = (PLAYER_SKILL_TYPE)Enum.Parse(typeof(PLAYER_SKILL_TYPE), type.ToString());
        return GetDemonicStructureSkillData(skillType);
    }
    public MinionPlayerSkill GetMinionPlayerSkillData(PLAYER_SKILL_TYPE type) {
        if (allMinionPlayerSkillsData.ContainsKey(type)) {
            return allMinionPlayerSkillsData[type];
        }
        return null;
    }
    public MinionPlayerSkill GetMinionPlayerSkillDataByMinionType(MINION_TYPE type) {
        for (int i = 0; i < allMinionPlayerSkills.Length; i++) {
            MinionPlayerSkill skill = GetMinionPlayerSkillData(allMinionPlayerSkills[i]);
            if(skill.minionType == type) {
                return skill;
            }
        }
        return null;
    }
    public SummonPlayerSkill GetSummonPlayerSkillData(PLAYER_SKILL_TYPE type) {
        if (allSummonPlayerSkillsData.ContainsKey(type)) {
            return allSummonPlayerSkillsData[type];
        }
        return null;
    }
    public SummonPlayerSkill GetSummonPlayerSkillData(RACE race, string className) {
        foreach (SummonPlayerSkill value in allSummonPlayerSkillsData.Values) {
            if(value.race == race && value.className == className) {
                return value;
            }
        }
        return null;
    }
    public PlayerSkillTreeNode GetPlayerSkillTreeNode(PLAYER_SKILL_TYPE skillType) {
        for (int i = 0; i < allSkillTrees.Length; i++) {
            PlayerSkillTree skillTree = allSkillTrees[i];
            if (skillTree.nodes.ContainsKey(skillType)) {
                return skillTree.nodes[skillType];
            }
        }
        return null;
    }
    public void SetSelectedArchetype(PLAYER_ARCHETYPE archetype) {
        selectedArchetype = archetype;
    }
    public PlayerSkillLoadout GetSelectedLoadout() {
        if(selectedArchetype == PLAYER_ARCHETYPE.Normal) {
            selectedArchetype = PLAYER_ARCHETYPE.Ravager;
        }
        return allSkillLoadouts[selectedArchetype];
    }
    public void ResetSpellsInUse() {
        for (int i = 0; i < allPlayerSkillsData.Values.Count; i++) {
            SkillData spellData = allPlayerSkillsData.Values.ElementAt(i);
            spellData.ResetData();
        }
    }
    public void ResetSummonPlayerSkills() {
        for (int i = 0; i < allSummonPlayerSkills.Length; i++) {
            SkillData spellData = allSummonPlayerSkillsData[allSummonPlayerSkills[i]];
            spellData.ResetData();
        }
    }
    #endregion

    #region Assets
    public T GetScriptableObjPlayerSkillData<T>(PLAYER_SKILL_TYPE spellType) where T : PlayerSkillData {
        if (_playerSkillDataDictionary.ContainsKey(spellType)) {
            return _playerSkillDataDictionary[spellType] as T;    
        }
        return null;
    }
    #endregion

    #region Passive Skills
    private void ConstructPassiveSkills() {
        passiveSkillsData = new Dictionary<PASSIVE_SKILL, PassiveSkill>();
        for (int i = 0; i < allPassiveSkillTypes.Length; i++) {
            PASSIVE_SKILL passiveSkillType = allPassiveSkillTypes[i];
            var typeName = $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(passiveSkillType.ToString())}, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            PassiveSkill passiveSkill = Activator.CreateInstance(Type.GetType(typeName) ?? throw new Exception($"Problem with creating spell data for {typeName}")) as PassiveSkill;
            passiveSkillsData.Add(passiveSkillType, passiveSkill);
        }
    }
    public PassiveSkill GetPassiveSkill(PASSIVE_SKILL passiveSkill) {
        if (passiveSkillsData.ContainsKey(passiveSkill)) {
            return passiveSkillsData[passiveSkill];
        }
        throw new Exception($"Could not find class for passive skill {passiveSkill.ToString()}");
    }
    #endregion

    #region utility
    public int GetDamageBaseOnLevel(PLAYER_SKILL_TYPE p_skillType, int p_forcedLevel = -1) {
        SkillData skillData = GetSkillData(p_skillType);
        return GetDamageBaseOnLevel(skillData, p_forcedLevel);
    }
    public int GetDamageBaseOnLevel(SkillData p_skill, int p_forcedLevel = -1) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
        if (p_forcedLevel == -1) {
            return playerSkillData.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_skill.currentLevel);
        } else {
            return playerSkillData.skillUpgradeData.GetAdditionalDamageBaseOnLevel(p_forcedLevel);
        }
    }

    public int GetTileRangeBonusPerLevel(PLAYER_SKILL_TYPE p_skillType) {
        SkillData skillData = GetSkillData(p_skillType);
        return GetTileRangeBonusPerLevel(skillData);
    }
    public int GetTileRangeBonusPerLevel(SkillData p_skill) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
        return playerSkillData.skillUpgradeData.GetTileRangeBonusPerLevel(p_skill.currentLevel);
    }

    public float GetAdditionalPiercePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType) {
        SkillData skillData = GetSkillData(p_skillType);
        return GetAdditionalPiercePerLevelBaseOnLevel(skillData);
    }
    public float GetAdditionalPiercePerLevelBaseOnLevel(SkillData p_skill) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
        return playerSkillData.skillUpgradeData.GetAdditionalPiercePerLevelBaseOnLevel(p_skill.currentLevel);
    }

    public float GetChanceBonusPerLevel(PLAYER_SKILL_TYPE p_skillType) {
        SkillData skillData = GetSkillData(p_skillType);
        return GetChanceBonusPerLevel(skillData);
    }
    public float GetChanceBonusPerLevel(SkillData p_skill) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
        return playerSkillData.skillUpgradeData.GetChanceBonusPerLevel(p_skill.currentLevel);
    }

    public float GetAdditionalHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.skillUpgradeData.GetAdditionalHpPercentagePerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.skillUpgradeData.GetAdditionalMaxHpPercentagePerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAdditionalAttackPercentagePerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.skillUpgradeData.GetAdditionalAttackPercentagePerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
    }

    public int GetAdditionalAttackActualPerLevelBaseOnLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.skillUpgradeData.GetAdditionalAttackActualPerLevelBaseOnLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetIncreaseStatsPercentagePerLevel(PLAYER_SKILL_TYPE p_skillType) {
        return GetIncreaseStatsPercentagePerLevel(GetSkillData(p_skillType));
    }
    public float GetIncreaseStatsPercentagePerLevel(SkillData p_skill) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
        return playerSkillData.skillUpgradeData.GetIncreaseStatsPercentagePerLevel(p_skill.currentLevel);
    }

    public int GetDurationBonusPerLevel(PLAYER_SKILL_TYPE p_skillType, int p_forcedLevel = -1) {
        return GetDurationBonusPerLevel(GetSkillData(p_skillType), p_forcedLevel);
    }
    public int GetDurationBonusPerLevel(SkillData p_skill, int p_forcedLevel = -1) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skill.type);
        if (p_forcedLevel == -1) {
            return playerSkillData.skillUpgradeData.GetDurationBonusPerLevel(p_skill.currentLevel);
        } else {
            return playerSkillData.skillUpgradeData.GetDurationBonusPerLevel(p_forcedLevel);
        }
    }

    public int GetSkillMovementSpeedPerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.skillUpgradeData.GetSkillMovementSpeedPerLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAfflictionPiercePerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetPiercePerLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAfflictionRateChancePerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetRateChancePerLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAfflictionNapsDurationPerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetNapsDurationPerLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAfflictionDurationPerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetDurationPerLevel(GetSkillData(p_skillType).currentLevel);
    }

    public float GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetHungerRatePerLevel(GetSkillData(p_skillType).currentLevel);
    }
    public float GetAfflictionHungerRatePerLevel(PLAYER_SKILL_TYPE p_skillType, int p_level) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetHungerRatePerLevel(p_level);
    }
    public int GetAfflictionCrowdNumberPerLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetCrowdNumberPerLevel(GetSkillData(p_skillType).currentLevel);
    }

    public List<OPINIONS> GetAfflictionOpinionTriggers(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetAllOpinionsTrigger();
    }

    public List<AFFLICTION_SPECIFIC_BEHAVIOUR> GetAfflictionAllAddedBehaviour(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetAllAddedBehaviour();
    }
    public bool HasAfflictionAddedBehaviourForSkillAtCurrentLevel(PLAYER_SKILL_TYPE p_skillType, AFFLICTION_SPECIFIC_BEHAVIOUR p_behaviour) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        SkillData skillData = GetSkillData(p_skillType);
        return playerSkillData.afflictionUpgradeData.HasAddedBehaviourForLevel(p_behaviour, skillData.currentLevel);
    }
    public float GetTriggerRateForCurrentLevel(PLAYER_SKILL_TYPE p_skillType) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        SkillData skillData = GetSkillData(p_skillType);
        return playerSkillData.afflictionUpgradeData.GetRateChancePerLevel(skillData.currentLevel);
    }
    public bool HasOpinionTriggerAtCurrentLevel(PLAYER_SKILL_TYPE p_skillType, OPINIONS p_opinion) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        SkillData skillData = GetSkillData(p_skillType);
        return playerSkillData.afflictionUpgradeData.HasOpinionTriggerForLevel(p_opinion, skillData.currentLevel);
    }
    public void PopulateOpinionTriggersAtCurrentLevel(PLAYER_SKILL_TYPE p_skillType, List<OPINIONS> p_opinions) {
        PlayerSkillData playerSkillData = GetScriptableObjPlayerSkillData<PlayerSkillData>(p_skillType);
        SkillData skillData = GetSkillData(p_skillType);
        for (int i = 0; i < playerSkillData.afflictionUpgradeData.opinionTrigger.Count; i++) {
            if (i <= skillData.currentLevel) {
                OPINIONS currentOpinion = playerSkillData.afflictionUpgradeData.opinionTrigger[i];
                p_opinions.Add(currentOpinion);
            } else {
                break;
            }
        }
    }
    public PLAYER_SKILL_TYPE GetAfflictionTypeByTraitName(string p_traitName) {
        if (afflictionsNameSkillTypeDictionary.ContainsKey(p_traitName)) {
            return afflictionsNameSkillTypeDictionary[p_traitName];
        }
        // Debug.LogWarning($"No affliction skill type with name {p_traitName}");
        return PLAYER_SKILL_TYPE.NONE;
    }
    #endregion
}
