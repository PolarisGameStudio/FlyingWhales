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
	NA
}

public enum PATHFINDING_MODE{
	USE_ROADS,
	NORMAL,
	COMBAT,
	RESOURCE_PRODUCTION,
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
	KING,
	EXPANDER,
	RAIDER,
}

public enum KINGDOM_TYPE {
	BARBARIC_TRIBE,
	HERMIT_TRIBE,
	RELIGIOUS_TRIBE,
	OPPORTUNISTIC_TRIBE,
	NOBLE_KINGDOM,
	EVIL_EMPIRE,
	MERCHANT_NATION,
	CHAOTIC_STATE,
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

public enum TRAIT{
	NONE,
	HONEST,
	SCHEMING,
	WARMONGER,
	PACIFIST,
	SMART,
	STUPID
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
	SABOTAGE //envoy
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
	DISAPPEARED_EXPANSION
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
	DIPLOMATIC_CRISIS,

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
	TARGET_DECLARED_WAR_AGAINST_FRIEND,
	TARGET_DECLARED_WAR_AGAINST_ALLY,
	TARGET_LOST_A_BATTLE,
	TARGET_GAINED_A_CITY,
	NONE
}

public enum DIRECTION{
	LEFT,
	RIGHT,
	UP,
	DOWN,
}