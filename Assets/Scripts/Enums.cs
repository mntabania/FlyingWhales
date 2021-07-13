public enum PROGRESSION_SPEED {
    X1,
    X2,
    X4
}
public enum BIOMES{
    GRASSLAND,
    SNOW,
	TUNDRA,
	DESERT,
	FOREST,
	BARE,
	NONE,
    ANCIENT_RUIN,
}
public enum EQUATOR_LINE{
	HORIZONTAL,
	VERTICAL,
	DIAGONAL_LEFT,
	DIAGONAL_RIGHT,
}
public enum ELEVATION{
    PLAIN,
    MOUNTAIN,
	WATER,
    TREES,
}
public enum RACE{
    NONE = 0,
	HUMANS = 1,
	ELVES = 2,
	MINGONS = 3,
	CROMADS = 4,
	UNDEAD = 5,
    GOBLIN = 6,
	TROLL = 7,
	DRAGON = 8,
	DEHKBRUG = 9,
    WOLF = 10,
    SLIME = 11,
    BEAST = 12,
    SKELETON = 13,
    DEMON = 14,
    FAERY = 15,
    INSECT = 16,
    SPIDER = 17,
    GOLEM = 18,
    ELEMENTAL = 19,
    KOBOLD = 20,
    MIMIC = 21,
    ENT = 22,
    ABOMINATION = 23,
    PIG = 24,
    SHEEP = 25,
    CHICKEN = 26,
    LESSER_DEMON = 27,
    NYMPH = 28,
    WISP = 29,
    SLUDGE = 30,
    GHOST = 31,
    ANGEL = 32,
    WURM = 33,
    REVENANT = 34,
    RAT = 35,
    RATMAN = 36,
    SCORPION = 37, 
    HARPY = 38,
    TRITON = 39,
    IMP = 40,
    BEAR = 45,
    BOAR = 46,
    MOONWALKER = 47,
    MINK = 48,
    RABBIT = 49,
}
public enum HEXTILE_DIRECTION {
    NORTH_WEST,
    WEST,
    SOUTH_WEST,
    SOUTH_EAST,
    EAST,
    NORTH_EAST,
    NONE
}
public enum PATHFINDING_MODE{
	NORMAL,
    UNRESTRICTED,
    PASSABLE,
}
public enum GRID_PATHFINDING_MODE {
    NORMAL,
    UNCONSTRAINED,
    CAVE_INTERCONNECTION
}

public enum GENDER{
	MALE,
	FEMALE,
}

public enum PRONOUN_TYPE {
    SUBJECTIVE,
    OBJECTIVE,
    POSSESSIVE,
    REFLEXIVE,
}
public enum MONTH{
	NONE,
	JAN,
	FEB,
	MAR,
	APR,
	MAY,
	JUN,
	JUL,
	AUG,
	SEP,
	OCT,
	NOV,
	DEC,
}
public enum LANGUAGES{
	NONE,
	ENGLISH,
}
public enum DIRECTION{
	LEFT,
	RIGHT,
	UP,
	DOWN,
}
public enum LOG_IDENTIFIER{
	NONE,
	ACTIVE_CHARACTER,
	FACTION_1,
	FACTION_LEADER_1,
    //KING_1_SPOUSE,
    LANDMARK_1,
    PARTY_1,
    //RANDOM_CITY_1,
    //RANDOM_GOVERNOR_1,
    TARGET_CHARACTER,
	FACTION_2,
	FACTION_LEADER_2,
	//KING_2_SPOUSE,
	LANDMARK_2,
    PARTY_2,
    //RANDOM_CITY_2,
    //RANDOM_GOVERNOR_2,
    CHARACTER_3,
	FACTION_3,
	FACTION_LEADER_3,
	//KING_3_SPOUSE,
	LANDMARK_3,
    PARTY_3,
    //RANDOM_CITY_3,
    //RANDOM_GOVERNOR_3,
    ACTION_DESCRIPTION,
    QUEST_NAME,
	ACTIVE_CHARACTER_PRONOUN_S,
	ACTIVE_CHARACTER_PRONOUN_O,
	ACTIVE_CHARACTER_PRONOUN_P,
	ACTIVE_CHARACTER_PRONOUN_R,
	FACTION_LEADER_1_PRONOUN_S,
	FACTION_LEADER_1_PRONOUN_O,
	FACTION_LEADER_1_PRONOUN_P,
	FACTION_LEADER_1_PRONOUN_R,
	FACTION_LEADER_2_PRONOUN_S,
	FACTION_LEADER_2_PRONOUN_O,
	FACTION_LEADER_2_PRONOUN_P,
	FACTION_LEADER_2_PRONOUN_R,
	TARGET_CHARACTER_PRONOUN_S,
	TARGET_CHARACTER_PRONOUN_O,
	TARGET_CHARACTER_PRONOUN_P,
	TARGET_CHARACTER_PRONOUN_R,
    MINION_1_PRONOUN_S,
    MINION_1_PRONOUN_O,
    MINION_1_PRONOUN_P,
    MINION_1_PRONOUN_R,
    MINION_2_PRONOUN_S,
    MINION_2_PRONOUN_O,
    MINION_2_PRONOUN_P,
    MINION_2_PRONOUN_R,
    //SECESSION_CITIES,
    TASK,
	DATE,
	FACTION_LEADER_3_PRONOUN_S,
	FACTION_LEADER_3_PRONOUN_O,
	FACTION_LEADER_3_PRONOUN_P,
	FACTION_LEADER_3_PRONOUN_R,
    ITEM_1,
    ITEM_2,
    ITEM_3,
    COMBAT_FILLER,
    //PARTY_NAME,
    OTHER,
    STRING_1,
    STRING_2,
    MINION_1,
    MINION_2,
    CHARACTER_LIST_1,
    CHARACTER_LIST_2,
    STRUCTURE_1,
    STRUCTURE_2,
    STRUCTURE_3,
    APPEND,
    OTHER_2,
}
public enum LANDMARK_TYPE {
    NONE = 0,
    THE_PORTAL = 1,
    WORKSHOP = 4,
    ABANDONED_MINE = 8,
    FARM = 17,
    VILLAGE = 20,
    BANDIT_CAMP = 24,
    MAGE_TOWER = 25,
    ANCIENT_RUIN = 30,
    CAVE = 31,
    BARRACKS = 34,
    MONSTER_LAIR = 42,
    OSTRACIZER = 48,
    CRYPT = 49,
    KENNEL = 50,
    THE_ANVIL = 51,
    MEDDLER = 52,
    EYE = 53,
    DEFILER = 54,
    THE_NEEDLES = 55,
    THE_PIT = 56,
    LUMBERYARD = 57,
    QUARRY = 58,
    HOUSES = 59,
    TORTURE_CHAMBERS = 60,
    DEMONIC_PRISON = 61,
    MINE = 62,
    ANCIENT_GRAVEYARD = 63,
    TEMPLE = 64,
    RUINED_ZOO = 65,
    BIOLAB = 66,
    SPIRE = 67,
    MANA_PIT = 68,
    MARAUD = 69,
    DEFENSE_POINT = 70,
}
public enum CHARACTER_ROLE {
    NONE,
    CIVILIAN,
    PLAYER,
    BANDIT,
    LEADER,
    BEAST,
    NOBLE,
    SOLDIER,
    ADVENTURER,
    MINION,
}
public enum FACTION_RELATIONSHIP_STATUS {
    Friendly,
    Hostile,
    Neutral,
}
public enum ATTACK_TYPE {
    PHYSICAL,
    MAGICAL,
}
public enum RANGE_TYPE {
    MELEE,
    RANGED,
}
public enum ACTION_CATEGORY {
    DIRECT,
    INDIRECT,
    CONSUME,
    VERBAL,
}

public enum LOCATION_TYPE {
    VILLAGE,
    DEMONIC_INTRUSION,
    DUNGEON,
    EMPTY,
}
public enum STAT {
    HP,
    ATTACK,
    SPEED,
    POWER,
    ALL,
}
public enum DAMAGE_IDENTIFIER {
    DEALT,
    RECEIVED,
}
public enum TRAIT_REQUIREMENT {
    RACE,
    TRAIT,
    ADJACENT_ALLIES,
    FRONTLINE,
    ONLY_1_TARGET,
    EVERY_MISSING_HP_25PCT,
    MELEE,
    RANGED,
    OPPOSITE_SEX,
    ONLY_DEMON,
    ROLE,
}

public enum FACTION_TYPE {
    Elven_Kingdom,
    Human_Empire,
    Demons,
    Vagrants,
    Wild_Monsters,
    Bandits,
    Undead,
    Disguised,
    Vampire_Clan,
    Lycan_Clan,
    Demon_Cult,
    Ratmen
}

