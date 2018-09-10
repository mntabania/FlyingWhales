﻿public enum PROGRESSION_SPEED {
    X1,
    X2,
    X4
}
public enum BIOMES{
    GRASSLAND,
    SNOW,
	TUNDRA,
	DESERT,
	//WOODLAND,
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
    NONE,
	HUMANS,
	ELVES,
	MINGONS,
	CROMADS,
	UNDEAD,
    GOBLIN,
	TROLL,
	DRAGON,
	DEHKBRUG,
    WOLF,
    SLIME,
    BEAST,
}

public enum PATH_DIRECTION{
	TOP_LEFT,
	TOP_RIGHT,
	RIGHT,
	BOTTOM_RIGHT,
	BOTTOM_LEFT,
	LEFT
}

public enum HEXTILE_DIRECTION {
    NORTH_WEST,
    NORTH_EAST,
    EAST,
    SOUTH_EAST,
    SOUTH_WEST,
    WEST,
    NONE
}
public enum KINGDOM_RELATIONSHIP_STATUS{
	SPITE,
	HATE,
	DISLIKE,
	NEUTRAL,
	LIKE,
	AFFECTIONATE,
	LOVE,
	NA
}

public enum PATHFINDING_MODE{
	POINT_TO_POINT,
	NORMAL,
    ROAD_CREATION,
    NO_MAJOR_ROADS,
	MAJOR_ROADS,
	MINOR_ROADS,
	MAJOR_ROADS_ONLY_KINGDOM,
	MAJOR_ROADS_WITH_ALLIES,
	MINOR_ROADS_ONLY_KINGDOM,
    REGION_CONNECTION,
    LANDMARK_ROADS,
    USE_ROADS,
	USE_ROADS_WITH_ALLIES,
	USE_ROADS_TRADE,
    //USE_ROADS_FACTION_RELATIONSHIP,
    //NORMAL_FACTION_RELATIONSHIP,
    LANDMARK_CONNECTION,
    UNRESTRICTED,
    PASSABLE,
    PASSABLE_REGION_ONLY,
    REGION_ISLAND_CONNECTION,
    AREA_ONLY,
}

