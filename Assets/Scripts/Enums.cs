﻿public enum BIOMES{
	SNOW,
	TUNDRA,
	DESERT,
	GRASSLAND,
	WOODLAND,
	FOREST,
	BARE,
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
	HUMANS,
	ELVES,
	MINGONS,
	CROMADS,
}
	
public enum CITY_TASK { 
	PURCHASE_TILE, 
	ASSIGN_CITIZEN 
}

public enum PATH_DIRECTION{
	TOP_LEFT,
	TOP_RIGHT,
	RIGHT,
	BOTTOM_RIGHT,
	BOTTOM_LEFT,
	LEFT
}

public enum REPRESENTATIVES{
	KING,
	MAYOR,
	CITIZENS,
}
public enum RELATIONSHIP_STATUS{
	RIVAL,
	ENEMY,
	COLD,
	NEUTRAL,
	WARM,
	FRIEND,
	ALLY,
}

public enum PATHFINDING_MODE{
	NORMAL,
	ROAD_CREATION,
	COMBAT
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
	CEDAR,
	OAK,
	EBONY,
	GRANITE,
	SLATE,
	MARBLE,
	MANA_STONE,
	MITHRIL,
	COBALT,
	GOLD,
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
	KING
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

public enum BEHAVIOR_TRAIT{
	NONE,
	NAIVE,
	SCHEMING,
	WARMONGER,
	PACIFIST,
	CHARISMATIC,
	REPULSIVE,
	AGGRESSIVE,
	DEFENSIVE,
}

public enum SKILL_TRAIT{
	NONE,
	GREEN_THUMB,
	HUNTER,
	EFFICIENT,
	INEFFICIENT,
	THRIFTY,
	LAVISH,
	STEALTHY,
	PERSUASIVE,
	ALERT,
}

public enum MISC_TRAIT{
	NONE,
	BARREN,
	HORNY,
	HOMOSEXUAL,
	LOYAL,
	AMBITIOUS,
	FAST,
	ROBUST,
	STRONG,
	TACTICAL,
}

public enum CAMPAIGN{
	NONE,
	OFFENSE,
	DEFENSE,
}
	
public enum WAR_TYPE{
	INTERNATIONAL,
	CIVIL,
	SUCCESSION,
	NONE,
}
public enum EVENT_TYPES{
	BORDER_CONFLICT,
	INVASION_PLAN,
	JOIN_WAR_REQUEST,
	MILITARIZATION,
	STATE_VISIT,
	ASSASSINATION,
	ESPIONAGE,
	RAID,
	REBELLION_PLOT,
	POWER_GRAB,
	EXHORTATION,
	KINGDOM_SUCCESSION,
	GOVERNOR_SUCCESSION,
	KINGDOM_WAR,
	REQUEST_PEACE,
	CIVIL_WAR,
	SUCCESSION_WAR,
	MARRIAGE_INVITATION,
	EXPANSION,
	ALL
}
public enum EVENT_STATUS{
	EXPOSED,
	HIDDEN,
}

public enum STRUCTURE{
	NONE,
	CITY,
	FARM,
	HUNTING_LODGE,
	QUARRY,
	LUMBERYARD,
	MINES,
	TRADING_POST,
	BARRACKS,
	SPY_GUILD,
	MINISTRY,
	KEEP
}

public enum HISTORY_IDENTIFIER{
	NONE,
}

public enum DEATH_REASONS{
	OLD_AGE,
	ACCIDENT,
	BATTLE,
	TREACHERY,
	ASSASSINATION,
	REBELLION,
	INTERNATIONAL_WAR,
	STARVATION,
}

public enum ASSASSINATION_TRIGGER_REASONS{
	NONE,
	DISCOVERING_A,
	DISCOVERING_IP,
	STATE_VISITING,
	STATE_VISIT,
	ASSASSINATION,
	BORDER_CONFLICT,
	ESPIONAGE,
	RAID,
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
}