public enum INTERACTION_TYPE {
    NONE = 0,
    RETURN_HOME = 1,
    DROP_ITEM = 2,
    PICK_UP = 3,
    RELEASE_CHARACTER = 4,
    MINE_METAL = 5,
    ASK_FOR_HELP_SAVE_CHARACTER = 6,
    ASSAULT = 7,
    TRANSFORM_TO_WOLF_FORM = 8,
    REVERT_TO_NORMAL_FORM = 9,
    SLEEP = 10,
    EAT = 12,
    DAYDREAM = 13,
    PLAY_GUITAR = 14,
    DRINK = 16,
    SLEEP_OUTSIDE = 17,
    REMOVE_POISON = 18,
    POISON = 19,
    PRAY = 20,
    CHOP_WOOD = 21,
    STEAL = 22,
    SCRAP = 23,
    DEPOSIT_RESOURCE_PILE = 26,
    RETURN_HOME_LOCATION = 27,
    PLAY = 28,
    RESTRAIN_CHARACTER = 29,
    FIRST_AID_CHARACTER = 30,
    CURE_CHARACTER = 31,
    JUDGE_CHARACTER = 34,
    FEED = 35,
    ASK_FOR_HELP_REMOVE_POISON_TABLE = 36,
    SIT = 37,
    STAND = 38,
    NAP = 39,
    BURY_CHARACTER = 40,
    REMEMBER_FALLEN = 42,
    SPIT = 43,
    MAKE_LOVE = 45,
    INVITE = 46,
    DRINK_BLOOD = 47,
    REPLACE_TILE_OBJECT = 48,
    TANTRUM = 50,
    BREAK_UP = 51,
    SHARE_INFORMATION = 52,
    WATCH = 53,
    INSPECT = 54,
    PUKE = 55,
    SEPTIC_SHOCK = 56,
    ZOMBIE_DEATH = 57,
    CARRY = 58,
    DROP = 59,
    KNOCKOUT_CHARACTER = 60,
    RITUAL_KILLING = 61,
    RESOLVE_CONFLICT = 62,
    STUMBLE = 64,
    ACCIDENT = 65,
    TAKE_RESOURCE = 66,
    DROP_RESOURCE = 67,
    BUTCHER = 68,
    ASK_TO_STOP_JOB = 69,
    WELL_JUMP = 70,
    STRANGLE = 71,
    REPAIR = 72,
    NARCOLEPTIC_NAP = 73,
    CRY = 74,
    CRAFT_TILE_OBJECT = 75,
    PRAY_TILE_OBJECT = 76,
    HAVE_AFFAIR = 77,
    SLAY_CHARACTER = 78,
    LAUGH_AT = 79,
    FEELING_CONCERNED = 80,
    TEASE = 81,
    FEELING_SPOOKED = 82,
    FEELING_BROKENHEARTED = 83,
    GRIEVING = 84,
    GO_TO = 85,
    SING = 86,
    DANCE = 87,
    SCREAM_FOR_HELP = 88,
    REACT_TO_SCREAM = 89,
    RESOLVE_COMBAT = 90,
    CHANGE_CLASS = 91,
    VISIT = 92,
    PLACE_BLUEPRINT = 93,
    BUILD_BLUEPRINT = 94,
    STEALTH_TRANSFORM = 95,
    HARVEST_PLANT = 96,
    REPAIR_STRUCTURE = 97,
    NEUTRALIZE = 113,
    MINE_STONE = 114,
    ROAM = 115,
    STUDY_MONSTER = 116,
    DESTROY_RESOURCE_AMOUNT = 117,
    STAND_STILL = 118,
    CREATE_HEALING_POTION = 119,
    CREATE_ANTIDOTE = 120,
    CREATE_POISON_FLASK = 121,
    EXTRACT_ITEM = 122,
    REMOVE_FREEZING = 123,
    BOOBY_TRAP = 124,
    REPORT_CORRUPTED_STRUCTURE = 125,
    FISH = 126,
    REMOVE_UNCONSCIOUS = 127,
    DOUSE_FIRE = 128,
    ATTACK_DEMONIC_STRUCTURE = 129,
    HEAL_SELF = 130,
    OPEN = 131,
    EXILE = 132,
    WHIP = 133,
    EXECUTE = 134,
    ABSOLVE = 135,
    TEND = 136,
    START_TEND = 137,
    START_DOUSE = 138,
    START_CLEANSE = 139,
    CLEANSE_TILE = 140,
    CLEAN_UP = 141,
    START_DRY = 142,
    PATROL = 143,
    START_PATROL = 144,
    MINE = 145,
    DIG = 146,
    BUILD_LAIR = 147,
    ABSORB_LIFE = 148,
    SPAWN_SKELETON = 149,
    RAISE_CORPSE = 150,
    PLACE_FREEZING_TRAP = 151,
    EAT_CORPSE = 152,
    BEGIN_MINE = 153,
    ABSORB_POWER = 154,
    READ_NECRONOMICON = 155,
    MEDITATE = 156,
    REGAIN_ENERGY = 157,
    MURDER = 158,
    REMOVE_RESTRAINED = 159,
    EAT_ALIVE = 160,
    REMOVE_BUFF = 161,
    CREATE_CULTIST_KIT = 162,
    IS_CULTIST = 163,
    SPAWN_POISON_CLOUD = 164,
    DECREASE_MOOD = 165,
    GO_TO_TILE = 166,
    DISABLE = 167,
    BURN = 168,
    LAY_EGG = 169,
    TAKE_SHELTER = 170,
    IS_PLAGUED = 171,
    DARK_RITUAL = 172,
    DRAW_MAGIC_CIRCLE = 173,
    CULTIST_TRANSFORM = 174,
    JOIN_GATHERING = 175,
    EXPLORE = 176,
    EXTERMINATE = 177,
    RESCUE = 178,
    COUNTERATTACK_ACTION = 179,
    MONSTER_INVADE = 180,
    DISGUISE = 181,
    RECRUIT = 182,
    RAID = 183,
    COOK = 184,
    BUILD_TROLL_CAULDRON = 185,
    FLEE_CRIME = 186,
    HOST_SOCIAL_PARTY = 187,
    PLAY_CARDS = 188,
    BUILD_WOLF_LAIR = 189,
    BUILD_CAMPFIRE = 190,
    WARM_UP = 191,
    REPORT_CRIME = 192,
    TRESPASSING = 193,
    EVANGELIZE = 194,
    //FLEE_TO_TILE = 195,
    HUNT_HEIRLOOM = 196,
    REMOVE_TRAP = 197,
    REMOVE_ENSNARED = 198,
    VAMPIRIC_EMBRACE = 199,
    BUILD_VAMPIRE_CASTLE = 200,
    BURN_AT_STAKE = 201,
    IS_VAMPIRE = 202,
    FEED_SELF = 203,
    CARRY_RESTRAINED = 204,
    DROP_RESTRAINED = 205,
    BUILD_NEW_VILLAGE = 206,
    IS_WEREWOLF = 207,
    DISPEL = 208,
    DROP_CORPSE = 209,
    CARRY_CORPSE = 210,
    SUMMON_BONE_GOLEM = 211,
    BIRTH_RATMAN = 212,
    TORTURE = 213,
    QUARANTINE = 214,
    CARRY_PATIENT = 215,
    START_PLAGUE_CARE = 216,
    CARE = 217,
    GO_TO_SPECIFIC_TILE = 218,
    LONG_STAND_STILL = 219,
    BURROW = 220,
    PLAGUE_FATALITY = 221,
    PICKPOCKET = 222,
    DISPOSE_FOOD = 223,
    IS_IMPRISONED = 224,
    STEAL_ANYTHING = 225,
    ABSORB_POWER_CRYSTAL = 226,
    STEAL_COINS = 227,
    MINE_ORE = 228,
    FIND_FISH = 229,
    TILL_TILE = 230,
    SHEAR_ANIMAL = 231,
    SKIN_ANIMAL = 232,
    HARVEST_CROPS = 233,
    CRAFT_EQUIPMENT = 234,
    RECUPERATE = 235,
    HEALER_CURE = 236,
    GATHER_HERB = 237,
    CREATE_HOSPICE_POTION = 238,
    CREATE_HOSPICE_ANTIDOTE = 239,
    STOCKPILE_FOOD,
    BUY_FOOD,
    BUY_WOOD,
    CRAFT_FURNITURE_WOOD,
    CRAFT_FURNITURE_STONE,
    BUY_STONE,
    BUY_ITEM,
    DROP_RESOURCE_TO_WORK_STRUCTURE,
    DRINK_WATER
}
public enum INTERRUPT {
    None,
    Accident,
    Break_Up,
    Chat,
    Cowering,
    Create_Faction,
    Feeling_Brokenhearted,
    Feeling_Concerned,
    Feeling_Embarassed,
    Feeling_Spooked,
    Flirt,
    Grieving,
    Join_Faction,
    Laugh_At,
    Leave_Faction,
    Mock,
    Narcoleptic_Nap,
    Puke,
    Reduce_Conflict,
    Septic_Shock,
    Set_Home,
    Shocked,
    Stopped,
    Stumble,
    Watch,
    Zombie_Death,
    Become_Settlement_Ruler,
    Become_Faction_Leader,
    Transform_To_Wolf,
    Revert_To_Normal,
    Angered,
    Inspired,
    Feeling_Lazy,
    Invite_To_Make_Love,
    Plagued,
    Ingested_Poison,
    Mental_Break,
    Being_Tortured,
    Loss_Of_Control,
    Feared,
    Abomination_Death,
    Cry,
    Surprised,
    Necromantic_Transformation,
    Set_Lair,
    Order_Attack,
    Recall_Attack,
    Worried,
    Being_Brainwashed,
    Cry_Request,
    Declare_Raid,
    Feeling_Angry,
    Heatstroke_Death,
    Seizure,
    Wary,
    Panicking,
    Create_Party,
    Join_Party,
    Leave_Party,
    Morale_Boost,
    Noise_Wake_Up,
    Transform_To_Bat,
    Revert_From_Bat,
    Become_Vampire_Lord,
    Burning_At_Stake,
    Become_Lycanthrope,
    Transform_To_Werewolf,
    Revert_From_Werewolf,
    Shed_Pelt,
    Become_Cult_Leader,
    Evaluate_Cultist_Affiliation,
    Heart_Attack,
    Stroke,
    Total_Organ_Failure,
    Sneeze,
    Pneumonia,
    Set_Home_Ratman,
    Resign,
    Leave_Home,
    Leave_Village,
    Declare_War,
    Pulled_Down,
    Taunted,
    Pass_Out,
    Narcoleptic_Nap_Short,
    Narcoleptic_Nap_Medium,
    Narcoleptic_Nap_Long,
    Claim_Work_Structure,
    Buy_Home
}

