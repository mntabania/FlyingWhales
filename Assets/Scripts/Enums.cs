﻿public enum PROGRESSION_SPEED {
    X1,
    X2,
    X4
}
public enum BIOMES{
	SNOW,
	TUNDRA,
	DESERT,
	GRASSLAND,
	WOODLAND,
	FOREST,
	BARE,
	NONE,
}
public enum EQUATOR_LINE{
	HORIZONTAL,
	VERTICAL,
	DIAGONAL_LEFT,
	DIAGONAL_RIGHT,
}
public enum ELEVATION{
	MOUNTAIN,
	WATER,
	PLAIN,
}

public enum RACE{
    NONE,
	HUMANS,
	ELVES,
	MINGONS,
	CROMADS,
	UNDEAD,
    GOBLIN
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
public enum RELATIONSHIP_STATUS{
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
	COMBAT,
	AVATAR,
    NO_HIDDEN_TILES,
    ROAD_CREATION,
    NO_MAJOR_ROADS,
	MAJOR_ROADS,
	MINOR_ROADS,
	MAJOR_ROADS_ONLY_KINGDOM,
	MAJOR_ROADS_WITH_ALLIES,
	MINOR_ROADS_ONLY_KINGDOM,
    REGION_CONNECTION,
    LANDMARK_CONNECTION,
    LANDMARK_EXTERNAL_CONNECTION,
	USE_ROADS,
	USE_ROADS_WITH_ALLIES,
	USE_ROADS_ONLY_KINGDOM,
	USE_ROADS_TRADE,
    UNIQUE_LANDMARK_CREATION
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
public enum RESOURCE{
	NONE,
	CORN,
	WHEAT,
	RICE,
	DEER,
	PIG,
	BEHEMOTH,
	OAK,
	EBONY,
	GRANITE,
	SLATE,
	MANA_STONE,
	MITHRIL,
	COBALT,
}
public enum RESOURCE_TYPE{
	NONE,
	FOOD,
	MATERIAL,
	ORE,
}
public enum ROLE{
	UNTRAINED,
	FOODIE, //Farming or Hunting
	GATHERER, //Lumberyard or Quarry
	MINER, 
	TRADER,
	SPY,
	GUARDIAN,
	ENVOY,
	GENERAL,
	GOVERNOR,
	KING,
	EXPANDER,
	RAIDER,
	REINFORCER,
	REBEL,
    EXTERMINATOR,
    SCOURGE,
    HEALER,
	PROVOKER,
	MISSIONARY,
	ABDUCTOR,
    LYCANTHROPE,
	INVESTIGATOR,
	THIEF,
    WITCH,
    ADVENTURER,
	RELIEVER,
	INTERCEPTER,
	RANGER,
    MILITARY_ALLIANCE_OFFICER,
	TREATYOFFICER,
	TRIBUTER,
	INSTIGATOR,
    GRAND_CHANCELLOR,
    GRAND_MARSHAL,
    QUEEN,
    CROWN_PRINCE,
	CARAVAN,
	REFUGEE,
}

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

public enum BASE_RESOURCE_TYPE{
	FOOD,
	WOOD,
	STONE,
	MANA_STONE,
	MITHRIL,
	COBALT,
	GOLD,
	NONE
}

#region Traits
public enum TRAIT {
    NONE,
    IMPERIALIST,
    BALANCER,
    BANDWAGONER,
    HOSTILE,
    MILITANT,
    PACIFIST,
    SCHEMING,
    OPPORTUNIST,
    EFFICIENT,
    INEFFICIENT,
    LOYAL,
    BACKSTABBER,
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
    HONEST
}
public enum CHARISMA {
    CHARISMATIC = TRAIT.CHARISMATIC,
    REPULSIVE = TRAIT.REPULSIVE,
    NONE = TRAIT.NONE
}
public enum EFFICIENCY {
    EFFICIENT = TRAIT.EFFICIENT,
    INEFFICIENT = TRAIT.INEFFICIENT,
    NONE = TRAIT.NONE
}
public enum INTELLIGENCE {
    SMART = TRAIT.SMART,
    DUMB = TRAIT.DUMB,
    NONE = TRAIT.NONE
}
public enum MILITARY {
    HOSTILE = TRAIT.HOSTILE,
    MILITANT = TRAIT.MILITANT,
    PACIFIST = TRAIT.PACIFIST,
    NONE = TRAIT.NONE,
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
	MINES,
	TRADING_POST,
    GENERIC
	//BARRACKS,
	//SPY_GUILD,
	//MINISTRY,
	//KEEP
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
	KINGDOM_1,
	KING_1,
	KING_1_SPOUSE,
	CITY_1,
	GOVERNOR_1,
	RANDOM_CITY_1,
	RANDOM_GOVERNOR_1,
	TARGET_CHARACTER,
	KINGDOM_2,
	KING_2,
	KING_2_SPOUSE,
	CITY_2,
	GOVERNOR_2,
	RANDOM_CITY_2,
	RANDOM_GOVERNOR_2,
	CHARACTER_3,
	KINGDOM_3,
	KING_3,
	KING_3_SPOUSE,
	CITY_3,
	GOVERNOR_3,
	RANDOM_CITY_3,
	RANDOM_GOVERNOR_3,
	TRIGGER_REASON,
	RANDOM_GENERATED_EVENT_NAME,
	ACTIVE_CHARACTER_PRONOUN_S,
	ACTIVE_CHARACTER_PRONOUN_O,
	ACTIVE_CHARACTER_PRONOUN_P,
	ACTIVE_CHARACTER_PRONOUN_R,
	KING_1_PRONOUN_S,
	KING_1_PRONOUN_O,
	KING_1_PRONOUN_P,
	KING_1_PRONOUN_R,
	KING_2_PRONOUN_S,
	KING_2_PRONOUN_O,
	KING_2_PRONOUN_P,
	KING_2_PRONOUN_R,
	TARGET_CHARACTER_PRONOUN_S,
	TARGET_CHARACTER_PRONOUN_O,
	TARGET_CHARACTER_PRONOUN_P,
	TARGET_CHARACTER_PRONOUN_R,
	SECESSION_CITIES,
	GAME_EVENT,
	DATE,
	KING_3_PRONOUN_S,
	KING_3_PRONOUN_O,
	KING_3_PRONOUN_P,
	KING_3_PRONOUN_R,
	CRIME_DETAILS,
	CRIME_PUNISHMENT,
	LAIR_NAME,
	WAR_NAME,
	ALLIANCE_NAME,
	OTHER,
}

public enum FOG_OF_WAR_STATE {
    HIDDEN,
    VISIBLE,
    SEEN
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

public enum STATUS_EFFECTS {
    INCURABLE_DISEASE
}

public enum KINGDOMS_ORDERED_BY {
    NAME,
    POPULATION,
    CITIES,
    EXPANSION_RATE,
    WEAPONS,
    ARMOR
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
public enum SUBTERFUGE_ACTIONS{
	DESTROY_WEAPONS,
//	DESTROY_ARMORS,
	REDUCE_STABILITY,
	FLATTER,
	SPREAD_PLAGUE,
    ASSASSINATE_KING,
    INTERNATIONAL_INCIDENT
}
public enum ENTITY_TYPE {
    GUARD,
    WORKER,
    BANDIT,
    MONSTER,
    ALL,
    NONE
}
public enum ACTION_TYPE {
    NONE,
    ATTACK,
    FLEE,
    RANDOM
    
}
public enum MOVE_TYPE {
    GROUND,
    FLYING,
    NONE
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
public enum BASE_LANDMARK_TYPE {
    SETTLEMENT,
    RESOURCE,
    DUNGEON,
    LAIR
}

public enum LANDMARK_TYPE {
    CORN,
    PIG,
    OAK,
    YEW,
    EBONY,
    SILK,
    COTTON,
    LEATHER,
    IRON,
    COBALT,
    MITHRIL,
    GOBLIN_CAMP,
    DARK_CAVE,
    ANCIENT_RUIN,
    ABANDONED_DUNGEON,
    MYSTERIOUS_TOWER,
    SUMMONING_SHRINE,
    CITY
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
public enum FACTION_TYPE {
    MAJOR,
    MINOR
}
public enum TECHNOLOGY {
    BOW_MAKING,
    SWORD_MAKING,
    SPEAR_MAKING,
    DAGGER_MAKING,
    AXE_MAKING,
    STAFF_MAKING,
    CLOTH_ARMOR_MAKING,
    METAL_ARMOR_MAKING,

