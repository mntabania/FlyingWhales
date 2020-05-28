using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Scriptable_Object_Scripts;
using UnityEngine;

public class PlayerSkillManager : MonoBehaviour {
    public static PlayerSkillManager Instance;

    public PlayerSkillTree[] allSkillTrees;
    public bool unlockAllSkills;

    public Dictionary<SPELL_TYPE, SpellData> allSpellsData;
    public Dictionary<SPELL_TYPE, PlayerAction> allPlayerActionsData;
    public Dictionary<SPELL_TYPE, SpellData> allAfflictionsData;
    public Dictionary<SPELL_TYPE, DemonicStructurePlayerSkill> allDemonicStructureSkillsData;
    public Dictionary<SPELL_TYPE, MinionPlayerSkill> allMinionPlayerSkillsData;
    public Dictionary<SPELL_TYPE, SummonPlayerSkill> allSummonPlayerSkillsData;
    public Dictionary<SPELL_TYPE, SpellData> allPlayerSkillsData;
    [SerializeField] private PlayerSkillTreeAssetsDictionary _playerSkillTreeAssets; 

    private SPELL_TYPE[] allSpells = { SPELL_TYPE.METEOR
            , SPELL_TYPE.TORNADO, SPELL_TYPE.RAVENOUS_SPIRIT, SPELL_TYPE.FEEBLE_SPIRIT, SPELL_TYPE.FORLORN_SPIRIT
            , SPELL_TYPE.LIGHTNING, SPELL_TYPE.POISON_CLOUD, SPELL_TYPE.EARTHQUAKE
            , SPELL_TYPE.SPAWN_BOULDER, SPELL_TYPE.WATER_BOMB, SPELL_TYPE.MANIFEST_FOOD
            , SPELL_TYPE.BRIMSTONES, SPELL_TYPE.SPLASH_POISON, SPELL_TYPE.LOCUST_SWARM, SPELL_TYPE.BLIZZARD, SPELL_TYPE.RAIN
            , SPELL_TYPE.BALL_LIGHTNING, SPELL_TYPE.ELECTRIC_STORM, SPELL_TYPE.FROSTY_FOG, SPELL_TYPE.VAPOR, SPELL_TYPE.FIRE_BALL
            , SPELL_TYPE.POISON_BLOOM, SPELL_TYPE.LANDMINE, SPELL_TYPE.TERRIFYING_HOWL, SPELL_TYPE.FREEZING_TRAP, SPELL_TYPE.SNARE_TRAP, SPELL_TYPE.WIND_BLAST
            , SPELL_TYPE.ICETEROIDS, SPELL_TYPE.HEAT_WAVE, SPELL_TYPE.SPLASH_WATER, SPELL_TYPE.WALL
    };

    private SPELL_TYPE[] allPlayerActions = { SPELL_TYPE.ZAP, SPELL_TYPE.RAISE_DEAD, SPELL_TYPE.DESTROY, SPELL_TYPE.IGNITE, SPELL_TYPE.POISON
            , SPELL_TYPE.TORTURE, SPELL_TYPE.SUMMON_MINION, SPELL_TYPE.STOP, SPELL_TYPE.SEIZE_OBJECT, SPELL_TYPE.SEIZE_CHARACTER, SPELL_TYPE.SEIZE_MONSTER
            /*, SPELL_TYPE.RETURN_TO_PORTAL*/, SPELL_TYPE.DEFEND, SPELL_TYPE.HARASS, SPELL_TYPE.INVADE, SPELL_TYPE.LEARN_SPELL/*, SPELL_TYPE.CHANGE_COMBAT_MODE*/
            , SPELL_TYPE.BUILD_DEMONIC_STRUCTURE, SPELL_TYPE.AFFLICT, SPELL_TYPE.ACTIVATE_TILE_OBJECT, SPELL_TYPE.BREED_MONSTER
            /*, SPELL_TYPE.END_RAID, SPELL_TYPE.END_HARASS, SPELL_TYPE.END_INVADE*/, SPELL_TYPE.INTERFERE, SPELL_TYPE.PLANT_GERM
            , SPELL_TYPE.AGITATE, SPELL_TYPE.KNOCKOUT, SPELL_TYPE.KILL, SPELL_TYPE.HEAL, SPELL_TYPE.ABDUCT, SPELL_TYPE.ANIMATE, SPELL_TYPE.EMPOWER
    };