public enum TRAIT_TYPE {
    STATUS,
    BUFF,
    FLAW,
    NEUTRAL,
}
public enum TRAIT_EFFECT {
    NEUTRAL,
    POSITIVE,
    NEGATIVE,
}
public enum TRAIT_TRIGGER {
    OUTSIDE_COMBAT,
    START_OF_COMBAT,
    DURING_COMBAT,
}
public enum TRAIT_REQUIREMENT_SEPARATOR {
    OR,
    AND,
}
public enum TRAIT_REQUIREMENT_TARGET {
    SELF,
    ENEMY, //TARGET
    OTHER_PARTY_MEMBERS,
    ALL_PARTY_MEMBERS,
    ALL_IN_COMBAT,
    ALL_ENEMIES,
}
public enum TRAIT_REQUIREMENT_CHECKER {
    SELF,
    ENEMY, //TARGET
    OTHER_PARTY_MEMBERS,
    ALL_PARTY_MEMBERS,
    ALL_IN_COMBAT,
    ALL_ENEMIES,
}
public enum SPELL_TARGET {
    NONE,
    CHARACTER,
    TILE_OBJECT,
    TILE,
    AREA,
    STRUCTURE,
    ROOM,
    SETTLEMENT,
}
public enum STRUCTURE_TYPE {
    TAVERN = 1,
    WAREHOUSE = 2,
    DWELLING = 3,
    WILDERNESS = 5,
    CEMETERY = 8,
    PRISON = 9,
    CITY_CENTER = 11,
    BARRACKS = 13,
    HOSPICE = 14,
    HUNTER_LODGE = 19,
    MAGE_QUARTERS = 20,
    NONE = 21,
    MONSTER_LAIR = 22,
    ABANDONED_MINE = 23,
    ANCIENT_RUIN = 24,
    MAGE_TOWER = 25,
    THE_PORTAL = 26,
    CAVE = 27,
    OCEAN = 28,
    OSTRACIZER = 29,
    KENNEL = 30,
    CRYPT = 31,
    MEDDLER = 32,
    DEFILER = 33,
    THE_ANVIL = 34,
    WATCHER = 35,
    THE_NEEDLES = 36,
    TORTURE_CHAMBERS = 37,
    FARM = 39,
    LUMBERYARD = 40,
    MINE = 41,
    ANCIENT_GRAVEYARD = 42,
    TEMPLE = 43,
    RUINED_ZOO = 44,
    VAMPIRE_CASTLE = 45,
    CULT_TEMPLE = 46,
    BIOLAB = 47,
    QUARRY = 48,
    WORKSHOP = 49,
    TAILORING = 50,
    TANNERY = 51,
    FISHERY = 52,
    SPIRE = 53,
    MANA_PIT = 54,
    MARAUD = 55,
    DEFENSE_POINT = 56,
    IMP_HUT = 57,
    BUTCHERS_SHOP = 58,
    BOAR_DEN = 59,
    WOLF_DEN = 60,
    BEAR_DEN = 61,
    RABBIT_HOLE = 62,
    MINK_HOLE = 63,
    MOONCRAWLER_HOLE = 64,
}
public enum RELATIONSHIP_TYPE {
    NONE = 0,
    RELATIVE = 3,
    LOVER = 4,
    AFFAIR = 5,
    EX_LOVER = 10,
    SIBLING = 11,
    PARENT = 12,
    CHILD = 13,
}