    BASIC_TAILORING,
    ADVANCED_TAILORING,
    EXPERT_TAILORING,
    BASIC_WOODCRAFTING,
    ADVANCED_WOODCRAFTING,
    EXPERT_WOODCRAFTING,
    BASIC_SMITHING,
    ADVANCED_SMITHING,
    EXPERT_SMITHING,

    BASIC_FARMING,
    ADVANCED_FARMING,
    BASIC_HUNTING,
    ADVANCED_HUNTING,
    BASIC_MINING,
    ADVANCED_MINING,
    BASIC_WOODCUTTING,
    ADVANCED_WOODCUTTING,

    ARCHER_CLASS,
    SWORDSMAN_CLASS,
    SPEARMAN_CLASS,
    WILDLING_CLASS,
    ROGUE_CLASS,
    MAGE_CLASS,

    RANGER_CLASS,
    BATTLEMAGE_CLASS,
    SCOUT_CLASS,
    BARBARIAN_CLASS,
    KNIGHT_CLASS,
    ARCANIST_CLASS,
    SAVAGE_CLASS,

    ESPIONAGE,
    DIPLOMACY,
    NECROMANCY,
    DRAGON_TAMING,

    GOBLIN_LANGUAGE,
    ELVEN_LANGUAGE,
    HUMAN_LANGUAGE,
    TROLL_LANGUAGE
}
public enum CHARACTER_ROLE {
    NONE,
    CHIEFTAIN,
    WARLORD,
    HERO,
    TRADER,
    ADVENTURER,
    COLONIST,
    SPY,
    MEDIATOR,
    NECROMANCER,
    DRAGON_TAMER,
    VILLAGE_HEAD
}
public enum CHARACTER_CLASS {
    NONE,
    ARCHER,
    SPEARMAN,
    SWORDSMAN,
    ROGUE,
    WILDLING,
    MAGE,
    BARBARIAN,
    RANGER,
    KNIGHT,
    BATTLEMAGE,
    SENTRY,
    SAVAGE,
    SCOUT
}
public enum CHARACTER_PRODUCTION_CAP {
    CITY_COUNT,
    MINOR_FACTION,
    SMALL_TRIBE,
    MEDIUM_TRIBE,
    LARGE_TRIBE,
    ENTIRE_WORLD,
    PER_TRIBE
}
public enum FACTION_SIZE {
    SMALL,
    MEDIUM,
    LARGE,
}
public enum QUEST_RESULT {
    SUCCESS,
    FAIL,
    CANCEL
}
//---------------------------------------- ENTITY COMPONENT SYSTEM ---------------------------------------//
public enum BODY_PART{
	HEAD,
	TORSO,
	TAIL,
	ARM,
	HAND,
	LEG,
	FEET,
}