public enum GENDER{
	MALE,
	FEMALE,
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
public enum RESOURCE {
    NONE,
    //OAK,
    //ELF_CIVILIAN,
    //HUMAN_CIVILIAN,
    //IRON,
    WOOD,
    IRON,
    GOLD,
    FOOD,

}
//public enum RESOURCE_TYPE{
//	NONE,
//	FOOD,
//	MATERIAL,
//	ORE,
//}
public enum MATERIAL{
	NONE, //0
	CLAY, //11
	LIMESTONE, //12
	GRANITE, //14
	MARBLE,
	SILK,
	COTTON,
	PIGMEAT,
	COWMEAT,
	GOATHIDE,
	DEERHIDE,
	BEHEMOTHHIDE,
	OAK,
	YEW,
	EBONY,
	IRON,
	COBALT,
	MITHRIL,
    FLAX,
    CORN,
    RICE
}
public enum MATERIAL_CATEGORY{
	NONE,
	METAL,
	WOOD,
	STONE,
	CLOTH,
	PLANT,
	MEAT,
	LEATHER,
}
//public enum ROLE{
//	UNTRAINED,
//	FOODIE, //Farming or Hunting
//	GATHERER, //Lumberyard or Quarry
//	MINER, 
//	TRADER,
//	SPY,
//	GUARDIAN,
//	ENVOY,
//	GENERAL,
//	GOVERNOR,
//	KING,
//	EXPANDER,
//	RAIDER,
//	REINFORCER,
//	REBEL,
//    EXTERMINATOR,
//    SCOURGE,
//    HEALER,
//	PROVOKER,
//	MISSIONARY,
//	ABDUCTOR,
//    LYCANTHROPE,
//	INVESTIGATOR,
//	THIEF,
//    WITCH,
//    ADVENTURER,
//	RELIEVER,
//	INTERCEPTER,
//	RANGER,
//    MILITARY_ALLIANCE_OFFICER,
//	TREATYOFFICER,
//	TRIBUTER,
//	INSTIGATOR,
//    GRAND_CHANCELLOR,
//    GRAND_MARSHAL,
//    QUEEN,
//    CROWN_PRINCE,
//	CARAVAN,
//	REFUGEE,
//}

public enum KINGDOM_TYPE {
	BARBARIC_TRIBE,
	NAIVE_TRIBE,
	OPPORTUNISTIC_TRIBE,
	DEFENSIVE_KINGDOM,
	OFFENSIVE_KINGDOM,
	SCIENTIFIC_KINGDOM,
	BALANCED_KINGDOM,
	RIGHTEOUS_SUPERPOWER,
	WICKED_SUPERPOWER,
	NONE
}

//public enum BASE_RESOURCE_TYPE{
//	FOOD,
//	WOOD,
//	STONE,
//	MANA_STONE,
//	MITHRIL,
//	COBALT,
//	GOLD,
//	NONE
//}

#region Traits
public enum TRAIT {
    NONE,
    IMPERIALIST,
    HOSTILE,
    PACIFIST,
    SCHEMING,
    OPPORTUNIST,
    EFFICIENT,
    INEPT,
    MEDDLER,
    SMART,
    DUMB,
    CHARISMATIC,
    REPULSIVE,
    RUTHLESS,
    DECEITFUL,
    BENEVOLENT,
    DIPLOMATIC,
    DEFENSIVE,
    HONEST,
    RACIST,
	ROBUST,
	FRAGILE,
    STRONG,
    WEAK,
    CLUMSY,
    AGILE,
    GRITTY
}
public enum CHARISMA {
    NONE = TRAIT.NONE,
    CHARISMATIC = TRAIT.CHARISMATIC,
    REPULSIVE = TRAIT.REPULSIVE,
}
public enum EFFICIENCY {
    NONE = TRAIT.NONE,
    EFFICIENT = TRAIT.EFFICIENT,
    INEPT = TRAIT.INEPT,
    
}
public enum INTELLIGENCE {
    NONE = TRAIT.NONE,
    SMART = TRAIT.SMART,
    DUMB = TRAIT.DUMB,
}
public enum MILITARY {
    NONE = TRAIT.NONE,
    HOSTILE = TRAIT.HOSTILE,
    PACIFIST = TRAIT.PACIFIST,
}
public enum HEALTH {
    NONE = TRAIT.NONE,
    ROBUST = TRAIT.ROBUST,
    FRAGILE = TRAIT.FRAGILE
}
public enum STRENGTH {
    NONE = TRAIT.NONE,
    STRONG = TRAIT.STRONG,
    WEAK = TRAIT.WEAK
}
public enum AGILITY {
    NONE = TRAIT.NONE,
    AGILE = TRAIT.AGILE,
    CLUMSY = TRAIT.CLUMSY
}
#endregion


public enum WAR_TYPE{
	INTERNATIONAL,
	CIVIL,
	SUCCESSION,
	NONE,
}
public enum EVENT_TYPES{
	NONE,
	BORDER_CONFLICT, // no agent
	INVASION_PLAN, // no agent
	JOIN_WAR_REQUEST, // envoy
	MILITARIZATION, // general
	STATE_VISIT, // envoy
	ASSASSINATION, // spy
	ESPIONAGE,
	RAID, // raider
	REBELLION_PLOT,
	POWER_GRAB,
	EXHORTATION,
	KINGDOM_SUCCESSION,
	GOVERNOR_SUCCESSION,
	KINGDOM_WAR,
	REQUEST_PEACE, // envoy
	CIVIL_WAR,
	SUCCESSION_WAR,
	MARRIAGE_INVITATION,
	EXPANSION, // settler
	TRADE, // trader
	DIPLOMATIC_CRISIS, // no agent
	ADMIRATION,
	ALL,
	ATTACK_CITY, // general
	SABOTAGE, //envoy
	REINFORCE_CITY, //general
	DEFEND_CITY, //general
	SECESSION, //governor
	RIOT_WEAPONS, //no agent
	REBELLION, //general
	PLAGUE,
	SCOURGE_CITY,
	BOON_OF_POWER,
	PROVOCATION,
	EVANGELISM,
	SPOUSE_ABDUCTION,
    LYCANTHROPY,
	FIRST_AND_KEYSTONE,
	RUMOR,
	SLAVES_MERCHANT,
	HIDDEN_HISTORY_BOOK,
    HYPNOTISM,
    KINGDOM_HOLIDAY,
	SERUM_OF_ALACRITY,
    DEVELOP_WEAPONS,
    KINGS_COUNCIL,
	ALTAR_OF_BLESSING,
    ADVENTURE,
	ATTACK_LAIR,
    EVIL_INTENT,
	GREAT_STORM,
	SEND_RELIEF_GOODS,
    ANCIENT_RUIN,
	HUNT_LAIR,
	KINGDOM_DISCOVERY,
	CRIME,
    MILITARY_ALLIANCE_OFFER,
	MUTUAL_DEFENSE_TREATY,
	TRIBUTE,
	INSTIGATION,
    REGRESSION,
    RIOTING_SETTLEMENTS,
	SEND_RESOURCES,
	CARAVANEER,
	INTERNATIONAL_INCIDENT,
    ALLIANCE_OF_PROTECTION_OFFER,
    ALLIANCE_OF_CONQUEST_OFFER,
    TRADE_DEAL_OFFER,
	REFUGE,
}
public enum PLAYER_EVENT{
	KINGDOM_DISCOVERY,
	CRIME,
}
public enum EVENT_STATUS{
	EXPOSED,
	HIDDEN,
}

public enum STRUCTURE_TYPE{
	NONE,
	CITY,
	FARM,
	HUNTING_LODGE,
	QUARRY,
	LUMBERYARD,
	MINE,
	TRADING_POST,
    GENERIC
	//BARRACKS,
	//SPY_GUILD,
	//MINISTRY,
	//KEEP
}

public enum STRUCTURE_QUALITY{
	NONE,
	BASIC,
	ADVANCED,
}

public enum HISTORY_IDENTIFIER{
	NONE,
	KING_RELATIONS
}

public enum DEATH_REASONS{
	NONE,
	OLD_AGE,
	ACCIDENT,
	BATTLE,
	TREACHERY,
	ASSASSINATION,
	REBELLION,
	INTERNATIONAL_WAR,
	STARVATION,
	DISAPPEARED_EXPANSION,
   	PLAGUE,
    LYCANTHROPE,
    MURDER,
	SUICIDE,
    TRIED_TO_CURE_LYCANTHROPY,
	SERUM_OF_ALACRITY,
    EVIL_INTENT,
    INCURABLE_DISEASE,
    RIOT
}

public enum ASSASSINATION_TRIGGER_REASONS{
	ASSASSINATION_KING,
	ASSASSINATION_ROYALTY,
	ASSASSINATION_GOVERNOR,
	ASSASSINATION_CIVILIAN,
	DISCOVERED_RAID_NO_DEATH,
	DISCOVERED_RAID_WITH_DEATH,
	BORDER_CONFLICT,
	DIPLOMATIC_CRISIS,
	STATE_VISIT,
	JOIN_WAR,
	TARGET_DECLARED_WAR_AGAINST_FRIEND,
	TARGET_DECLARED_WAR_AGAINST_ALLY,
	TARGET_LOST_A_BATTLE,
	TARGET_GAINED_A_CITY,
	OPPOSING_APPROACH,
	FIRST_AND_KEYSTONE_STEAL,
	DISCOVERED_INTERCEPTER,
	SUCCESS_RUMOR,
	CAUGHT_RUMOR,
	NONE
}

public enum STATEVISIT_TRIGGER_REASONS{
	NONE,
	DISCOVERING_A,
	DISCOVERING_IP,
	STATE_VISIT,
	ASSASSINATION,
	BORDER_CONFLICT,
	ESPIONAGE,
	RAID,
	DIPLOMATIC_CRISIS,

}

public enum INVASION_TRIGGER_REASONS{
	NONE,
	DISCOVERING_A,
	DISCOVERING_IP,
	STATE_VISIT,
	ASSASSINATION,
	BORDER_CONFLICT,
	ESPIONAGE,
	RAID,
	DIPLOMATIC_CRISIS,

}

public enum LANGUAGES{
	NONE,
	ENGLISH,
}

public enum MILITARY_STRENGTH{
	MUCH_WEAKER,
	SLIGHTLY_WEAKER,
	COMPARABLE,
	SLIGHTLY_STRONGER,
	MUCH_STRONGER,
	NA
}

public enum WAR_TRIGGER {
	ASSASSINATION_KING,
	ASSASSINATION_ROYALTY,
	ASSASSINATION_GOVERNOR,
	ASSASSINATION_CIVILIAN,
	DISCOVERED_RAID_NO_DEATH,
	DISCOVERED_RAID_WITH_DEATH,
	BORDER_CONFLICT,
	DIPLOMATIC_CRISIS,
	JOIN_WAR,
	TARGET_DECLARED_WAR_AGAINST_FRIEND,
	TARGET_DECLARED_WAR_AGAINST_ALLY,
	TARGET_LOST_A_BATTLE,
	TARGET_GAINED_A_CITY,
	OPPOSING_APPROACH,
    THE_FIRST_AND_THE_KEYSTONE,
    EVIL_INTENT,
	GREAT_STORM,
	SPOUSE_ABDUCTION,
	MARRIAGE_INCOMPATIBILITY,
	NONE
}
public enum ASSASSINATION_TRIGGER{
	NONE,
}
public enum DIRECTION{
	LEFT,
	RIGHT,
	UP,
	DOWN,
}

public enum EMBARGO_REASON {
    NONE,
    PLAGUE
}

public enum RATE_TYPE{
	CUMULATIVE,
}
public enum RESOURCE_BENEFITS {
    GROWTH_RATE,
    EXPANSION_RATE,
    TECH_LEVEL
}

public enum EVENT_APPROACH {
    NONE,
    PRAGMATIC,
    OPPORTUNISTIC,
    HUMANISTIC
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
    COMBAT,
    //PARTY_NAME,
    OTHER,
    STRING_1,
    STRING_2,
}

public enum LAIR {
	LYCAN,
//	STORM_WITCH,
	PERE,
	GHOUL,
}

public enum MONSTER{
	LYCAN,
	STORM_WITCH,
	PERE,
	GHOUL,
}

public enum STRUCTURE_STATE {
    NORMAL,
    RUINED,
}

public enum PUNISHMENT{
	NO,
	LIGHT,
	HARSH,
}
public enum BEHAVIOR{
	HOMING,
	ROAMING,
}
public enum PURPOSE{
	BALANCE,
	SUPERIORITY,
	BANDWAGON,
}
public enum KINGDOM_SIZE{
	SMALL,
	MEDIUM,
	LARGE,
}
public enum WAR_SIDE{
	NONE,
	A,
	B,
}

public enum CITIZEN_STATUS_EFFECTS {
    INCURABLE_DISEASE
}

public enum ORDER_BY {
    NAME,
    POPULATION,
    CITIES,
    EXPANSION_RATE,
    WEAPONS,
    ARMOR,
    CHARACTERS
}
public enum WARMONGER{
	VERY_HIGH,
	HIGH,
	AVERAGE,
	LOW,
	VERY_LOW,
}
public enum RELATIONSHIP_MODIFIER{
	LEAVE_ALLIANCE,
	REBELLION,
	FLATTER,
	INTERNATIONAL_INCIDENT,
}
public enum DECAY_INTERVAL{
	DAILY,
	MONTHLY,
	YEARLY,
}
public enum AGENT_CATEGORY {
    LIVING,
    STRUCTURE
}
public enum AGENT_TYPE {
    NONE,
    MONSTER,
    GUARD,
    CITY,
    LAIR,
    NECROMANCER
}
public enum ROAD_TYPE{
    NONE,
    MAJOR,
	MINOR,
	ALL,
}
//public enum BASE_LANDMARK_TYPE {
//    NONE,
//    SETTLEMENT,
//    RESOURCE,
//    DUNGEON,
//    LAIR
//}
public enum LANDMARK_TAG {
    CAN_HUNT,
    CAN_SCAVENGE,
}
public enum LANDMARK_TYPE {
    NONE = 0,
    DEMONIC_PORTAL = 1,
    ELVEN_SETTLEMENT = 2,
    HUMAN_SETTLEMENT = 3,
    GARRISON = 4,
    OAK_FORTIFICATION = 5,
    IRON_FORTIFICATION = 6,
    OAK_LUMBERYARD = 7,
    IRON_MINES = 8,
    INN = 9,
    //PUB = 9,
    TEMPLE = 10,
    HUNTING_GROUNDS = 11,
    ELVEN_HOUSES = 12,
    HUMAN_HOUSES = 13,
    MONSTER_DEN = 14,
    SNATCHER_DEMONS_LAIR = 15,
    SHOP = 16,
    FARM = 17,
    GOLD_MINE = 18,
    LUMBERYARD = 19,
    PALACE = 20,
}
public enum WEIGHTED_ACTION_REQS {
    NONE,
    NO_ALLIANCE,
    HAS_ALLIANCE,
    HAS_WAR,
    HAS_ACTIVE_TRADE_DEAL
}
public enum WEIGHTED_ACTION {
    DO_NOTHING,
    //WAR_OF_CONQUEST,
    ALLIANCE_OF_CONQUEST,
    ALLIANCE_OF_PROTECTION,
    TRADE_DEAL,
    INCITE_UNREST,
    START_INTERNATIONAL_INCIDENT,
    FLATTER,
    SEND_AID,
    //DECLARE_PEACE,
    LEAVE_ALLIANCE,
    LEAVE_TRADE_DEAL,
}

public enum GENERAL_TASKS{
	ATTACK_CITY,
	DEFEND_CITY,
	REINFORCE_CITY,
}
public enum RESPONSE {
    ACCEPT,
    REJECT
}
public enum ACTION_CHOICES {
    DO_ACTION,
    DONT_DO_ACTION
}
public enum TECHNOLOGY {
    //Weapon Production
    BOW_MAKING,
    SWORD_MAKING,
    SPEAR_MAKING,
    DAGGER_MAKING,
    AXE_MAKING,
    STAFF_MAKING,