public enum POINT_OF_INTEREST_TYPE {
    //ITEM,
    CHARACTER,
    TILE_OBJECT,
}
public enum TILE_OBJECT_TYPE {
    WOOD_PILE = 0,
    BERRY_SHRUB = 3,
    GUITAR = 4,
    MAGIC_CIRCLE = 5,
    TABLE = 6,
    BED = 7,
    ORE = 8,
    SMALL_TREE_OBJECT = 9,
    DESK = 11,
    TOMBSTONE = 12,
    NONE = 13,
    MUSHROOM = 14,
    NECRONOMICON = 15,
    CHAOS_ORB = 16,
    HERMES_STATUE = 17,
    ANKH_OF_ANUBIS = 18,
    MIASMA_EMITTER = 19,
    WATER_WELL = 20,
    GENERIC_TILE_OBJECT = 21,
    ANIMAL_MEAT = 22,
    GODDESS_STATUE = 23,
    STRUCTURE_TILE_OBJECT = 24,
    STONE_PILE = 25,
    // METAL_PILE = 26,
    TORNADO = 27,
    BANDAGES = 29,
    TABLE_MEDICINE = 30,
    ANVIL = 31,
    ARCHERY_TARGET = 32,
    WATER_BASIN = 33,
    BRAZIER = 34,
    CAMPFIRE = 35,
    CANDELABRA = 36,
    CAULDRON = 37,
    FOOD_BASKETS = 38,
    PLINTH_BOOK = 39,
    RACK_FARMING_TOOLS = 40,
    RACK_STAVES = 41,
    RACK_TOOLS = 42,
    RACK_WEAPONS = 43,
    SHELF_ARMOR = 44,
    SHELF_BOOKS = 45,
    SHELF_SCROLLS = 46,
    SHELF_SWORDS = 47,
    SMITHING_FORGE = 48,
    STUMP = 49,
    TABLE_ALCHEMY = 50,
    TABLE_ARMOR = 51,
    TABLE_CONJURING = 52,
    TABLE_HERBALISM = 53,
    TABLE_METALWORKING_TOOLS = 54,
    TABLE_SCROLLS = 55,
    TABLE_WEAPONS = 56,
    TEMPLE_ALTAR = 57,
    TORCH = 58,
    TRAINING_DUMMY = 59,
    WHEELBARROW = 60,
    BED_CLINIC = 61,
    BARREL = 62,
    CRATE = 63,
    FIREPLACE = 64,
    RUG = 65,
    CHAINS = 66,
    SHELF = 67,
    STATUE = 68,
    GRAVE = 69,
    PLANT = 70,
    TRASH = 71,
    ROCK = 72,
    FLOWER = 73,
    KINDLING = 74,
    BIG_TREE_OBJECT = 75,
    HEALING_POTION = 76,
    TOOL = 77,
    WATER_FLASK = 78,
    IRON_MAIDEN = 79,
    ARTIFACT = 80,
    BLOCK_WALL = 81,
    RAVENOUS_SPIRIT = 82,
    FEEBLE_SPIRIT = 83,
    FORLORN_SPIRIT = 84,
    POISON_CLOUD = 85,
    LOCUST_SWARM = 86,
    TREASURE_CHEST = 87,
    SAWHORSE = 88,
    OBELISK = 89,
    BALL_LIGHTNING = 90,
    FROSTY_FOG = 91,
    CORN_CROP = 92,
    CAGE = 93,
    VAPOR = 94,
    FIRE_BALL = 95,
    HERB_PLANT = 96,
    POISON_FLASK = 97,
    EMBER = 98,
    ANTIDOTE = 99,
    FIRE_CRYSTAL = 100,
    ICE_CRYSTAL = 101,
    WATER_CRYSTAL = 102,
    POISON_CRYSTAL = 103,
    ELECTRIC_CRYSTAL = 104,
    QUICKSAND = 105,
    SNOW_MOUND = 106,
    ICE = 107,
    TORTURE_TABLE = 108,
    MANACLES = 109,
    CANDLES = 110,
    JARS = 111,
    FEEDING_TROUGH = 112,
    SKULLS = 113,
    DEMON_ALTAR = 114,
    SIGIL = 115,
    SKULL_LAMP = 116,
    CRYPT_CHEST = 117,
    GRIMOIRE = 118,
    EYEBALL = 119,
    BLOOD_POOL = 120,
    PEW = 121,
    CORRUPTED_SPIKE = 122,
    DEMON_CIRCLE = 123,
    SPAWNING_PIT = 124,
    BIG_SPAWNING_PIT = 125,
    CARPET = 126,
    BONE_ROWS = 127,
    BONES = 128,
    CORRUPTED_PIT = 129,
    CORRUPTED_TENDRIL = 130,
    PLINTH_ORB = 131,
    MANA_RUNE = 132,
    GROUND_BOLT = 133,
    DEMON_RACK = 134,
    PORTAL_TILE_OBJECT = 135,
    WINTER_ROSE = 136,
    DESERT_ROSE = 137,
    MIMIC_TILE_OBJECT = 138,
    DOOR_TILE_OBJECT = 139,
    POISON_VENT = 140,
    VAPOR_VENT = 141,
    ELF_MEAT = 142,
    HUMAN_MEAT = 143,
    FISH_PILE = 145,
    DIAMOND = 146,
    GOLD = 147,
    CULTIST_KIT = 148,
    SPIDER_EGG = 149,
    REPTILE_EGG = 150,
    GOOSE_EGG = 151,
    EXCALIBUR = 152,
    PLINTH = 153,
    SARCOPHAGUS = 154,
    TROLL_CAULDRON = 155,
    WURM_HOLE = 156,
    HEIRLOOM = 157,
    WEREWOLF_PELT = 158,
    PHYLACTERY = 159,
    CULT_CIRCLE = 160,
    CULT_ALTAR = 161,
    CULT_CROSS = 162,
    PROFESSION_PEDESTAL = 163,
    ORE_VEIN = 164,
    FISHING_SPOT = 165,
    HARPY_EGG = 166,
    RAT_MEAT = 167,
    THIN_WALL = 168,
    SPIRE_TILE_OBJECT = 169,
    DEMON_EYE = 170,
    MANA_PIT_TILE_OBJECT = 171,
    MARAUD_TILE_OBJECT = 172,
    DEFENSE_POINT_TILE_OBJECT = 173,
    WATCHER_TILE_OBJECT = 174,
    BIOLAB_TILE_OBJECT = 175,
    IMP_HUT_TILE_OBJECT = 176,
    MEDDLER_TILE_OBJECT = 177,
    CRYPT_TILE_OBJECT = 178,
    DEFILER_TILE_OBJECT = 179,
    TORTURE_CHAMBERS_TILE_OBJECT = 180,
    DEMONIC_STRUCTURE_BLOCKER_TILE_OBJECT = 181,
    KENNEL_TILE_OBJECT = 182,
    COPPER_SWORD = 183,
    IRON_SWORD = 184,
    RING = 185,
    BRACER = 186,
    FUR_SHIRT = 187,
    MINK_SHIRT = 188,
    POWER_CRYSTAL = 189,
    MITHRIL_SWORD = 190,
    ORICHALCUM_SWORD = 191,
    COPPER_AXE = 192,
    IRON_AXE = 193,
    MITHRIL_AXE = 194,
    ORICHALCUM_AXE = 195,
    COPPER_BOW = 196,
    IRON_BOW = 197,
    MITHRIL_BOW = 198,
    ORICHALCUM_BOW = 199,
    COPPER_STAFF = 200,
    IRON_STAFF = 201,
    MITHRIL_STAFF = 202,
    ORICHALCUM_STAFF = 203,
    COPPER_DAGGER = 204,
    IRON_DAGGER = 205,
    MITHRIL_DAGGER = 206,
    ORICHALCUM_DAGGER = 207,
    RABBIT_SHIRT = 208,
    WOOL_SHIRT = 209,
    SPIDER_SILK_SHIRT = 210,
    MOONWALKER_SHIRT = 211,
    BOAR_HIDE_ARMOR = 212,
    WOLF_HIDE_ARMOR = 213,
    BEAR_HIDE_ARMOR = 214,
    SCALE_ARMOR = 215,
    DRAGON_ARMOR = 216,
    COPPER_ARMOR = 217,
    IRON_ARMOR = 218,
    MITHRIL_ARMOR = 219,
    ORICHALCUM_ARMOR = 220,
    NECKLACE = 221,
    BELT = 222,
    SCROLL = 223,
    MITHRIL = 224,
    ORICHALCUM = 225,
    IRON = 226,
    COPPER = 227,
    CORN = 228,
    POTATO = 229,
    PINEAPPLE = 230,
    ICEBERRY = 231,
    HYPNO_HERB = 232,
    WOOL = 233,
    MINK_CLOTH = 234,
    MOONCRAWLER_CLOTH = 235,
    BOAR_HIDE = 236,
    WOLF_HIDE = 237,
    BEAR_HIDE = 238,
    DRAGON_HIDE = 239,
    RABBIT_CLOTH = 240,
    SCALE_HIDE = 241,
    SPIDER_SILK = 242,
    MOON_THREAD = 243,
    HYPNO_HERB_CROP = 244,
    ICEBERRY_CROP = 245,
    PINEAPPLE_CROP = 246,
    POTATO_CROP = 247,
    VEGETABLES = 248,
    COUNTER_TOP = 249,
    BOAR_DEN = 250,
    WOLF_DEN = 251,
    BEAR_DEN = 252,
    RABBIT_HOLE = 253,
    SHEEP_SPAWNING_SPOT = 254,
    MINK_HOLE = 255,
    MOONCRAWLER_HOLE = 256,
    BASIC_SWORD = 257,
    BASIC_AXE = 258,
    BASIC_DAGGER = 259,
    BASIC_STAFF = 260,
    BASIC_SHIRT = 261,
    BASIC_BOW = 262,
}
public enum POI_STATE {
    ACTIVE,
    INACTIVE,
}

public enum TARGET_POI { ACTOR, TARGET, }
public enum GridNeighbourDirection { North, South, West, East, North_West, North_East, South_West, South_East }
public enum TIME_IN_WORDS { AFTER_MIDNIGHT, MORNING, AFTERNOON, EARLY_NIGHT, LATE_NIGHT, LUNCH_TIME, NONE }
public enum GOAP_EFFECT_CONDITION { NONE, REMOVE_TRAIT, HAS_TRAIT, FULLNESS_RECOVERY, TIREDNESS_RECOVERY, HAPPINESS_RECOVERY, STAMINA_RECOVERY, CANNOT_MOVE, REMOVE_FROM_PARTY, DESTROY, DEATH, PATROL, EXPLORE, REMOVE_ITEM, HAS_TRAIT_EFFECT, HAS_PLAN
        , TARGET_REMOVE_RELATIONSHIP, TARGET_STOP_ACTION_AND_JOB, RESTRAIN_CARRY, REMOVE_FROM_PARTY_NO_CONSENT, IN_VISION, REDUCE_HP, INVITED, MAKE_NOISE, STARTS_COMBAT, CHANGE_CLASS
        , PRODUCE_FOOD, PRODUCE_WOOD, PRODUCE_STONE, PRODUCE_METAL, DEPOSIT_RESOURCE, REMOVE_REGION_CORRUPTION, CLEAR_REGION_FACTION_OWNER, REGION_OWNED_BY_ACTOR_FACTION, FACTION_QUEST_DURATION_INCREASE
        , FACTION_QUEST_DURATION_DECREASE, DESTROY_REGION_LANDMARK, CHARACTER_TO_MINION, SEARCH
        , HAS_POI, TAKE_POI //The process of "take" in this manner is different from simply carrying the poi. In technicality, since the actor will only get an amount from the poi target, the actor will not carry the whole poi instead he/she will create a new poi with the amount that he/she needs while simultaneously reducing that amount from the poi target
        , ABSORB_LIFE, RAISE_CORPSE, SUMMON, CARRIED_PATIENT, PRODUCE_CLOTH, BUY_OBJECT, FEED,
}
public enum GOAP_EFFECT_TARGET { ACTOR, TARGET, }
public enum GOAP_PLAN_STATE { IN_PROGRESS, SUCCESS, FAILED, CANCELLED, }
public enum GOAP_PLANNING_STATUS { NONE, RUNNING, PROCESSING_RESULT }