    private SPELL_TYPE[] allAfflictions = { SPELL_TYPE.CANNIBALISM
            , SPELL_TYPE.LYCANTHROPY, SPELL_TYPE.VAMPIRISM, SPELL_TYPE.KLEPTOMANIA
            , SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.CURSED_OBJECT, SPELL_TYPE.ALCOHOLIC
            , SPELL_TYPE.AGORAPHOBIA, SPELL_TYPE.PARALYSIS, SPELL_TYPE.ZOMBIE_VIRUS
            , SPELL_TYPE.PESTILENCE, SPELL_TYPE.PSYCHOPATHY, SPELL_TYPE.COWARDICE, SPELL_TYPE.PYROPHOBIA
            , SPELL_TYPE.NARCOLEPSY, SPELL_TYPE.HOTHEADED, SPELL_TYPE.LAZINESS, SPELL_TYPE.MUSIC_HATER, SPELL_TYPE.GLUTTONY
    };
    private SPELL_TYPE[] allDemonicStructureSkills = { SPELL_TYPE.THE_GOADER, SPELL_TYPE.THE_EYE, SPELL_TYPE.THE_CRYPT,
        SPELL_TYPE.THE_KENNEL, SPELL_TYPE.THE_SPIRE, SPELL_TYPE.TORTURE_CHAMBER, SPELL_TYPE.DEMONIC_PRISON, SPELL_TYPE.THE_PROFANE
    };

    private SPELL_TYPE[] allMinionPlayerSkills = { SPELL_TYPE.DEMON_WRATH, SPELL_TYPE.DEMON_PRIDE, SPELL_TYPE.DEMON_LUST
        , SPELL_TYPE.DEMON_GLUTTONY, SPELL_TYPE.DEMON_SLOTH, SPELL_TYPE.DEMON_ENVY, SPELL_TYPE.DEMON_GREED,
    };