    //Armor Production
    CHEST_ARMOR_MAKING,
    LEGGINGS_MAKING,
    HELMET_MAKING,
    GLOVE_MAKING,
    BOOT_MAKING,

    //Construction
    BASIC_FARMING,
    ADVANCED_FARMING,
    BASIC_HUNTING,
    ADVANCED_HUNTING,
    BASIC_MINING,
    ADVANCED_MINING,
    BASIC_WOODCUTTING,
    ADVANCED_WOODCUTTING,
    BASIC_QUARRYING,
    ADVANCED_QUARRYING,

    //Training Tier 1
    ARCHER_CLASS,
    SWORDSMAN_CLASS,
    SPEARMAN_CLASS,
    WILDLING_CLASS,
    ROGUE_CLASS,
    MAGE_CLASS,

    //Training Tier 2
    RANGER_CLASS,
    BATTLEMAGE_CLASS,
    SCOUT_CLASS,
    BARBARIAN_CLASS,
    KNIGHT_CLASS,
    ARCANIST_CLASS,
    NIGHTBLADE_CLASS,

    //Unlock character roles
    ESPIONAGE,
    DIPLOMACY,
    NECROMANCY,
    DRAGON_TAMING,

    //Miscellaneous
    GOBLIN_LANGUAGE,
    ELVEN_LANGUAGE,
    HUMAN_LANGUAGE,
    TROLL_LANGUAGE,