public enum JOB_TYPE { NONE, UNDERMINE, ENERGY_RECOVERY_URGENT, FULLNESS_RECOVERY_URGENT, ENERGY_RECOVERY_NORMAL, FULLNESS_RECOVERY_NORMAL, HAPPINESS_RECOVERY, REMOVE_STATUS, RESTRAIN
        , PRODUCE_WOOD, PRODUCE_FOOD, PRODUCE_STONE, PRODUCE_METAL, FEED, KNOCKOUT, APPREHEND, BURY, CRAFT_OBJECT, JUDGE_PRISONER
        , PATROL, OBTAIN_PERSONAL_ITEM, MOVE_CHARACTER, RITUAL_KILLING, INSPECT, DOUSE_FIRE, COMMIT_SUICIDE, SEDUCE, REPAIR
        , DESTROY, TRIGGER_FLAW, CORRUPT_CULTIST, CORRUPT_CULTIST_SABOTAGE_FACTION, SCREAM, CLEANSE_CORRUPTION, CLAIM_REGION
        , BUILD_BLUEPRINT, PLACE_BLUEPRINT, COMBAT, STROLL, HAUL, OBTAIN_PERSONAL_FOOD, NEUTRALIZE_DANGER, FLEE_TO_HOME, BURY_SERIAL_KILLER_VICTIM, DEMON_KILL, GO_TO, CHECK_PARALYZED_FRIEND, VISIT_FRIEND
        , IDLE_RETURN_HOME, IDLE_NAP, IDLE_SIT, IDLE_STAND, IDLE_GO_TO_INN, IDLE, COMBINE_STOCKPILE, ROAM_AROUND_TERRITORY, ROAM_AROUND_CORRUPTION, ROAM_AROUND_PORTAL, ROAM_AROUND_TILE, RETURN_TERRITORY, RETURN_PORTAL
        , STAND, ABDUCT, LEARN_MONSTER, TAKE_ARTIFACT, TAKE_ITEM, HIDE_AT_HOME, STAND_STILL, SUICIDE_FOLLOW
        , DRY_TILES, CLEANSE_TILES, MONSTER_ABDUCT, REPORT_CORRUPTED_STRUCTURE, COUNTERATTACK, RECOVER_HP, POISON_FOOD
        , BRAWL, PLACE_TRAP, SPREAD_RUMOR, CONFIRM_RUMOR, OPEN_CHEST, TEND_FARM, VISIT_DIFFERENT_VILLAGE, BERSERK_ATTACK, MINE, DIG_THROUGH, SPAWN_LAIR, ABSORB_LIFE, ABSORB_POWER
        , SPAWN_SKELETON, RAISE_CORPSE, HUNT_PREY, DROP_ITEM, BERSERK_STROLL, RETURN_HOME_URGENT, SABOTAGE_NEIGHBOUR, SHARE_NEGATIVE_INFO
        , DECREASE_MOOD, DISABLE, MONSTER_EAT, ARSON, SEEK_SHELTER, DARK_RITUAL, CULTIST_TRANSFORM, CULTIST_POISON, CULTIST_BOOBY_TRAP, JOIN_GATHERING, EXPLORE, EXTERMINATE, RESCUE, RELEASE_CHARACTER, COUNTERATTACK_PARTY, MONSTER_BUTCHER
        , ROAM_AROUND_STRUCTURE, MONSTER_INVADE, PARTY_GO_TO, KIDNAP, RECRUIT, RAID, FLEE_CRIME, HOST_SOCIAL_PARTY, PARTYING, CRAFT_MISSING_FURNITURE, FULLNESS_RECOVERY_ON_SIGHT, HOARD, ZOMBIE_STROLL, WARM_UP, NO_PATH_IDLE, REPORT_CRIME
        , PREACH, HUNT_HEIRLOOM, SNATCH, DROP_ITEM_PARTY, GO_TO_WAITING, PRODUCE_FOOD_FOR_CAMP, KIDNAP_RAID, STEAL_RAID, BUILD_CAMP, CAPTURE_CHARACTER, BURY_IN_ACTIVE_PARTY, VAMPIRIC_EMBRACE, BUILD_VAMPIRE_CASTLE, FIND_NEW_VILLAGE
        , IMPRISON_BLOOD_SOURCE, OFFER_BLOOD, CURE_MAGICAL_AFFLICTION, LYCAN_HUNT_PREY, STEAL_CORPSE, SUMMON_BONE_GOLEM, CHANGE_CLASS, QUARANTINE, PLAGUE_CARE, TORTURE, MONSTER_EAT_CORPSE, TRITON_KIDNAP, RETURN_STOLEN_THING
        , DISPOSE_FOOD_PILE, SNATCH_RESTRAIN, KLEPTOMANIAC_STEAL, LAZY_NAP, FIND_AFFAIR, ABSORB_CRYSTAL, MINE_ORE, FIND_FISH, TILL_TILE, SHEAR_ANIMAL, SKIN_ANIMAL, HARVEST_CROPS, CRAFT_EQUIPMENT, CHOP_WOOD, MINE_STONE, RECUPERATE, HEALER_CURE
        , GATHER_HERB, CREATE_HOSPICE_POTION, CREATE_HOSPICE_ANTIDOTE, STOCKPILE_FOOD, BUY_ITEM, VISIT_HOSPICE, HAUL_ANIMAL_CORPSE, VISIT_STRUCTURE, SOCIALIZE, IDLE_CLEAN, RESCUE_MOVE_CHARACTER
        , OBTAIN_WANTED_ITEM, BUY_FOOD_FOR_TAVERN, IDLE_RETURN_HOME_HIGHER,
}

public enum JOB_OWNER { CHARACTER, SETTLEMENT, FACTION, PARTY }

public enum Cardinal_Direction { North, South, East, West }

public enum ACTION_LOCATION_TYPE {
    IN_PLACE,
    NEARBY,
    RANDOM_LOCATION,
    RANDOM_LOCATION_B,
    NEAR_TARGET,
    //ON_TARGET,
    TARGET_IN_VISION,
    OVERRIDE,
    NEAR_OTHER_TARGET,
    UPON_STRUCTURE_ARRIVAL,
}
public enum CHARACTER_STATE_CATEGORY { MAJOR, MINOR,}
//public enum MOVEMENT_MODE { NORMAL, FLEE, ENGAGE }
public enum CHARACTER_STATE { NONE, PATROL, HUNT, STROLL, BERSERKED, STROLL_OUTSIDE, COMBAT, DOUSE_FIRE, FOLLOW,
    DRY_TILES,
    CLEANSE_TILES,
    TEND_FARM
}
public enum CRIME_SEVERITY { Unapplicable, None, Infraction, Misdemeanor, Serious, Heinous, }
public enum CRIME_STATUS { Unpunished, Punished, Exiled, Absolved, Executed, Burned_At_Stake, }
public enum CRIME_TYPE { Unset, None, Infidelity, Disturbances, Rumormongering, Kidnapping, Theft, Assault, Attempted_Murder, Murder, Arson, Demon_Worship, Divine_Worship, Nature_Worship,
    Aberration, Cannibalism, Plagued, Animal_Killing, Vampire, Werewolf, Treason, Trespassing, }