    private SPELL_TYPE[] allSummonPlayerSkills = { SPELL_TYPE.SKELETON_MARAUDER, SPELL_TYPE.WOLF, SPELL_TYPE.GOLEM, SPELL_TYPE.INCUBUS, SPELL_TYPE.SUCCUBUS, SPELL_TYPE.FIRE_ELEMENTAL, SPELL_TYPE.KOBOLD, SPELL_TYPE.GHOST,
    SPELL_TYPE.ABOMINATION, SPELL_TYPE.MIMIC, SPELL_TYPE.PIG, SPELL_TYPE.CHICKEN, SPELL_TYPE.SHEEP, SPELL_TYPE.SLUDGE,
    SPELL_TYPE.WATER_NYMPH, SPELL_TYPE.WIND_NYMPH, SPELL_TYPE.ICE_NYMPH,
    SPELL_TYPE.ELECTRIC_WISP, SPELL_TYPE.EARTHEN_WISP, SPELL_TYPE.FIRE_WISP,
    SPELL_TYPE.GRASS_ENT, SPELL_TYPE.SNOW_ENT, SPELL_TYPE.CORRUPT_ENT, SPELL_TYPE.DESERT_ENT, SPELL_TYPE.FOREST_ENT,
    SPELL_TYPE.GIANT_SPIDER, SPELL_TYPE.SMALL_SPIDER,
    SPELL_TYPE.SKELETON_ARCHER, SPELL_TYPE.SKELETON_BARBARIAN, SPELL_TYPE.SKELETON_CRAFTSMAN, SPELL_TYPE.SKELETON_DRUID, SPELL_TYPE.SKELETON_HUNTER, SPELL_TYPE.SKELETON_MAGE, SPELL_TYPE.SKELETON_KNIGHT, SPELL_TYPE.SKELETON_MINER, SPELL_TYPE.SKELETON_NOBLE, SPELL_TYPE.SKELETON_PEASANT, SPELL_TYPE.SKELETON_SHAMAN, SPELL_TYPE.SKELETON_STALKER, };

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
        allPlayerSkillsData = new Dictionary<SPELL_TYPE, SpellData>();
        ConstructAllSpellsData();
        ConstructAllPlayerActionsData();
        ConstructAllAfflictionsData();
        ConstructAllDemonicStructureSkillsData();
        ConstructAllMinionPlayerSkillsData();
        ConstructAllSummonPlayerSkillsData();
    }

    #region Utilities
    private void ConstructAllSpellsData() {
        allSpellsData = new Dictionary<SPELL_TYPE, SpellData>();
        for (int i = 0; i < allSpells.Length; i++) {
            SPELL_TYPE spellType = allSpells[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                SpellData spellData = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as SpellData;
                allSpellsData.Add(spellType, spellData);
                allPlayerSkillsData.Add(spellType, spellData);
            }
        }
    }
    private void ConstructAllPlayerActionsData() {
        allPlayerActionsData = new Dictionary<SPELL_TYPE, PlayerAction>();
        for (int i = 0; i < allPlayerActions.Length; i++) {
            SPELL_TYPE spellType = allPlayerActions[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";

                PlayerAction playerAction = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as PlayerAction;
                allPlayerActionsData.Add(spellType, playerAction);
                allPlayerSkillsData.Add(spellType, playerAction);
            }
        }
    }
    private void ConstructAllDemonicStructureSkillsData() {
        allDemonicStructureSkillsData = new Dictionary<SPELL_TYPE, DemonicStructurePlayerSkill>();
        for (int i = 0; i < allDemonicStructureSkills.Length; i++) {
            SPELL_TYPE spellType = allDemonicStructureSkills[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                DemonicStructurePlayerSkill demonicStructureSkill = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as DemonicStructurePlayerSkill;
                allDemonicStructureSkillsData.Add(spellType, demonicStructureSkill);
                allPlayerSkillsData.Add(spellType, demonicStructureSkill);
            }
        }
    }
    private void ConstructAllAfflictionsData() {
        allAfflictionsData = new Dictionary<SPELL_TYPE, SpellData>();
        for (int i = 0; i < allAfflictions.Length; i++) {
            SPELL_TYPE spellType = allAfflictions[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                SpellData affliction = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as SpellData;
                allAfflictionsData.Add(spellType, affliction);
                allPlayerSkillsData.Add(spellType, affliction);
            }
        }
    }
    private void ConstructAllMinionPlayerSkillsData() {
        allMinionPlayerSkillsData = new Dictionary<SPELL_TYPE, MinionPlayerSkill>();
        for (int i = 0; i < allMinionPlayerSkills.Length; i++) {
            SPELL_TYPE spellType = allMinionPlayerSkills[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                MinionPlayerSkill minionPlayerSkill = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as MinionPlayerSkill;
                allMinionPlayerSkillsData.Add(spellType, minionPlayerSkill);
                allPlayerSkillsData.Add(spellType, minionPlayerSkill);
            }
        }
    }
    private void ConstructAllSummonPlayerSkillsData() {
        allSummonPlayerSkillsData = new Dictionary<SPELL_TYPE, SummonPlayerSkill>();
        for (int i = 0; i < allSummonPlayerSkills.Length; i++) {
            SPELL_TYPE spellType = allSummonPlayerSkills[i];
            if (spellType != SPELL_TYPE.NONE) {
                var typeName =
                    $"{UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLettersNoSpace(spellType.ToString())}Data";
                SummonPlayerSkill summonPlayerSkill = System.Activator.CreateInstance(System.Type.GetType(typeName) ??
                   throw new Exception($"Problem with creating spell data for {typeName}")) as SummonPlayerSkill;
                allSummonPlayerSkillsData.Add(spellType, summonPlayerSkill);
                allPlayerSkillsData.Add(spellType, summonPlayerSkill);
            }
        }
    }
    public bool IsSpell(SPELL_TYPE type) {
        return allSpells.Contains(type);
    }
    public bool IsAffliction(SPELL_TYPE type) {
        return allAfflictions.Contains(type);
    }
    public bool IsMinion(SPELL_TYPE type) {
        return allMinionPlayerSkills.Contains(type);
    }
    public bool IsPlayerAction(SPELL_TYPE type) {
        return allPlayerActions.Contains(type);
    }
    public bool IsDemonicStructure(SPELL_TYPE type) {
        return allDemonicStructureSkills.Contains(type);
    }
    public SpellData GetPlayerSkillData(SPELL_TYPE type) {
        if (allPlayerSkillsData.ContainsKey(type)) {
            return allPlayerSkillsData[type];
        }
        return null;
    }
    public SpellData GetSpellData(SPELL_TYPE type) {
        if (allSpellsData.ContainsKey(type)) {
            return allSpellsData[type];
        }
        return null;
    }
    public SpellData GetAfflictionData(SPELL_TYPE type) {
        if (allAfflictionsData.ContainsKey(type)) {
            return allAfflictionsData[type];
        }
        return null;
    }
    public PlayerAction GetPlayerActionData(SPELL_TYPE type) {
        if (allPlayerActionsData.ContainsKey(type)) {
            return allPlayerActionsData[type];
        }
        return null;
    }
    public DemonicStructurePlayerSkill GetDemonicStructureSkillData(SPELL_TYPE type) {
        if (allDemonicStructureSkillsData.ContainsKey(type)) {
            return allDemonicStructureSkillsData[type];
        }
        return null;
    }
    public MinionPlayerSkill GetMinionPlayerSkillData(SPELL_TYPE type) {
        if (allMinionPlayerSkillsData.ContainsKey(type)) {
            return allMinionPlayerSkillsData[type];
        }
        return null;
    }
    public SummonPlayerSkill GetSummonPlayerSkillData(SPELL_TYPE type) {
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
    public PlayerSkillTreeNode GetPlayerSkillTreeNode(SPELL_TYPE skillType) {
        for (int i = 0; i < allSkillTrees.Length; i++) {
            PlayerSkillTree skillTree = allSkillTrees[i];
            if (skillTree.nodes.ContainsKey(skillType)) {
                return skillTree.nodes[skillType];
            }
        }
        return null;
    }
    #endregion

    #region Assets
    public T GetPlayerSkillAsset<T>(SPELL_TYPE spellType) where T : PlayerSkillAssets {
        if (_playerSkillTreeAssets.ContainsKey(spellType)) {
            return _playerSkillTreeAssets[spellType] as T;    
        }
        return null;
    }
    #endregion
}