	NONE,
}
public enum CHARACTER_ROLE {
    NONE,
    //CHIEFTAIN,
    //WARLORD,
    HERO,
    //ADVENTURER,
    //COLONIST,
    //VILLAGE_HEAD,
    //WORKER,
	//TAMED_BEAST,
	//BANDIT,
    //ANCIENT_VAMPIRE,
	//CRATER_BEAST,
	//SLYX,
	VILLAIN,
    CIVILIAN,
    KING,
    PLAYER,
    //FOLLOWER,
    //HERMIT,
    //BEAST,
}
public enum CHARACTER_CLASS {
    WARRIOR,
    BARBARIAN,
    SHOPKEEPER,
    MINER,
    WOODCUTTER,
    FARMER,
    RETIRED_HERO
}
//public enum CHARACTER_JOB {
//    NONE,
//    SHOPKEEPER,
//    MINER,
//    WOODCUTTER,
//    FARMER,
//    RETIRED_HERO
//}
public enum CHARACTER_PRODUCTION_CAP {
    CITY_COUNT,
    MINOR_FACTION,
    SMALL_TRIBE,
    MEDIUM_TRIBE,
    LARGE_TRIBE,
    ENTIRE_WORLD,
    PER_TRIBE,
    PER_SETTLEMENT
}
public enum TASK_STATUS {
    IN_PROGRESS,
    SUCCESS,
    FAIL,
    CANCEL
}
public enum QUEST_TYPE { 
    RELEASE_CHARACTER,
    BUILD_STRUCTURE,
    FETCH_ITEM,
}
public enum GROUP_TYPE {
    SOLO,
    PARTY,
}
public enum TASK_TYPE { //This is the list of actions a character can do on it's own
    QUEST,
    REST,
    GO_HOME,
    DO_NOTHING,
    JOIN_PARTY,
    MOVE_TO,
    TAKE_QUEST,
    UPGRADE_GEAR,
	DROP_PRISONERS,
    HUNT_PREY,
    HIBERNATE,
    PILLAGE,
    RECRUIT_FOLLOWERS,
	EXPLORE_TILE,
	RAZE,
	ATTACK,
	PATROL,
    HUNT_MAGIC_USER,
	VAMPIRIC_EMBRACE,
	DRINK_BLOOD,
	HYPNOTIZE,
    SEARCH,
    REPORT,
	CALL_SLYXES,
	SIPHON_SLYX,
	COMMAND_INFECTION,
	DO_RITUAL,
	COLLECT,
    PROCLAIM_SUCCESSOR,
	INFECT_PSYTOXIN,
	INVADE,
	ROB,
    MOVE_TO_BEAST,
	STEAL,
    PROWL,
	ATTACK_ENEMY,
	MOVE_TOWARDS_CHARACTER,
    FAINT,
}
public enum TASK_ACTION_RESULT {
    SUCCESS,
    FAIL,
    CANCEL
}
public enum PARTY_ACTION {
    STAY,
    LEAVE
}
public enum FACTION_RELATIONSHIP_STATUS {
    NON_HOSTILE,
    HOSTILE
}
public enum INTERNATIONAL_INCIDENT_TYPE {
    CHARACTER_DEATH,
    HARMFUL_QUEST
}
public enum INTERNATIONAL_INCIDENT_ACTION {
    DO_NOTHING,
    DECLARE_WAR
}
public enum ALLY_WAR_REACTION {
    JOIN_WAR,
    REMAIN_NEUTRAL,
    BETRAY
}
//---------------------------------------- ENTITY COMPONENT SYSTEM ---------------------------------------//
//public enum BODY_PART{
//	HEAD,
//	TORSO,
//	TAIL,
//	ARM,
//	HAND,
//	LEG,
//	FEET,
//    HEART,
//    BRAIN,
//	EYE,
//	NOSE,
//	EAR,
//	ELBOW,
//	WRIST,
//	FINGER,
//	THIGH,
//	KNEE,
//	SHIN,
//	BREAST,
//	ABS,
//	RIB,
//	MOUTH,
//	WING,
//	HORN,
//	HIP,
//	CROTCH,
//	ASS,
//	PELVIS,
//}

public enum CHARACTER_CLASS_TYPE {
    GENERAL,
	NINJA
}

public enum SKILL_TYPE {
    ATTACK,
    HEAL,
    OBTAIN_ITEM,
    FLEE,
    MOVE
}
public enum SKILL_CATEGORY {
	GENERAL,
	CLASS,
}

public enum ATTACK_TYPE {
    CRUSH,
    PIERCE,
    SLASH,
    MAGIC,
	STATUS
}
public enum DEFEND_TYPE {
    DODGE,
    PARRY,
    BLOCK,
	NONE,
}
public enum STATUS_EFFECT {
    NONE,
	INJURED,
	DECAPITATED,
    POISONED,
    STUNNED,
    BLEEDING,
    BURNING,
	CONFUSED,
}
public enum CHARACTER_ATTRIBUTES {
    STRENGTH,
    INTELLIGENCE,
    AGILITY
}

public enum ITEM_TYPE{
	WEAPON,
	ARMOR,
	CONSUMABLE,
	MISC,
}

public enum EQUIPMENT_TYPE {
    NONE,
    SWORD,
    DAGGER,
	SPEAR,
    BOW,
	STAFF,
	AXE,
    SHIRT,
    BRACER,
    HELMET,
	LEGGINGS,
	BOOT,
    GREAT_SWORD,
}

public enum WEAPON_TYPE{
	NONE = EQUIPMENT_TYPE.NONE,
	SWORD = EQUIPMENT_TYPE.SWORD,
	DAGGER = EQUIPMENT_TYPE.DAGGER,
	SPEAR = EQUIPMENT_TYPE.SPEAR,
	BOW = EQUIPMENT_TYPE.BOW,
    STAFF = EQUIPMENT_TYPE.STAFF,
	AXE = EQUIPMENT_TYPE.AXE,
    GREAT_SWORD = EQUIPMENT_TYPE.GREAT_SWORD,
}

public enum ARMOR_TYPE{
	NONE = EQUIPMENT_TYPE.NONE,
	SHIRT = EQUIPMENT_TYPE.SHIRT,
	BRACER = EQUIPMENT_TYPE.BRACER,
	HELMET = EQUIPMENT_TYPE.HELMET,
	LEGGINGS = EQUIPMENT_TYPE.LEGGINGS,
	BOOT = EQUIPMENT_TYPE.BOOT,
}

public enum ACTIVATION_WEIGHT_TYPE{
	NONE,
	CURRENT_HEALTH,
	MISSING_HEALTH,
	ALLY_MISSING_HEALTH,
}

public enum QUALITY{
	NORMAL,
	CRUDE,
	EXCEPTIONAL,
}

public enum ENCOUNTERABLE{
	ITEM_CHEST,
	PARTY,
}

public enum ATTRIBUTE{
    SAPIENT,