public enum CHARACTER_MOOD {
    DARK, BAD, GOOD, GREAT,
}
public enum MOOD_STATE {
    Normal, Bad, Critical
}
public enum SEXUALITY {
    STRAIGHT, BISEXUAL, GAY
}
public enum FACILITY_TYPE { NONE, HAPPINESS_RECOVERY, FULLNESS_RECOVERY, TIREDNESS_RECOVERY, SIT_DOWN_SPOT  }
public enum FURNITURE_TYPE { NONE, BED, TABLE, DESK, GUITAR, }
public enum RELATIONSHIP_EFFECT {
    NONE,
    NEUTRAL,
    POSITIVE,
    NEGATIVE,
}
public enum REACTION_STATUS { WITNESSED, INFORMED,}
public enum PLAYER_SKILL_TYPE { NONE = 0, LYCANTHROPY = 1, KLEPTOMANIA = 2, VAMPIRISM = 3, UNFAITHFULNESS = 4, CANNIBALISM = 5, ZAP = 6,
    DESTROY = 7, RAISE_DEAD = 8, METEOR = 9, IGNITE = 10, AGORAPHOBIA = 12, ALCOHOLIC = 13, PLAGUE = 14,
    PARALYSIS = 15, PSYCHOPATHY = 17, TORNADO = 18, RAVENOUS_SPIRIT = 19, FEEBLE_SPIRIT = 20, FORLORN_SPIRIT = 21, POISON_CLOUD = 22, LIGHTNING = 23, EARTHQUAKE = 24,
    LOCUST_SWARM = 25/*, SPAWN_BOULDER = 26*/, WATER_BOMB = 27, MANIFEST_FOOD = 28, BRIMSTONES = 29,
    SPLASH_POISON = 30, BLIZZARD = 31, RAIN = 32, POISON = 33, BALL_LIGHTNING = 34, ELECTRIC_STORM = 35, FROSTY_FOG = 36, VAPOR = 37, FIRE_BALL = 38,
    POISON_BLOOM = 39, LANDMINE = 40, TERRIFYING_HOWL = 41, FREEZING_TRAP = 42, SNARE_TRAP = 43, WIND_BLAST = 44, ICETEROIDS = 45, HEAT_WAVE = 46, TORTURE = 47,
    SEIZE_OBJECT = 50, SEIZE_CHARACTER = 51, SEIZE_MONSTER = 52, BUILD_DEMONIC_STRUCTURE = 59, AFFLICT = 60,
    BREED_MONSTER = 62, COWARDICE = 67, PYROPHOBIA = 68, NARCOLEPSY = 69,
    PLANT_GERM = 70, MEDDLER = 71, WATCHER = 72, CRYPT = 73, KENNEL = 74, OSTRACIZER = 75, TORTURE_CHAMBERS = 76, DEMONIC_PRISON = 77,
    DEMON_WRATH = 78, DEMON_PRIDE = 79, DEMON_LUST = 80, DEMON_GLUTTONY = 81, DEMON_SLOTH = 82, DEMON_ENVY = 83, DEMON_GREED = 84,
    KILL = 87, EMPOWER = 88, AGITATE = 89, HOTHEADED = 90, LAZINESS = 91, HEAL = 92, SPLASH_WATER = 93, WALL = 94,
    MUSIC_HATER = 95, DEFILER = 96, GLUTTONY = 99, WOLF = 100, GOLEM = 101, INCUBUS = 102, SUCCUBUS = 103, FIRE_ELEMENTAL = 104, KOBOLD = 105, GHOST = 106,
    ABOMINATION = 107, MIMIC = 108, PIG = 109, CHICKEN = 110, SHEEP = 111, SLUDGE = 112,
    WATER_NYMPH = 113, WIND_NYMPH = 114, ICE_NYMPH = 115, ELECTRIC_WISP = 116, EARTHEN_WISP = 117, FIRE_WISP = 118,
    GRASS_ENT = 119, SNOW_ENT = 120, CORRUPT_ENT = 121, DESERT_ENT = 122, FOREST_ENT = 123,
    GIANT_SPIDER = 124, SMALL_SPIDER = 125, BRAINWASH = 138, UNSUMMON = 139, TRIGGER_FLAW = 140, CULTIST_TRANSFORM = 141,
    CULTIST_POISON = 142, CULTIST_BOOBY_TRAP = 143, VENGEFUL_GHOST = 144, WURM = 145, TROLL = 146, REVENANT = 147, SNATCH = 148,
    SACRIFICE = 149, REPAIR = 150, EVANGELIZE = 151, SPREAD_RUMOR = 152, FOUND_CULT = 153, BONE_GOLEM = 154, BIOLAB = 155, PLAGUED_RAT = 156, UPGRADE = 157,
    SCHEME = 158, INSTIGATE_WAR = 159, RESIGN = 160, LEAVE_FACTION = 161, LEAVE_HOME = 162, LEAVE_VILLAGE = 163,
    BREAK_UP = 164, JOIN_FACTION = 165, REBELLION = 166, OVERTHROW_LEADER = 167, INDUCE_MIGRATION = 168, STIFLE_MIGRATION = 169,
    RELEASE = 170, EXPEL = 172, PROTECTION = 173, REMOVE_BUFF = 174, REMOVE_FLAW = 175, SCORPION = 176, HARPY = 177, TRITON = 178,
    CULTIST_JOIN_FACTION = 179, SKELETON = 180, SPIRE = 181, SPAWN_EYE_WARD = 182, DESTROY_EYE_WARD = 183, MANA_PIT = 184, MARAUD = 185,
    DRAIN_SPIRIT = 186, LET_GO = 187, FULL_HEAL = 188, CREATE_BLACKMAIL = 189, DEFENSE_POINT = 190, IMP_HUT = 191, ICE_BLAST = 192, EARTH_SPIKE = 193, WATER_SPIKE = 194,
    RELEASE_ABILITIES = 195, SNATCH_VILLAGER = 196, SNATCH_MONSTER = 197, RAID = 198, UPGRADE_ABILITIES = 199, DEFEND = 200, 
    UPGRADE_PORTAL = 201, DESTROY_STRUCTURE = 202, DIRE_WOLF = 203, SPAWN_NECRONOMICON = 204, SPAWN_PARTY = 205, UPGRADE_BEHOLDER_EYE_LEVEL = 206, UPGRADE_BEHOLDER_RADIUS_LEVEL = 207,
    SPAWN_RATMAN = 208,
}
public enum PLAYER_SKILL_CATEGORY { NONE, SPELL, AFFLICTION, PLAYER_ACTION, DEMONIC_STRUCTURE, MINION, SUMMON, SCHEME, }

#region Player Skill Categories and Subcategories
[System.AttributeUsage(System.AttributeTargets.Field)]
public class PlayerSkillSubCategoryOf : System.Attribute {
    public PlayerSkillSubCategoryOf(PLAYER_SKILL_CATEGORY cat) {
        Category = cat;
    }
    public PLAYER_SKILL_CATEGORY Category { get; private set; }
}
#endregion


public enum COMBAT_ABILITY {
    SINGLE_HEAL, FLAMESTRIKE, FEAR_SPELL, SACRIFICE, TAUNT,
}

public enum SUMMON_TYPE {
    None, 
    Wolf, 
    Skeleton,
    Golem,
    Succubus,
    Incubus,
    Fire_Elemental,
    Kobold,
    Giant_Spider,
    Mimic,
    Small_Spider,
    Abomination,
    Pig,
    Sheep,
    Chicken,
    Desert_Ent,
    Forest_Ent,
    Snow_Ent,
    Grass_Ent,
    Corrupt_Ent,
    Ice_Nymph,
    Water_Nymph,
    Wind_Nymph,
    Electric_Wisp,
    Fire_Wisp,
    Earthen_Wisp,
    Sludge,
    Ghost,
    Warrior_Angel,
    Magical_Angel,
    Vengeful_Ghost,
    Wurm,
    Revenant,
    Dragon,
    Troll,
    Bone_Golem,
    Rat,
    Scorpion,
    Harpy,
    Triton,
    Imp,
    Dire_Wolf,
    Bear,
    Boar,
    Moonwalker,
    Mink,
    Rabbit,
}
public enum ARTIFACT_TYPE { None, Necronomicon, Ankh_Of_Anubis, Berserk_Orb, Heart_Of_The_Wind, Gorgon_Eye }
public enum ABILITY_TAG { NONE, MAGIC, SUPPORT, DEBUFF, CRIME, PHYSICAL, }
public enum LANDMARK_YIELD_TYPE { SUMMON, ARTIFACT, ABILITY, SKIRMISH, STORY_EVENT, }
public enum SERIAL_VICTIM_TYPE { None, Gender, Race, Class, Trait }
public enum DEADLY_SIN_ACTION { SPELL_SOURCE, INSTIGATOR, BUILDER, SABOTEUR, INVADER, FIGHTER, RESEARCHER, }
public enum RESOURCE { FOOD, WOOD, STONE, METAL, NONE, CLOTH, LEATHER }
public enum MAP_OBJECT_STATE { BUILT, UNBUILT, BUILDING }
public enum FACTION_IDEOLOGY { Inclusive = 0, Exclusive = 1, Warmonger = 2, Peaceful = 3, Divine_Worship = 4, Nature_Worship = 5, Demon_Worship = 6,
    Reveres_Vampires = 7, Reveres_Werewolves = 8, Hates_Vampires = 9, Hates_Werewolves = 10, Bone_Golem_Makers = 11,
}
public enum BEHAVIOUR_COMPONENT_ATTRIBUTE { WITHIN_HOME_SETTLEMENT_ONLY, ONCE_PER_DAY, DO_NOT_SKIP_PROCESSING, STOPS_BEHAVIOUR_LOOP } //, OUTSIDE_SETTLEMENT_ONLY
public enum EXCLUSIVE_IDEOLOGY_CATEGORIES { RACE, GENDER, TRAIT, RELIGION }
public enum EMOTION { None, Fear, Approval, Embarassment, Disgust, Anger, Betrayal, Concern, Disappointment, Scorn, Sadness, Threatened,
    Arousal, Disinterest, Despair, Shock, Resentment, Disapproval, Gratefulness, Rage, Plague_Hysteria, Distraught,
}
public enum PLAYER_ARCHETYPE { Normal, Ravager, Lich, Puppet_Master, Tutorial, Icalawa, Affatt, Oona, Pangat_Loo, Zenko, Aneem, Pitto, Old_Ravager, Old_Lich, Old_Puppet_Master }
public enum ELEMENTAL_TYPE { Normal, Fire, Poison, Water, Ice, Electric, Earth, Wind, }
/// <summary>
/// STARTED - actor is moving towards the target but is not yet performing action
/// PERFORMING - actor arrived at the target and is performing action
/// SUCCESS - only when action is finished; if action is successful
/// FAIL - only when action is finished; if action failed
/// </summary>
public enum ACTION_STATUS { NONE, STARTED, PERFORMING, SUCCESS, FAIL }
public enum ARTIFACT_UNLOCKABLE_TYPE { Structure, Action }
public enum COMBAT_MODE { Aggressive, Passive, Defend, }
public enum WALL_TYPE { Stone, Flesh, Demon_Stone }
public enum PARTICLE_EFFECT { None, Poison, Freezing, Fire, Burning, Explode, Electric, Frozen, Poison_Explosion, 
    Frozen_Explosion, Smoke_Effect, Lightning_Strike, Meteor_Strike, Water_Bomb, Poison_Bomb, Blizzard, Destroy_Explosion, Minion_Dissipate, Brimstones,
    Rain, Landmine, Burnt, Terrifying_Howl, Freezing_Trap, Snare_Trap, Wind_Blast, Iceteroids, Heat_Wave, Gorgon_Eye, Landmine_Explosion, Freezing_Trap_Explosion,
    Snare_Trap_Explosion, Fervor, Desert_Rose, Winter_Rose, Build_Demonic_Structure, Zombie_Transformation, Torture_Cloud, Freezing_Object,
    Necronomicon_Activate, Berserk_Orb_Activate, Artifact, Infected, Ankh_Of_Anubis_Activate, Fog_Of_War, Stoned, Demooder,
    Disabler, Overheating, Transform_Revert, Teleport, Protection, Build_Grid_Tile_Smoke, Place_Demonic_Structure, Eye_Ward_Highlight, Heal, Ice_Blast, Earth_Spike, Water_Spike, Taunt, Resist,
}
public enum PLAYER_SKILL_STATE { Locked, Unlocked, Learned, }
public enum REACTABLE_EFFECT { Neutral, Positive, Negative, }
public enum STRUCTURE_TAG { Dangerous, Treasure, Monster_Spawner, Shelter, Physical_Power_Up, Magic_Power_Up, Counterattack, Resource }
public enum LOG_TYPE { None, Action, Assumption, Witness, Informed }
public enum AWARENESS_STATE { None, Available, Missing, Presumed_Dead }
public enum PARTY_QUEST_TYPE { None, Exploration, Rescue, Extermination, Counterattack, Monster_Invade, Raid, Heirloom_Hunt, Demon_Defend, Demon_Snatch, Demon_Raid, Demon_Rescue, Morning_Patrol, Night_Patrol, Hunt_Beast, }
public enum PARTY_STATE { None, Waiting, Moving, Resting, Working, }
public enum GATHERING_TYPE { Social, Monster_Invade }
public enum COMBAT_REACTION { None, Fight, Flight }
public enum RUMOR_TYPE { Action, Interrupt }
public enum ASSUMPTION_TYPE { Action, Interrupt }
public enum CRIMABLE_TYPE { Action, Interrupt }
public enum OBJECT_TYPE { 
    Character = 0, Summon = 1, Minion = 2, Faction = 3, Region = 4, Area = 5, Structure = 6, Settlement = 7, Gridtile = 8, Trait = 9, Job = 10, 
    Action = 12, Interrupt = 13, Tile_Object = 14, Player = 15, Log = 16, Burning_Source = 17, Rumor = 18, Assumption = 19, Party = 20, Crime = 21, Party_Quest = 22, Gathering = 23,
    Reaction_Quest = 24, Plague_Disease = 25
}
public enum PASSIVE_SKILL {
    None, Monster_Chaos_Orb, Undead_Chaos_Orb, Enemies_Chaos_Orb, Auto_Absorb_Chaos_Orb, Passive_Mana_Regen, Prayer_Chaos_Orb, Spell_Damage_Chaos_Orb, Mental_Break_Chaos_Orb, Plague_Chaos_Orb, Player_Success_Raid_Chaos_Orb, Dark_Ritual_Chaos_Orb, Raid_Chaos_Orb, Night_Creature_Chaos_Orb, Trap_Chaos_Orb,
    Meddler_Chaos_Orb, Trigger_Flaw_Chaos_Orb, Lycanthrope_Chaos_Orb, Skill_Base_Chaos_Orb,
}
public enum LOG_TAG {
    Life_Changes, Social, Needs, Work, Combat, Crimes, Witnessed, Informed, Party, Major, Player, Intel, Important
}
public enum PARTY_TARGET_DESTINATION_TYPE { Structure, Settlement, Area, }
public enum SETTLEMENT_TYPE { Human_Village, Elven_Hamlet, Capital, Cult_Town }

public enum RELATIONS_FILTER {
    Enemies, Rivals, Acquaintances, Friends, Close_Friends, Relatives, Lovers,
}
public enum OVERLAP_UI_TAG { Top, Bottom, }

public enum SETTLEMENT_EVENT {
    Vampire_Hunt, Werewolf_Hunt, Plagued_Event,
}
public enum RELIGION {
    None, Demon_Worship, Divine_Worship, Nature_Worship
}
public enum PLAGUE_FATALITY {
    Septic_Shock, Heart_Attack, Stroke, Total_Organ_Failure, Pneumonia
}
public enum PLAGUE_SYMPTOM {
    Paralysis, Vomiting, Lethargy, Seizure, Insomnia, Poison_Cloud, Monster_Scent, Sneezing, Depression, Hunger_Pangs,
}
public enum PLAGUE_TRANSMISSION {
    Airborne, Consumption, Physical_Contact, Combat
}
public enum PLAGUE_DEATH_EFFECT {
    Explosion, Zombie, Chaos_Generator, Haunted_Spirits,
}
public enum PLAGUE_EVENT_RESPONSE {
    Undecided, Do_Nothing, Quarantine, Slay, Exile
}
public enum SETTLEMENT_JOB_TRIGGER {
    Plague_Care
}
public enum TEMPTATION {
    Dark_Blessing, Empower, Cleanse_Flaws
}
public enum BLACKMAIL_TYPE {
    None, Strong, Normal, Weak,
}
public enum FACTION_SUCCESSION_TYPE {
    None, Lineage, Popularity, Power
}

public enum MAP_SIZE {
    Small, Medium, Large, Extra_Large
}
public enum VILLAGE_SIZE {
    Small, Medium, Large
}
public enum MIGRATION_SPEED {
    None, Slow, Normal 
}
public enum SKILL_COOLDOWN_SPEED {
    None, Half, Normal, Double 
}
public enum SKILL_COST_AMOUNT {
    None, Half, Normal, 
}
public enum SKILL_CHARGE_AMOUNT {
    Unlimited, Half, Normal, Double 
}
public enum RETALIATION {
    Enabled, Disabled 
}
public enum OMNIPOTENT_MODE {
    Disabled, Enabled 
}
public enum VICTORY_CONDITION {
    Eliminate_All = 0, Wipe_Out_Village_On_Day = 2, Wipe_Elven_Kingdom_Survive_Humans = 3, Kill_By_Plague = 5, Create_Demon_Cult = 6, Summon_Ruinarch = 7, Sandbox = 8, 
}
public enum RESISTANCE {
    None = 0/*, Normal = 1*/, Fire = 2, Poison = 3, Water = 4, Ice = 5, Electric = 6, Earth = 7, Wind = 8, Mental = 9, Physical = 10,
}