    HUNGRY,
    STARVING,

    FAMISHED,

    TIRED,
    EXHAUSTED,

    SAD,
    DEPRESSED,
    
    INSECURE,
    DRUNK,

    DISTURBED,
    CRAZED,

    ANXIOUS,
    DEMORALIZED,

    WOUNDED,
    WRECKED,

    IMPULSIVE,
    BETRAYED,
    HEARTBROKEN,

    GREGARIOUS,
    BOOKWORM,
    DEAFENED,
    MUTE,
    SINGER,
    ROYALTY,
    DAYDREAMER,
    MEDITATOR,
    LIBERATED,
    UNFAITHFUL,
    DIRTY,
    CLEANER,
    DO_NOT_DISTURB,
    INTROVERT,
    EXTROVERT,
    BELLIGERENT,
    HUMAN,
    STALKER,
}

public enum PRODUCTION_TYPE{
	WEAPON,
	ARMOR,
	CONSTRUCTION,
	TRAINING
}

public enum LOCATION_IDENTIFIER{
	LANDMARK,
	HEXTILE,
}

public enum STANCE {
    COMBAT,
    NEUTRAL,
    STEALTHY
}

public enum MODE {
    DEFAULT,
    ALERT,
    STEALTH
}

public enum STORYLINE {
    THE_DYING_KING,
}

public enum ACTION_ALIGNMENT {
    LAWFUL,
    UNLAWFUL,
    HEROIC,
    VILLAINOUS,
    PEACEFUL,
    HOSTILE
}

public enum STATE{
	NONE,
	DO_NOTHING,
	SEARCH,
	MOVE,
	EXPLORE,
	PATROL,
	ATTACK,
	REST,
	HIBERNATE,
    HUNT_MAGIC_USER,
	SIPHON,
	COMMAND_INFECTION,
	INFECT,
	RITUAL,
	HUNT,
	PILLAGE,
	RAZE,
    REPORT,
    DRINK_BLOOD,
    HYPNOTIZE,
    VAMPIRIC_EMBRACE,
	RECRUIT,
	PURCHASE,
    TAKE_QUEST,
    PROCLAIM_SUCCESSOR,
	COLLECT,
	INVADE,
    PROWL,
	STEAL,
    FAINTED,
}

public enum CHARACTER_RELATIONSHIP{
	FRIEND,
    MENTOR,
    STUDENT,
    FATHER,
    MOTHER,
    BROTHER,
    SISTER,
    SON,
    DAUGHTER,
    LOVER,
    HUSBAND,
    WIFE,
    ENEMY,
    RIVAL,
    STALKER,
    //RIVAL,
	//FRIEND,
	//ENEMY,
	//SIBLING,
	//PARENT,
	//CHILD,
	//LOVER,
	//EX_LOVER,
	//APPRENTICE,
	//MENTOR,
	//ACQUAINTANCE,
}

public enum CHARACTER_RELATIONSHIP_CATEGORY{
	NEGATIVE,
	POSITIVE,
	FAMILIAL,
	NEUTRAL,
}

public enum COMBAT_INTENT{
	KILL,
	IMPRISON,
	DEFEAT,
}

public enum ACTION_TYPE {
    REST,
    MOVE,
    HUNT,
    DESTROY,
    EAT,
    BUILD,
    REPAIR,
    DRINK,
    HARVEST,
    IDLE,
    POPULATE,
    TORTURE,
    PATROL,
    ABDUCT,
    DISPEL,
    PRAY,
    ATTACK,
    JOIN_BATTLE,
    GO_HOME,
    RELEASE,
    CLEANSE,
    ENROLL,
    TRAIN,
    DAYDREAM,
    BERSERK,
    PLAY,
    FLAIL,
    SELFMUTILATE,
    DEPOSIT,
    CHANGE_CLASS,
    CHAT,
    WAITING,
    DISBAND_PARTY,
    FORM_PARTY,
    IN_PARTY,
    JOIN_PARTY,
    GRIND,
    MINING,
    WOODCUTTING,
    WORKING,
    PARTY,
    READ,
    SING,
    PLAYING_INSTRUMENT,
    MEDITATE,
    FOOLING_AROUND,
    HOUSEKEEPING,
    ARGUE,
    STALK,
    QUESTING,
    FETCH,
    WAIT_FOR_PARTY,
    TURN_IN_QUEST,
}
public enum ACTION_CATEGORY {
    WORK,
    MISC,
}

public enum OBJECT_TYPE {
    CHARACTER,
    STRUCTURE,
    ITEM,
    NPC,
    LANDMARK,
    MONSTER,
}

public enum ACTION_RESULT {
    SUCCESS,
    FAIL,
}
public enum NEEDS {
    FULLNESS,
    ENERGY,
    FUN,
    PRESTIGE,
    SANITY,
    SAFETY,
}

public enum ACTION_FILTER_TYPE {
    ROLE,
    LOCATION,
    CLASS,
}
public enum ACTION_FILTER_CONDITION {
    IS,
    IS_NOT,
}

public enum ACTION_FILTER {
    HERO,
    VILLAIN,
    RUINED,
    CIVILIAN,
    FARMER,
    MINER,
    WOODCUTTER,
}

public enum PREREQUISITE {
    RESOURCE,
    ITEM,
    POWER,
}

public enum PASSABLE_TYPE {
    UNPASSABLE,
    MAJOR_DEADEND,
    MINOR_DEADEND,
    MAJOR_BOTTLENECK,
    MINOR_BOTTLENECK,
    CROSSROAD,
    WIDE_OPEN,
    OPEN
}

public enum IMAGE_SIZE {
    X64,
    X256,
    X72,
    X36,
}

public enum LEVEL {
    HIGH,
    AVERAGE,
    LOW
}
public enum BASE_AREA_TYPE {
    SETTLEMENT,
    DUNGEON,
    PLAYER,
}
public enum AREA_TYPE {
    ELVEN_SETTLEMENT,
    HUMAN_SETTLEMENT,
    WILDLANDS,
    DEMONIC_INTRUSION, //Player area
    ANCIENT_RUINS,
}
public enum ELEMENT {
    NONE,
    FIRE,
    WATER,
    EARTH,
    WIND,
}

public enum MONSTER_TYPE {
    SLIME,
    BEAST,
    FLORAL,
    INSECT,
    HUMANOID,
    DEMON,
}

public enum MONSTER_CATEGORY {
    NORMAL,
    BOSS,
}

public enum ATTACK_CATEGORY {
    PHYSICAL,
    MAGICAL,
}

public enum WEAPON_PREFIX {
    NONE,
}

public enum WEAPON_SUFFIX {
    NONE,
}

public enum ARMOR_PREFIX {
    NONE,
}

public enum ARMOR_SUFFIX {
    NONE,
}

public enum MESSAGE_BOX_MODE {
    MESSAGE_ONLY,
    YES_NO
}

public enum ICHARACTER_TYPE {
    CHARACTER,
    MONSTER,
}

public enum MOVEMENT_TYPE {
    NORMAL,
    AVOID
}

public enum TARGET_TYPE {
    SINGLE,
    ROW,
}
public enum ATTRIBUTE_CATEGORY {
    CHARACTER,
    ITEM,
    STRUCTURE,
}
public enum ATTRIBUTE_BEHAVIOR {
    NONE,
    DEPLETE_FUN,
}
public enum SCHEDULE_PHASE_TYPE {
    WORK,
    MISC,
    SPECIAL,
}
public enum ABUNDANCE {
    NONE,
    HIGH,
    MED,
    LOW,
}