public enum UPGRADE_BONUS {
    Damage = 0, Pierce, HP_HEAL_Percentage, HP_Actual_Amount, Max_HP_Percentage, Max_HP_Actual, Atk_Percentage, Atk_Actual_Amount, Mana_Received, Amplify_Effect_By_Percentage, Duration, Chance_Bonus_Percentage, Tile_Range, Decrease_Movement_Speed, Cooldown, Skill_Movement_Speed, Applied_Blessed_On_Max_Level, None,
}

public enum EQUIPMENT_TYPE { WEAPON = 0, ARMOR, ACCESSORY }

public enum EQUIPMENT_BONUS {
    Increased_Piercing = 0, Increased_3_Random_Resistance, Increased_4_Random_Resistance, Increased_5_Random_Resistance, Max_HP_Percentage, Max_HP_Actual, Str_Percentage, Str_Actual, Attack_Element, Slayer_Bonus, Ward_Bonus, Flight, Int_Percentage, Int_Actual, Crit_Rate_Actual, Random_Ward_Bonus, Random_Slayer_Bonus, None,
}

public enum EQUIPMENT_SLAYER_BONUS { 
    None = 0, Monster_Slayer, Elf_Slayer, Human_Slayer, Undead_SLayer, Demon_Slayer,
}

public enum EQUIPMENT_WARD_BONUS {
    None = 0, Monster_Ward, Elf_Wawrd, Human_Ward, Undead_Ward, Demon_Ward,
}

public enum EQUIPMENT_CLASS_COMPATIBILITY {
    Knight = 0, Noble, Hero, Barbarian, Marauder, Cult_Leader, Archer, Hunter, Stalker, Mage, Shaman, Druid, Necromancer, Non_Combatant,
}

public enum CONCRETE_RESOURCES { 
    Copper = 0, Iron, Mithril, Orichalcum, Rabbit_Cloth, Mink_Cloth, Wool, Spider_Silk, Moon_Thread, Boar_Hide, Scale_Hide, Dragon_Hide, Stone, Diamond, 
    Elf_Meat, Human_Meat, Animal_Meat, Fish, Corn, Potato, Pineapple, Iceberry, Mushroom, Wood, Wolf_Hide, Bear_Hide, Mooncrawler_Cloth, Gold, Hypno_Herb,
    Rat_Meat, Vegetables
}

public enum UNLOCKING_SKILL_REQUIREMENT {
    Archetype = 0, Skills, actions_count, affliction_count, spells_count, tier1_count, tier2_count, tier3_count, portal_level,
}

public enum SOUND_EFFECT {
    Heal, Resist,
}

public enum CURRENCY { 
    Mana = 0, Chaotic_Energy, Spirit_Energy,
}

public enum MINION_TYPE { 
    Lust = 0, Envy, Greed, Gluttony, Pride, Sloth, Wrath, 
}

public enum STORED_TARGET_TYPE {
    Character, Tile_Objects, Structures, Monster, Village
}
public enum CHARACTER_COMBAT_BEHAVIOUR {
    None, Tower, Attacker, Snatcher, Razer, Healer, Tank, Escort, Glass_Cannon, Defender,
}
public enum COMBAT_SPECIAL_SKILL {
    None, Heal, Taunt, Fast_Heal, Strong_Heal, Group_Heal, Self_Heal, Slow_Taunt, Max_Heal,
}
public enum COMBAT_SPECIAL_SKILL_TARGET {
    Single, Multiple,
}

public enum BOOKMARK_CATEGORY {
    None, Win_Condition, Major_Events, Portal, Player_Parties, Targets
}

public enum BOOKMARK_TYPE {
    Progress_Bar, Text, Text_With_Cancel, Special
}

public enum AFFLICTION_UPGRADE_BONUS {
    Pierce = 0, Crowd_Number, CoolDown, Hunger_Rate, Trigger_Rate, Trigger_Opinion, Naps_Percent, Naps_Duration, Added_Behaviour, Number_Criteria, Criteria, Duration,
}

public enum AFFLICTION_SPECIFIC_BEHAVIOUR {
    None, Make_Anxious, No_Longer_Join_Parties, Ignore_Urgent_Tasks, Angry_Upon_Hear_Music, Knockout_Singers_Guitar_Players, Murder_Singers_Guitar_Players, Active_Search_Affair, Multiple_Affair, Wild_Multiple_Affair, Pass_Out_From_Fright, Flees_From_Anyone, Do_Pick_Pocket, Rob_From_House, Rob_Any_Place, May_Suffer_heart_Attack,
    Becomes_Stronger_Dire_wolf, Transform_Master_Werewolf, Form_Lycan_Clan_faction, Add_And_Selection, Add_Or_Selection, Transform_into_bats, Can_Become_Vampire_Lords,
    Create_Vampire_Clan_Faction, Likes_To_Sleep, Loves_To_Sleep,
}

public enum LIST_OF_CRITERIA {
    NoOne = 0, Trait, Class, Gender, Race,
}

public enum OPINIONS { 
    NoOne = 0, Rival, Enemy, Acquaintance, Everyone,
}

public enum EQUIP_MATERIAL {
    Diamond = 0, Stone, Mithrill, Wolf_Hide, Any_Metal, Any_Wood, Any_Cloth, Any_Leather,
}


public enum CHANCE_TYPE {
    Kleptomania_Pickpocket_Level_1, Kleptomania_Pickpocket_Level_2, Kleptomania_Rob_Other_House, Kleptomania_Rob_Any_Place,
    Base_Cult_Leader_Spawn_Chance, Laziness_Nap_Level_2, Laziness_Nap_Level_3, Unfaithful_Active_Search_Affair,
    Ignore_Urgent_Task,
    Flirt_Acquaintance_Become_Lover_Chance,
    Flirt_Acquaintance_Become_Affair_Chance,
    Flirt_Friend_Become_Lover_Chance,
    Flirt_Friend_Become_Affair_Chance,
    Flirt_On_Sight_Base_Chance,
    Vampire_Lord_Chance,
    Host_Social_Party,
    Demonic_Decor_On_Corrupt,
    Retaliation_Character_Death,
    Retaliation_Structure_Destroy,
    Retaliation_Resource_Pile,
    Harpy_Capture,
    Lycanthrope_Transform_Chance,
    Visit_Friend,
    Ent_Spawn,
    Mimic_Spawn,
    Vampire_Lycan_Visit_Hospice,
    Settlement_Ruler_Default_Facility_Chance,
    Kidnap_Chance,
    Raid_Kidnap_Chance,
    Raid_Chance,
    Rescue_Chance,
    Find_Fish,
    Party_Quest_First_Knockout,
    Change_Intent,
    Change_Intent_Kleptomania,
    Change_Intent_Vampire,
    Change_Intent_Cultist,
    Plauged_Injured_Visit_Hospice,
    Hunt_Chance,
    Free_Time_Obtain_Want,
    Do_Work_Chance,
    Monster_Migration,
    Socialize_Chance,
    Visit_Village_Chance,
    Create_Change_Class_Combatant,
    Personal_Combatant_Change_Class,
    Kobold_Place_Freezing_Trap,
    Base_Create_Faction_Chance,
    Vagrant_Join_Or_Create_Faction,
    Vampire_Hunt_Drink_Blood_Chance,
    Werewolf_Hunt_On_See_Werewolf,
    Werewolf_Hunt_Mangled,
    Plagued_Event_Lethargic,
    Plagued_Event_Paralyzed,
    Plagued_Event_Heart_Attck,
    Plagued_Event_Pneumonia,
    Plagued_Event_Puke,
    Plagued_Event_Seizure,
    Plagued_Event_Septic_Shock,
    Plagued_Event_Sneeze,
    Plagued_Event_Stroke,
    Plagued_Event_Organ_Failure
}
public enum Gradient_Direction {
    Top, Bottom, Left, Right
}
public enum Biome_Tile_Type {
    Desert, Oasis, Grassland, Jungle, Taiga, Tundra, Snow
}
public enum Tile_Tag {
    Decor, Tree, Berry_Shrub, None
}
public enum Temperature_Type {
    Coldest, Colder, Cold, Hot, Hotter, Hottest
}
public enum Precipitation_Type {
    Dryest, Dryer, Dry, Wet, Wetter, Wettest
}
public enum CHARACTER_TALENT { None, Martial_Arts, Combat_Magic, Healing_Magic, Crafting, Resources, Food, Social }

public enum EQUIPMENT_QUALITY { Normal = 0, High, Premium }
public enum DAILY_SCHEDULE { Free_Time, Work, Sleep }
public enum VISIT_VILLAGE_INTENT { Socialize, Steal, Drink_Blood, Preach }

public enum TIPS { Time_Manager = 0, Chaotic_Energy, Target_Menu, Base_Building, Unlocking_Powers, Upgrading_Portal }