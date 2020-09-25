﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Inner_Maps;
using Traits;
using UnityEngine;

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
}
public enum ACTION_RESULT {
    SUCCESS,
    FAIL,
}
public enum BASE_AREA_TYPE {
    SETTLEMENT,
    DUNGEON,
    PLAYER,
}
public enum LOCATION_TYPE {
    SETTLEMENT,
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
public enum MORALITY {
    GOOD,
    EVIL,
    NEUTRAL,
}
public enum FACTION_TYPE {
    Elven_Kingdom,
    Human_Empire,
    Demons,
    Vagrants,
    Wild_Monsters,
    Bandits,
    Undead,
    Disguised
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
    CHAT_CHARACTER = 15,
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
    BUILD_STRUCTURE = 94,
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
    DRY_TILE = 141,
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
    FLEE_TO_TILE = 195,
    HUNT_HEIRLOOM = 196,
    REMOVE_TRAP = 197,
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
    Narcoleptic_Attack,
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
    HEX,
    STRUCTURE,
    ROOM,
}
public enum STRUCTURE_TYPE {
    TAVERN = 1,
    WAREHOUSE = 2,
    DWELLING = 3,
    WILDERNESS = 5,
    CEMETERY = 8,
    PRISON = 9,
    POND = 10,
    CITY_CENTER = 11,
    SMITHY = 12,
    BARRACKS = 13,
    APOTHECARY = 14,
    GRANARY = 15,
    MINER_CAMP = 16,
    RAIDER_CAMP = 17,
    ASSASSIN_GUILD = 18,
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
    EYE = 35,
    THE_NEEDLES = 36,
    TORTURE_CHAMBERS = 37,
    DEMONIC_PRISON = 38,
    FARM = 39,
    LUMBERYARD = 40,
    MINE_SHACK = 41,
    ANCIENT_GRAVEYARD = 42,
    TEMPLE = 43,
    RUINED_ZOO = 44,
}
public enum RELATIONSHIP_TYPE {
    NONE = 0,
    RELATIVE = 3,
    LOVER = 4,
    AFFAIR = 5,
    MASTER = 6,
    SERVANT = 7,
    SAVER = 8,
    SAVE_TARGET = 9,
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
    TREE_OBJECT = 9,
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
    METAL_PILE = 26,
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
    VEGETABLES = 144,
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
}
public enum POI_STATE {
    ACTIVE,
    INACTIVE,
}

public enum TARGET_POI { ACTOR, TARGET, }
public enum GridNeighbourDirection { North, South, West, East, North_West,  North_East, South_West, South_East }
public enum TIME_IN_WORDS { AFTER_MIDNIGHT, MORNING, AFTERNOON, EARLY_NIGHT, LATE_NIGHT, LUNCH_TIME, NONE }
//public enum CRIME_SEVERITY { NONE, INFRACTION, MISDEMEANOUR, SERIOUS_CRIME, }
public enum Food { BERRY, MUSHROOM, RABBIT, RAT }
public enum GOAP_EFFECT_CONDITION { NONE, REMOVE_TRAIT, HAS_TRAIT, FULLNESS_RECOVERY, TIREDNESS_RECOVERY, HAPPINESS_RECOVERY, STAMINA_RECOVERY, CANNOT_MOVE, REMOVE_FROM_PARTY, DESTROY, DEATH, PATROL, EXPLORE, REMOVE_ITEM, HAS_TRAIT_EFFECT, HAS_PLAN
        , TARGET_REMOVE_RELATIONSHIP, TARGET_STOP_ACTION_AND_JOB, RESTRAIN_CARRY, REMOVE_FROM_PARTY_NO_CONSENT, IN_VISION, REDUCE_HP, INVITED, MAKE_NOISE, STARTS_COMBAT, CHANGE_CLASS
        , PRODUCE_FOOD, PRODUCE_WOOD, PRODUCE_STONE, PRODUCE_METAL, DEPOSIT_RESOURCE, REMOVE_REGION_CORRUPTION, CLEAR_REGION_FACTION_OWNER, REGION_OWNED_BY_ACTOR_FACTION, FACTION_QUEST_DURATION_INCREASE
        , FACTION_QUEST_DURATION_DECREASE, DESTROY_REGION_LANDMARK, CHARACTER_TO_MINION, SEARCH
        , HAS_POI, TAKE_POI //The process of "take" in this manner is different from simply carrying the poi. In technicality, since the actor will only get an amount from the poi target, the actor will not carry the whole poi instead he/she will create a new poi with the amount that he/she needs while simultaneously reducing that amount from the poi target
        , ABSORB_LIFE, RAISE_CORPSE
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
        , BRAWL, PLACE_TRAP, SPREAD_RUMOR, CONFIRM_RUMOR, OPEN_CHEST, TEND_FARM, VISIT_DIFFERENT_REGION, BERSERK_ATTACK, MINE, DIG_THROUGH, SPAWN_LAIR, ABSORB_LIFE, ABSORB_POWER
        , SPAWN_SKELETON, RAISE_CORPSE, HUNT_PREY, DROP_ITEM, BERSERK_STROLL, RETURN_HOME_URGENT, SABOTAGE_NEIGHBOUR, SHARE_NEGATIVE_INFO
        , DECREASE_MOOD, DISABLE, MONSTER_EAT, ARSON, SEEK_SHELTER, DARK_RITUAL, CULTIST_TRANSFORM, CULTIST_POISON, CULTIST_BOOBY_TRAP, JOIN_GATHERING, EXPLORE, EXTERMINATE, RESCUE, RELEASE_CHARACTER, COUNTERATTACK_PARTY, MONSTER_BUTCHER
        , ROAM_AROUND_STRUCTURE, MONSTER_INVADE, PARTY_GO_TO, KIDNAP, RECRUIT, RAID, FLEE_CRIME, HOST_SOCIAL_PARTY, PARTYING, CRAFT_MISSING_FURNITURE, FULLNESS_RECOVERY_ON_SIGHT, HOARD, ZOMBIE_STROLL, WARM_UP, NO_PATH_IDLE, REPORT_CRIME
        , EVANGELIZE, HUNT_HEIRLOOM, SNATCH, DROP_ITEM_PARTY, GO_TO_WAITING, PRODUCE_FOOD_FOR_CAMP, KIDNAP_RAID, STEAL_RAID, IDLE_CAMP,
}

public enum JOB_OWNER { CHARACTER, SETTLEMENT, FACTION, }

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
public enum CRIME_STATUS { Unpunished, Punished, Exiled, Absolved, Executed }
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
public enum SPELL_TYPE { NONE = 0, LYCANTHROPY = 1, KLEPTOMANIA = 2, VAMPIRISM = 3, UNFAITHFULNESS = 4, CANNIBALISM = 5, ZAP = 6,
    DESTROY = 7, RAISE_DEAD = 8, METEOR = 9, IGNITE = 10, AGORAPHOBIA = 12, ALCOHOLIC = 13, PLAGUE = 14,
    PARALYSIS = 15, ZOMBIE_VIRUS = 16, PSYCHOPATHY = 17, TORNADO = 18, RAVENOUS_SPIRIT = 19, FEEBLE_SPIRIT = 20, FORLORN_SPIRIT = 21, POISON_CLOUD = 22, LIGHTNING = 23, EARTHQUAKE = 24,
    LOCUST_SWARM = 25, SPAWN_BOULDER = 26, WATER_BOMB = 27, MANIFEST_FOOD = 28, BRIMSTONES = 29,
    SPLASH_POISON = 30, BLIZZARD = 31, RAIN = 32, POISON = 33, BALL_LIGHTNING = 34, ELECTRIC_STORM = 35, FROSTY_FOG = 36, VAPOR = 37, FIRE_BALL = 38,
    POISON_BLOOM = 39, LANDMINE = 40, TERRIFYING_HOWL = 41, FREEZING_TRAP = 42, SNARE_TRAP = 43, WIND_BLAST = 44, ICETEROIDS = 45, HEAT_WAVE = 46, TORTURE = 47, SUMMON_MINION = 48,
    STOP = 49, SEIZE_OBJECT = 50, SEIZE_CHARACTER = 51, SEIZE_MONSTER = 52, RETURN_TO_PORTAL = 53, DEFEND = 54, HARASS = 55, INVADE = 56, LEARN_SPELL = 57, CHANGE_COMBAT_MODE = 58, BUILD_DEMONIC_STRUCTURE = 59, AFFLICT = 60, ACTIVATE = 61,
    BREED_MONSTER = 62, END_RAID = 63, END_HARASS = 64, END_INVADE = 65, INTERFERE = 66, COWARDICE = 67, PYROPHOBIA = 68, NARCOLEPSY = 69,
    PLANT_GERM = 70, MEDDLER = 71, EYE = 72, CRYPT = 73, KENNEL = 74, OSTRACIZER = 75, TORTURE_CHAMBERS = 76, DEMONIC_PRISON = 77,
    DEMON_WRATH = 78, DEMON_PRIDE = 79, DEMON_LUST = 80, DEMON_GLUTTONY = 81, DEMON_SLOTH = 82, DEMON_ENVY = 83, DEMON_GREED = 84,
    SKELETON_MARAUDER = 85, KNOCKOUT = 86, KILL = 87, EMPOWER = 88, AGITATE = 89, HOTHEADED = 90, LAZINESS = 91, HEAL = 92, SPLASH_WATER = 93, WALL = 94,
    MUSIC_HATER = 95, DEFILER = 96, ABDUCT = 97, ANIMATE = 98, GLUTTONY = 99,
    WOLF = 100, GOLEM = 101, INCUBUS = 102, SUCCUBUS = 103, FIRE_ELEMENTAL = 104, KOBOLD = 105, GHOST = 106,
    ABOMINATION = 107, MIMIC = 108, PIG = 109, CHICKEN = 110, SHEEP = 111, SLUDGE = 112,
    WATER_NYMPH = 113, WIND_NYMPH = 114, ICE_NYMPH = 115,
    ELECTRIC_WISP = 116, EARTHEN_WISP = 117, FIRE_WISP = 118,
    GRASS_ENT = 119, SNOW_ENT = 120, CORRUPT_ENT = 121, DESERT_ENT = 122, FOREST_ENT = 123,
    GIANT_SPIDER = 124, SMALL_SPIDER = 125,
    SKELETON_ARCHER = 126, SKELETON_BARBARIAN = 127, SKELETON_CRAFTSMAN = 128, SKELETON_DRUID = 129, SKELETON_HUNTER = 130, SKELETON_MAGE = 131, SKELETON_KNIGHT = 132, SKELETON_MINER = 133, SKELETON_NOBLE = 134, SKELETON_PEASANT = 135, SKELETON_SHAMAN = 136, SKELETON_STALKER = 137,
    BRAINWASH = 138, UNSUMMON = 139, TRIGGER_FLAW = 140,
    CULTIST_TRANSFORM = 141,
    CULTIST_POISON = 142,
    CULTIST_BOOBY_TRAP = 143,
    VENGEFUL_GHOST = 144, WURM = 145, TROLL = 146,
    REVENANT = 147,
    SNATCH = 148,
    SACRIFICE = 149
}
//public enum INTERVENTION_ABILITY_TYPE { NONE, AFFLICTION, SPELL, }
public enum SPELL_CATEGORY { NONE, SPELL, AFFLICTION, PLAYER_ACTION, DEMONIC_STRUCTURE, MINION, SUMMON }
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
}
public enum ARTIFACT_TYPE { None, Necronomicon, Ankh_Of_Anubis, Berserk_Orb, Heart_Of_The_Wind, Gorgon_Eye }
public enum ABILITY_TAG { NONE, MAGIC, SUPPORT, DEBUFF, CRIME, PHYSICAL, }
public enum LANDMARK_YIELD_TYPE { SUMMON, ARTIFACT, ABILITY, SKIRMISH, STORY_EVENT, }
public enum SERIAL_VICTIM_TYPE { None, Gender, Race, Class, Trait }
// public enum SPECIAL_OBJECT_TYPE { DEMON_STONE, SPELL_SCROLL, SKILL_SCROLL }
public enum WORLD_EVENT { NONE, HARVEST, SLAY_MINION, MINE_SUPPLY, STUDY, PRAY_AT_TEMPLE, DESTROY_DEMONIC_LANDMARK, HOLY_INCANTATION, CORRUPT_CULTIST, DEMONIC_INCANTATION, SEARCHING, CLAIM_REGION, CLEANSE_REGION, INVADE_REGION, ATTACK_DEMONIC_REGION, ATTACK_NON_DEMONIC_REGION }
public enum DEADLY_SIN_ACTION { SPELL_SOURCE, INSTIGATOR, BUILDER, SABOTEUR, INVADER, FIGHTER, RESEARCHER, }
public enum WORLD_EVENT_EFFECT { GET_FOOD, GET_SUPPLY, GAIN_POSITIVE_TRAIT, REMOVE_NEGATIVE_TRAIT, EXPLORE, COMBAT, DESTROY_LANDMARK, DIVINE_INTERVENTION_SPEED_UP, CORRUPT_CHARACTER, DIVINE_INTERVENTION_SLOW_DOWN, SEARCHING, CONQUER_REGION, REMOVE_CORRUPTION, INVADE_REGION, ATTACK_DEMONIC_REGION, ATTACK_NON_DEMONIC_REGION }
public enum WORLD_OBJECT_TYPE { NONE, ARTIFACT, SUMMON, SPECIAL_OBJECT, }
public enum REGION_FEATURE_TYPE { PASSIVE, ACTIVE }
public enum RESOURCE { FOOD, WOOD, STONE, METAL, NONE }
public enum MAP_OBJECT_STATE { BUILT, UNBUILT, BUILDING }
public enum FACTION_IDEOLOGY { Inclusive = 0, Exclusive = 1, Warmonger = 2, Peaceful = 3, Divine_Worship = 4, Nature_Worship = 5, Demon_Worship = 6 }
public enum BEHAVIOUR_COMPONENT_ATTRIBUTE { WITHIN_HOME_SETTLEMENT_ONLY, ONCE_PER_DAY, DO_NOT_SKIP_PROCESSING, } //, OUTSIDE_SETTLEMENT_ONLY
public enum EXCLUSIVE_IDEOLOGY_CATEGORIES { RACE, GENDER, }
public enum EMOTION { None, Fear, Approval, Embarassment, Disgust, Anger, Betrayal, Concern, Disappointment, Scorn, Sadness, Threatened,
    Arousal, Disinterest, Despair, Shock, Resentment, Disapproval, Gratefulness, Rage, Plague_Hysteria, Distraught,
}
public enum PLAYER_ARCHETYPE { Normal, Ravager, Lich, Puppet_Master, Tutorial, Second_World}
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
    Disabler, Overheating, Transform_Revert
}
public enum PLAYER_SKILL_STATE { Locked, Unlocked, Learned, }
public enum REACTABLE_EFFECT { Neutral, Positive, Negative, }
public enum STRUCTURE_TAG { Dangerous, Treasure, Monster_Spawner, Shelter, Physical_Power_Up, Magic_Power_Up, Counterattack, Resource }
public enum LOG_TYPE { None, Action, Assumption, Witness, Informed }
public enum AWARENESS_STATE { None, Available, Missing, Presumed_Dead }
public enum PARTY_QUEST_TYPE { Exploration, Rescue, Extermination, Counterattack, Monster_Invade, Raid, Heirloom_Hunt, }
public enum PARTY_STATE { None, Waiting, Moving, Resting, Working, }
public enum GATHERING_TYPE { Social, Monster_Invade }
public enum COMBAT_REACTION { None, Fight, Flight }
public enum RUMOR_TYPE { Action, Interrupt }
public enum ASSUMPTION_TYPE { Action, Interrupt }
public enum CRIMABLE_TYPE { Action, Interrupt }
public enum OBJECT_TYPE { 
    Character = 0, Summon = 1, Minion = 2, Faction = 3, Region = 4, Hextile = 5, Structure = 6, Settlement = 7, Gridtile = 8, Trait = 9, Job = 10, 
    Action = 12, Interrupt = 13, Tile_Object = 14, Player = 15, Log = 16, Burning_Source = 17, Rumor = 18, Assumption = 19, Party = 20, Crime = 21, Party_Quest = 22, Gathering = 23,
}
public enum PASSIVE_SKILL {
    Monster_Chaos_Orb, Undead_Chaos_Orb, Enemies_Chaos_Orb, Auto_Absorb_Chaos_Orb, Passive_Mana_Regen
}
public enum LOG_TAG {
    Life_Changes, Social, Needs, Work, Combat, Crimes, Witnessed, Informed, Party, Misc, Player, Intel
}
public enum PARTY_TARGET_DESTINATION_TYPE { Structure, Settlement, Hextile, }
public enum SETTLEMENT_TYPE { Default_Human, Default_Elf, Capital }


#region Crime Subcategories
[System.AttributeUsage(System.AttributeTargets.Field)]
public class SubcategoryOf : System.Attribute {
    public SubcategoryOf(CRIME_SEVERITY cat) {
        Category = cat;
    }
    public CRIME_SEVERITY Category { get; private set; }
}
#endregion
public static class Extensions {

    #region Crimes
    public static bool IsLessThan(this CRIME_SEVERITY sub, CRIME_SEVERITY other) {
        return sub < other;
    }
    public static bool IsGreaterThanOrEqual(this CRIME_SEVERITY sub, CRIME_SEVERITY other) {
        return sub >= other;
    }
    #endregion

    #region Structures
    /// <summary>
    /// Is this stucture contained within walls?
    /// </summary>
    /// <param name="sub"></param>
    /// <returns>True or false</returns>
    public static bool IsOpenSpace(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.WILDERNESS:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.POND:
            case STRUCTURE_TYPE.CITY_CENTER:
            case STRUCTURE_TYPE.THE_PORTAL:
            case STRUCTURE_TYPE.EYE:
            case STRUCTURE_TYPE.OCEAN:
            case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
                return true;
            default:
                return false;
        }
    }
    public static bool IsSettlementStructure(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.CITY_CENTER:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.PRISON:
            case STRUCTURE_TYPE.DWELLING:
            case STRUCTURE_TYPE.SMITHY:
            case STRUCTURE_TYPE.BARRACKS:
            case STRUCTURE_TYPE.APOTHECARY:
            case STRUCTURE_TYPE.GRANARY:
            case STRUCTURE_TYPE.MINER_CAMP:
            case STRUCTURE_TYPE.RAIDER_CAMP:
            case STRUCTURE_TYPE.ASSASSIN_GUILD:
            case STRUCTURE_TYPE.HUNTER_LODGE:
            case STRUCTURE_TYPE.MAGE_QUARTERS:
            case STRUCTURE_TYPE.FARM:
            case STRUCTURE_TYPE.LUMBERYARD:
            case STRUCTURE_TYPE.MINE_SHACK:
            case STRUCTURE_TYPE.TAVERN:
                return true;
            default:
                return false;
        }
    }
    public static bool IsFacilityStructure(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.TAVERN:
            case STRUCTURE_TYPE.CITY_CENTER:
            case STRUCTURE_TYPE.WAREHOUSE:
            case STRUCTURE_TYPE.FARM:
            case STRUCTURE_TYPE.MINE_SHACK:
            case STRUCTURE_TYPE.LUMBERYARD:
            case STRUCTURE_TYPE.APOTHECARY:
            case STRUCTURE_TYPE.CEMETERY:
            case STRUCTURE_TYPE.BARRACKS:
            case STRUCTURE_TYPE.MAGE_QUARTERS:
                return true;
            default:
                return false;
        }
    }
    public static int StructurePriority(this STRUCTURE_TYPE sub) {
        switch (sub) {
            case STRUCTURE_TYPE.WILDERNESS:
            case STRUCTURE_TYPE.POND:
            case STRUCTURE_TYPE.CEMETERY:
                return -1;
            case STRUCTURE_TYPE.DWELLING:
                return 0;
            case STRUCTURE_TYPE.CITY_CENTER:
                return 1;
            case STRUCTURE_TYPE.TAVERN:
                return 2;
            case STRUCTURE_TYPE.WAREHOUSE:
                return 3;
            case STRUCTURE_TYPE.PRISON:
                return 5;
            default:
                return 99;
        }
    }
    public static bool IsInterior(this STRUCTURE_TYPE structureType) {
        switch (structureType) {
            case STRUCTURE_TYPE.DWELLING:
            case STRUCTURE_TYPE.TAVERN:
            case STRUCTURE_TYPE.PRISON:
            case STRUCTURE_TYPE.SMITHY:
            case STRUCTURE_TYPE.GRANARY:
            case STRUCTURE_TYPE.BARRACKS:
            case STRUCTURE_TYPE.MINER_CAMP:
            case STRUCTURE_TYPE.WAREHOUSE:
            case STRUCTURE_TYPE.APOTHECARY:
            case STRUCTURE_TYPE.RAIDER_CAMP:
            case STRUCTURE_TYPE.HUNTER_LODGE:
            case STRUCTURE_TYPE.ASSASSIN_GUILD:
            case STRUCTURE_TYPE.DEMONIC_PRISON:
            case STRUCTURE_TYPE.TORTURE_CHAMBERS:
            case STRUCTURE_TYPE.MAGE_TOWER:
            case STRUCTURE_TYPE.ABANDONED_MINE:
            case STRUCTURE_TYPE.LUMBERYARD:
            case STRUCTURE_TYPE.MINE_SHACK:
            case STRUCTURE_TYPE.MAGE_QUARTERS:
            case STRUCTURE_TYPE.EYE:
            case STRUCTURE_TYPE.CRYPT:
            case STRUCTURE_TYPE.OSTRACIZER:
            case STRUCTURE_TYPE.MEDDLER:
            case STRUCTURE_TYPE.KENNEL:
            case STRUCTURE_TYPE.CAVE:
            case STRUCTURE_TYPE.DEFILER:
            case STRUCTURE_TYPE.RUINED_ZOO:
                return true;
            default:
                return false;
        }
    }
    public static bool IsSpecialStructure(this STRUCTURE_TYPE structureType) {
        switch (structureType) {
            case STRUCTURE_TYPE.MAGE_TOWER:
            case STRUCTURE_TYPE.ABANDONED_MINE:
            case STRUCTURE_TYPE.ANCIENT_GRAVEYARD:
            case STRUCTURE_TYPE.ANCIENT_RUIN:
            case STRUCTURE_TYPE.MONSTER_LAIR:
            case STRUCTURE_TYPE.CAVE:
            case STRUCTURE_TYPE.TEMPLE:
            case STRUCTURE_TYPE.RUINED_ZOO:
                return true;
            default:
                return false;
        }
    }
    public static LANDMARK_TYPE GetLandmarkType(this STRUCTURE_TYPE structureType) {
        if (System.Enum.TryParse(structureType.ToString(), out LANDMARK_TYPE parsed)) {
            return parsed;
        } else {
            return LANDMARK_TYPE.HOUSES;
        }
    }
    #endregion

    #region Misc
    public static Cardinal_Direction OppositeDirection(this Cardinal_Direction dir) {
        switch (dir) {
            case Cardinal_Direction.North:
                return Cardinal_Direction.South;
            case Cardinal_Direction.South:
                return Cardinal_Direction.North;
            case Cardinal_Direction.East:
                return Cardinal_Direction.West;
            case Cardinal_Direction.West:
                return Cardinal_Direction.East;
        }
        throw new System.Exception($"No opposite direction for {dir}");
    }
    public static bool IsCardinalDirection(this GridNeighbourDirection dir) {
        switch (dir) {
            case GridNeighbourDirection.North:
            case GridNeighbourDirection.South:
            case GridNeighbourDirection.West:
            case GridNeighbourDirection.East:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Actions
    /// <summary>
    /// Is this action type considered to be a hostile action.
    /// </summary>
    /// <returns>True or false.</returns>
    public static bool IsHostileAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT:
            case INTERACTION_TYPE.STEAL:
            case INTERACTION_TYPE.RESTRAIN_CHARACTER:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// Will the given action type directly make it's actor enter combat state.
    /// </summary>
    /// <param name="type">The type of action.</param>
    /// <returns>True or false</returns>
    public static bool IsDirectCombatAction(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.ASSAULT:
                return true;
            default:
                return false;
        }
    }
    public static bool CanBeReplaced(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.DRINK:
            case INTERACTION_TYPE.EAT:
            case INTERACTION_TYPE.SIT:
                return true;
            default:
                return false;
        }
    }
    public static bool WillAvoidCharactersWhileMoving(this INTERACTION_TYPE type) {
        switch (type) {
            case INTERACTION_TYPE.RITUAL_KILLING:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region State
    public static bool IsCombatState(this CHARACTER_STATE type) {
        switch (type) {
            case CHARACTER_STATE.BERSERKED:
            case CHARACTER_STATE.COMBAT:
            case CHARACTER_STATE.HUNT:
                return true;
            default:
                return false;
        }
    }
    /// <summary>
    /// This is used to determine what class should be created when saving a CharacterState. <see cref="SaveUtilities.CreateCharacterStateSaveDataInstance(CharacterState)"/>
    /// </summary>
    /// <param name="type">The type of state</param>
    /// <returns>If the state type has a unique save data class or not.</returns>
    public static bool HasUniqueSaveData(this CHARACTER_STATE type) {
        switch (type) {
            case CHARACTER_STATE.DOUSE_FIRE:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Furniture
    public static TILE_OBJECT_TYPE ConvertFurnitureToTileObject(this FURNITURE_TYPE type) {
        TILE_OBJECT_TYPE to;
        if (System.Enum.TryParse<TILE_OBJECT_TYPE>(type.ToString(), out to)) {
            return to;
        }
        return TILE_OBJECT_TYPE.NONE;
    }
    public static bool CanBeCraftedBy(this TILE_OBJECT_TYPE type, Character character) {
        if (type == TILE_OBJECT_TYPE.NONE) {
            return false;
        }
        TileObjectData data = TileObjectDB.GetTileObjectData(type);
        if (data.neededCharacterClass == null || data.neededCharacterClass.Length <= 0) {
            return true;
        }
        return data.neededCharacterClass.Contains(character.characterClass.className);
    }
    #endregion

    #region Tile Objects
    public static FURNITURE_TYPE ConvertTileObjectToFurniture(this TILE_OBJECT_TYPE type) {
        FURNITURE_TYPE to;
        if (System.Enum.TryParse<FURNITURE_TYPE>(type.ToString(), out to)) {
            return to;
        }
        return FURNITURE_TYPE.NONE;
    }
    public static bool CanProvideFacility(this TILE_OBJECT_TYPE tileObj, FACILITY_TYPE facility) {
        TileObjectData data;
        if (TileObjectDB.TryGetTileObjectData(tileObj, out data)) {
            return data.CanProvideFacility(facility);
        }
        return false;
    }
    public static bool IsPreBuilt(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.TABLE:
            case TILE_OBJECT_TYPE.BED:
            case TILE_OBJECT_TYPE.BED_CLINIC:
            case TILE_OBJECT_TYPE.DESK:
            case TILE_OBJECT_TYPE.GUITAR:
            case TILE_OBJECT_TYPE.TABLE_ARMOR:
            case TILE_OBJECT_TYPE.TABLE_ALCHEMY:
            case TILE_OBJECT_TYPE.TABLE_SCROLLS:
            case TILE_OBJECT_TYPE.TABLE_WEAPONS:
            case TILE_OBJECT_TYPE.TABLE_MEDICINE:
            case TILE_OBJECT_TYPE.TABLE_CONJURING:
            case TILE_OBJECT_TYPE.TABLE_HERBALISM:
            case TILE_OBJECT_TYPE.TABLE_METALWORKING_TOOLS:
            case TILE_OBJECT_TYPE.PLINTH_BOOK:
                return false;
            default:
                return true;
        }
    }
    public static bool IsTileObjectAnItem(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.HEALING_POTION:
            case TILE_OBJECT_TYPE.TOOL:
            case TILE_OBJECT_TYPE.ARTIFACT:
            case TILE_OBJECT_TYPE.CULTIST_KIT:
            case TILE_OBJECT_TYPE.ANTIDOTE:
            case TILE_OBJECT_TYPE.WATER_FLASK:
            case TILE_OBJECT_TYPE.EMBER:
            case TILE_OBJECT_TYPE.HERB_PLANT:
            case TILE_OBJECT_TYPE.POISON_FLASK:
            case TILE_OBJECT_TYPE.POISON_CRYSTAL:
            case TILE_OBJECT_TYPE.FIRE_CRYSTAL:
            case TILE_OBJECT_TYPE.WATER_CRYSTAL:
            case TILE_OBJECT_TYPE.ELECTRIC_CRYSTAL:
            case TILE_OBJECT_TYPE.ICE_CRYSTAL:
            case TILE_OBJECT_TYPE.ICE:
            case TILE_OBJECT_TYPE.EXCALIBUR:
                return true;
            default:
                return false;
        }
    }
    public static bool IsTileObjectVisibleByDefault(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.TOMBSTONE:
            case TILE_OBJECT_TYPE.TREASURE_CHEST:
            case TILE_OBJECT_TYPE.EXCALIBUR:
            case TILE_OBJECT_TYPE.HEIRLOOM:
                return true;
            default:
                return tileObjectType.IsTileObjectAnItem();
        }
    }
    public static bool CanBeRepaired(this TILE_OBJECT_TYPE tileObjectType) {
        switch (tileObjectType) {
            case TILE_OBJECT_TYPE.BED:
            case TILE_OBJECT_TYPE.TABLE:
            case TILE_OBJECT_TYPE.DESK:
            case TILE_OBJECT_TYPE.GUITAR:
            case TILE_OBJECT_TYPE.TORCH:
            case TILE_OBJECT_TYPE.WATER_WELL:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Jobs
    public static bool IsNeedsTypeJob(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.ENERGY_RECOVERY_URGENT:
            case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
            case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
            case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
            case JOB_TYPE.HAPPINESS_RECOVERY:
            case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
                return true;
            default:
                return false;
        }
    }
    public static int GetJobTypePriority(this JOB_TYPE jobType) {
        int priority = 0;
        switch (jobType) {
            case JOB_TYPE.DIG_THROUGH:
                priority = 1300;
                break;
            case JOB_TYPE.FLEE_TO_HOME:
                priority = 1200;
                break;
            case JOB_TYPE.NEUTRALIZE_DANGER:
                priority = 1100;
                break;
            case JOB_TYPE.COMBAT:
                priority = 1090;
                break;
            case JOB_TYPE.FLEE_CRIME:
            case JOB_TYPE.BERSERK_ATTACK:
            case JOB_TYPE.BERSERK_STROLL:
                priority = 1088;
                break;
            case JOB_TYPE.NO_PATH_IDLE:
                priority = 1087;
                break;
            case JOB_TYPE.FULLNESS_RECOVERY_ON_SIGHT:
                priority = 1086;
                break;
            //case JOB_TYPE.FLEE_CRIME:
            //case JOB_TYPE.BERSERK_ATTACK:
            case JOB_TYPE.DESTROY:
            //case JOB_TYPE.BERSERK_STROLL:
            ////case JOB_TYPE.GO_TO:
                priority = 1085;
                break;
            case JOB_TYPE.REPORT_CORRUPTED_STRUCTURE:
            case JOB_TYPE.COUNTERATTACK:
                priority = 1080;
                break;
            case JOB_TYPE.TRIGGER_FLAW:
                priority = 1050;
                break;
            case JOB_TYPE.RETURN_HOME_URGENT:
                priority = 1055;
                break;
            case JOB_TYPE.HIDE_AT_HOME:
            case JOB_TYPE.SEEK_SHELTER:
                priority = 1040;
                break;
            case JOB_TYPE.SCREAM:
                priority = 1020;
                break;
            case JOB_TYPE.RELEASE_CHARACTER:
                priority = 1015;
                break;
            case JOB_TYPE.BURY_SERIAL_KILLER_VICTIM:
                priority = 1010;
                break;
            case JOB_TYPE.REMOVE_STATUS:
                priority = 1008;
                break;
            case JOB_TYPE.RECOVER_HP:
                priority = 1005;
                break;
            case JOB_TYPE.FEED:
            case JOB_TYPE.RITUAL_KILLING:
                priority = 1003;
                break;
            case JOB_TYPE.ENERGY_RECOVERY_URGENT:
            case JOB_TYPE.FULLNESS_RECOVERY_URGENT:
            case JOB_TYPE.HUNT_PREY:
                priority = 1000;
                break;
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.BRAWL:
            case JOB_TYPE.ABSORB_LIFE:
            case JOB_TYPE.ABSORB_POWER:
            case JOB_TYPE.SPAWN_SKELETON:
            case JOB_TYPE.RAISE_CORPSE:
            case JOB_TYPE.HOARD:
                priority = 970;
                break;
            case JOB_TYPE.DOUSE_FIRE:
            case JOB_TYPE.SUICIDE_FOLLOW:
                priority = 950;
                break;
            case JOB_TYPE.DEMON_KILL:
                priority = 930;
                break;
            case JOB_TYPE.MOVE_CHARACTER:
                priority = 926;
                break;
            case JOB_TYPE.GO_TO:
                priority = 925;
                break;
            case JOB_TYPE.ZOMBIE_STROLL:
                priority = 915;
                break;
            //case JOB_TYPE.RECOVER_HP:
            //    priority = 920;
            //    break;
            case JOB_TYPE.UNDERMINE:
            case JOB_TYPE.POISON_FOOD:
            case JOB_TYPE.PLACE_TRAP:
            case JOB_TYPE.OPEN_CHEST:
            case JOB_TYPE.ROAM_AROUND_STRUCTURE:
                priority = 910;
                break;
            //case JOB_TYPE.FEED:
            case JOB_TYPE.MONSTER_INVADE:
                priority = 900;
                break;
            case JOB_TYPE.RESTRAIN:
                priority = 970;
                break;
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.REPORT_CRIME:
            //case JOB_TYPE.BURY:
                priority = 870;
                break;
            case JOB_TYPE.BUILD_BLUEPRINT:
            case JOB_TYPE.PLACE_BLUEPRINT:
            case JOB_TYPE.SPAWN_LAIR:
                priority = 850;
                break;
            case JOB_TYPE.SABOTAGE_NEIGHBOUR:
            case JOB_TYPE.DECREASE_MOOD:
            case JOB_TYPE.DISABLE:
            case JOB_TYPE.ARSON:
            case JOB_TYPE.DARK_RITUAL:
            case JOB_TYPE.CULTIST_TRANSFORM:
            case JOB_TYPE.CULTIST_POISON:
            case JOB_TYPE.CULTIST_BOOBY_TRAP:
            case JOB_TYPE.EVANGELIZE:
            case JOB_TYPE.SNATCH:
                priority = 830;
                break;
            case JOB_TYPE.BURY:
                priority = 820;
                break;
            case JOB_TYPE.PRODUCE_FOOD:
            case JOB_TYPE.PRODUCE_FOOD_FOR_CAMP:
            case JOB_TYPE.PRODUCE_METAL:
            case JOB_TYPE.PRODUCE_STONE:
            case JOB_TYPE.PRODUCE_WOOD:
            case JOB_TYPE.MONSTER_BUTCHER:
                priority = 800;
                break;
            case JOB_TYPE.CRAFT_OBJECT:
                priority = 750;
                break;
            case JOB_TYPE.HAUL:
                priority = 700;
                break;
            case JOB_TYPE.REPAIR:
                priority = 650;
                break;
            case JOB_TYPE.CLEANSE_TILES:
                priority = 630;
                break;
            case JOB_TYPE.CLEANSE_CORRUPTION:
            case JOB_TYPE.RECRUIT:
                priority = 600;
                break;
            case JOB_TYPE.JUDGE_PRISONER:
                priority = 570;
                break;
            //case JOB_TYPE.APPREHEND:
            //    priority = 550;
            //    break;
            case JOB_TYPE.KIDNAP:
            case JOB_TYPE.KIDNAP_RAID:
            case JOB_TYPE.STEAL_RAID:
                priority = 530;
                break;
            //case JOB_TYPE.MOVE_CHARACTER:
            //    priority = 520;
            //    break;
            case JOB_TYPE.TAKE_ITEM:
            case JOB_TYPE.INSPECT:
                priority = 510;
                break;
            case JOB_TYPE.CONFIRM_RUMOR:
            case JOB_TYPE.SHARE_NEGATIVE_INFO:
                priority = 505;
                break;
            case JOB_TYPE.ENERGY_RECOVERY_NORMAL:
            case JOB_TYPE.FULLNESS_RECOVERY_NORMAL:
            case JOB_TYPE.HAPPINESS_RECOVERY:
                priority = 500;
                break;
            case JOB_TYPE.DROP_ITEM_PARTY:
                priority = 495;
                break;
            case JOB_TYPE.PARTY_GO_TO:
            case JOB_TYPE.GO_TO_WAITING:
            case JOB_TYPE.PARTYING:
            case JOB_TYPE.IDLE_CAMP:
                priority = 490;
                break;
            case JOB_TYPE.PATROL:
            case JOB_TYPE.EXPLORE:
            case JOB_TYPE.EXTERMINATE:
            case JOB_TYPE.COUNTERATTACK_PARTY:
            case JOB_TYPE.RESCUE:
            case JOB_TYPE.JOIN_GATHERING:
            case JOB_TYPE.RAID:
            case JOB_TYPE.HOST_SOCIAL_PARTY:
            case JOB_TYPE.HUNT_HEIRLOOM:
                priority = 450;
                break;
            case JOB_TYPE.MINE:
            case JOB_TYPE.TEND_FARM:
                priority = 440;
                break;
            case JOB_TYPE.DRY_TILES:
                priority = 430;
                break;
            case JOB_TYPE.CHECK_PARALYZED_FRIEND:
                priority = 400;
                break;
            case JOB_TYPE.OBTAIN_PERSONAL_FOOD:
                priority = 300;
                break;
            case JOB_TYPE.VISIT_FRIEND:
            case JOB_TYPE.VISIT_DIFFERENT_REGION:
                priority = 280;
                break;
            case JOB_TYPE.SPREAD_RUMOR:
            //case JOB_TYPE.CONFIRM_RUMOR:
            //case JOB_TYPE.SHARE_NEGATIVE_INFO:
                priority = 270;
                break;
            case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.LEARN_MONSTER:
            case JOB_TYPE.TAKE_ARTIFACT:
                priority = 260;
                break;
            case JOB_TYPE.IDLE_RETURN_HOME:
            case JOB_TYPE.IDLE_NAP:
            case JOB_TYPE.IDLE_SIT:
            case JOB_TYPE.IDLE_STAND:
            case JOB_TYPE.IDLE_GO_TO_INN:
            case JOB_TYPE.IDLE:
            case JOB_TYPE.ROAM_AROUND_TERRITORY:
            case JOB_TYPE.ROAM_AROUND_CORRUPTION:
            case JOB_TYPE.ROAM_AROUND_PORTAL:
            case JOB_TYPE.ROAM_AROUND_TILE:
            case JOB_TYPE.RETURN_TERRITORY:
            case JOB_TYPE.RETURN_PORTAL:
            case JOB_TYPE.STAND:
            case JOB_TYPE.STAND_STILL:
            case JOB_TYPE.DROP_ITEM:
            case JOB_TYPE.CRAFT_MISSING_FURNITURE:
            case JOB_TYPE.WARM_UP:
                priority = 250;
                break;
            case JOB_TYPE.COMBINE_STOCKPILE:
                priority = 200;
                break;
            case JOB_TYPE.COMMIT_SUICIDE:
                priority = 150;
                break;
            case JOB_TYPE.STROLL:
                priority = 100;
                break;
            case JOB_TYPE.MONSTER_ABDUCT:
            case JOB_TYPE.MONSTER_EAT:
                priority = 90;
                break;
            // case JOB_TYPE.SNUFF_TORNADO:
            // case JOB_TYPE.INTERRUPTION:
            //     priority = 2;
            //     break;
            // case JOB_TYPE.COMBAT:
            //     priority = 3;
            //     break;
            // case JOB_TYPE.TRIGGER_FLAW:
            //     priority = 4;
            //     break;
            // case JOB_TYPE.MISC:
            // case JOB_TYPE.CORRUPT_CULTIST:
            // case JOB_TYPE.DESTROY_FOOD:
            // case JOB_TYPE.DESTROY_SUPPLY:
            // case JOB_TYPE.SABOTAGE_FACTION:
            // case JOB_TYPE.SCREAM:
            // case JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM:
            //     //case JOB_TYPE.INTERRUPTION:
            //     priority = 5;
            //     break;
            // case JOB_TYPE.TANTRUM:
            // case JOB_TYPE.CLAIM_REGION:
            // case JOB_TYPE.CLEANSE_REGION:
            // case JOB_TYPE.ATTACK_DEMONIC_REGION:
            // case JOB_TYPE.ATTACK_NON_DEMONIC_REGION:
            // case JOB_TYPE.INVADE_REGION:
            //     priority = 6;
            //     break;
            // // case JOB_TYPE.IDLE:
            // //     priority = 7;
            // //     break;
            // case JOB_TYPE.DEATH:
            // case JOB_TYPE.BERSERK:
            // case JOB_TYPE.STEAL:
            // case JOB_TYPE.RESOLVE_CONFLICT:
            // case JOB_TYPE.DESTROY:
            //     priority = 10;
            //     break;
            // case JOB_TYPE.KNOCKOUT:
            // case JOB_TYPE.SEDUCE:
            // case JOB_TYPE.UNDERMINE_ENEMY:
            //     priority = 20;
            //     break;
            // case JOB_TYPE.HUNGER_RECOVERY_STARVING:
            // case JOB_TYPE.TIREDNESS_RECOVERY_EXHAUSTED:
            //     priority = 30;
            //     break;
            // case JOB_TYPE.APPREHEND:
            // case JOB_TYPE.DOUSE_FIRE:
            //     priority = 40;
            //     break;
            // case JOB_TYPE.REMOVE_TRAIT:
            //     priority = 50;
            //     break;
            // case JOB_TYPE.RESTRAIN:
            //     priority = 60;
            //     break;
            // case JOB_TYPE.HAPPINESS_RECOVERY_FORLORN:
            //     priority = 100;
            //     break;
            // case JOB_TYPE.FEED:
            //     priority = 110;
            //     break;
            // case JOB_TYPE.BURY:
            // case JOB_TYPE.REPAIR:
            // case JOB_TYPE.WATCH:
            // case JOB_TYPE.DESTROY_PROFANE_LANDMARK:
            // case JOB_TYPE.PERFORM_HOLY_INCANTATION:
            // case JOB_TYPE.PRAY_GODDESS_STATUE:
            // case JOB_TYPE.REACT_TO_SCREAM:
            //     priority = 120;
            //     break;
            // case JOB_TYPE.BREAK_UP:
            //     priority = 130;
            //     break;
            // case JOB_TYPE.PATROL:
            //     priority = 170;
            //     break;
            // case JOB_TYPE.JUDGEMENT:
            //     priority = 220;
            //     break;
            // case JOB_TYPE.SUICIDE:
            // case JOB_TYPE.HAUL:
            //     priority = 230;
            //     break;
            // case JOB_TYPE.CRAFT_OBJECT:
            // case JOB_TYPE.PRODUCE_FOOD:
            // case JOB_TYPE.PRODUCE_WOOD:
            // case JOB_TYPE.PRODUCE_STONE:
            // case JOB_TYPE.PRODUCE_METAL:
            // case JOB_TYPE.TAKE_PERSONAL_FOOD:
            // case JOB_TYPE.DROP:
            // case JOB_TYPE.INSPECT:
            // case JOB_TYPE.PLACE_BLUEPRINT:
            // case JOB_TYPE.BUILD_BLUEPRINT:
            // case JOB_TYPE.OBTAIN_PERSONAL_ITEM:
            //     priority = 240;
            //     break;
            // case JOB_TYPE.HUNGER_RECOVERY:
            // case JOB_TYPE.TIREDNESS_RECOVERY:
            // case JOB_TYPE.HAPPINESS_RECOVERY:
            //     priority = 270;
            //     break;
            // case JOB_TYPE.STROLL:
            // case JOB_TYPE.IDLE:
            //     priority = 290;
            //     break;
            // case JOB_TYPE.IMPROVE:
            // case JOB_TYPE.EXPLORE:
            //     priority = 300;
            //     break;
        }
        return priority;
    }
    public static bool IsJobLethal(this JOB_TYPE type) {
        switch (type) {
            case JOB_TYPE.APPREHEND:
            case JOB_TYPE.RITUAL_KILLING:
            case JOB_TYPE.KNOCKOUT:
            case JOB_TYPE.ABDUCT:
            case JOB_TYPE.LEARN_MONSTER:
            case JOB_TYPE.BRAWL:
            case JOB_TYPE.KIDNAP:
            case JOB_TYPE.MOVE_CHARACTER:
            case JOB_TYPE.RESTRAIN:
            case JOB_TYPE.KIDNAP_RAID:
                return false;
            default:
                return true;
        }
    }
    #endregion

    #region Summons
    public static string SummonName(this SUMMON_TYPE type) {
        switch (type) {
            default:
                return UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetters(type.ToString());
        }
    }
    #endregion

    #region Artifacts
    public static bool CanBeSummoned(this ARTIFACT_TYPE type) {
        switch (type) {
            case ARTIFACT_TYPE.None:
                return true;
            default:
                return false;
        }
    }
    #endregion

    #region Intervention Abilities
    public static List<ABILITY_TAG> GetAbilityTags(this SPELL_TYPE type) {
        List<ABILITY_TAG> tags = new List<ABILITY_TAG>();
        switch (type) {
            case SPELL_TYPE.LYCANTHROPY:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case SPELL_TYPE.KLEPTOMANIA:
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case SPELL_TYPE.VAMPIRISM:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            case SPELL_TYPE.UNFAITHFULNESS:
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case SPELL_TYPE.CANNIBALISM:
                tags.Add(ABILITY_TAG.MAGIC);
                tags.Add(ABILITY_TAG.CRIME);
                break;
            case SPELL_TYPE.ZAP:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            //case SPELL_TYPE.JOLT:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
            //case SPELL_TYPE.ENRAGE:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
            //case SPELL_TYPE.PROVOKE:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
            case SPELL_TYPE.RAISE_DEAD:
                tags.Add(ABILITY_TAG.MAGIC);
                break;
            //case SPELL_TYPE.CLOAK_OF_INVISIBILITY:
            //    tags.Add(ABILITY_TAG.MAGIC);
            //    break;
        }
        return tags;
    }
    #endregion

    #region Landmarks
    public static bool IsPlayerLandmark(this LANDMARK_TYPE type) {
        switch (type) {
            case LANDMARK_TYPE.THE_PORTAL:
            case LANDMARK_TYPE.OSTRACIZER:
            case LANDMARK_TYPE.CRYPT:
            case LANDMARK_TYPE.KENNEL:
            case LANDMARK_TYPE.THE_ANVIL:
            case LANDMARK_TYPE.MEDDLER:
            case LANDMARK_TYPE.EYE:
            case LANDMARK_TYPE.DEFILER:
            case LANDMARK_TYPE.THE_NEEDLES:
            case LANDMARK_TYPE.TORTURE_CHAMBERS:
                return true;
            default:
                return false;
        }
    }
    public static STRUCTURE_TYPE GetStructureType(this LANDMARK_TYPE landmarkType) {
        switch (landmarkType) {
            case LANDMARK_TYPE.HOUSES:
                return STRUCTURE_TYPE.DWELLING;
            case LANDMARK_TYPE.VILLAGE:
                return STRUCTURE_TYPE.CITY_CENTER;
            default:
                STRUCTURE_TYPE parsed;
                if (System.Enum.TryParse(landmarkType.ToString(), out parsed)) {
                    return parsed;
                } else {
                    throw new System.Exception($"There is no corresponding structure type for {landmarkType.ToString()}");
                }
        }
        
    }
    #endregion

    #region Deadly Sins
    public static string Description(this DEADLY_SIN_ACTION sin) {
        switch (sin) {
            case DEADLY_SIN_ACTION.SPELL_SOURCE:
                return "Knows three Spells that can be extracted by the Ruinarch";
            case DEADLY_SIN_ACTION.INSTIGATOR:
                return "Can be assigned to spawn Chaos Events in The Fingers";
            case DEADLY_SIN_ACTION.BUILDER:
                return "Can construct demonic structures";
            case DEADLY_SIN_ACTION.SABOTEUR:
                return "Can interfere in Events spawned by non-combatant characters";
            case DEADLY_SIN_ACTION.INVADER:
                return "Can invade adjacent regions";
            case DEADLY_SIN_ACTION.FIGHTER:
                return "Can interfere in Events spawned by combat-ready characters";
            case DEADLY_SIN_ACTION.RESEARCHER:
                return "Can be assigned to research upgrades in The Anvil";
            default:
                return string.Empty;
        }
    }
    #endregion

    #region Combat Abilities
    public static string Description(this COMBAT_ABILITY ability) {
        switch (ability) {
            case COMBAT_ABILITY.SINGLE_HEAL:
                return "Heals a friendly unit by a percentage of its max HP.";
            case COMBAT_ABILITY.FLAMESTRIKE:
                return "Deal AOE damage in the surrounding npcSettlement.";
            case COMBAT_ABILITY.FEAR_SPELL:
                return "Makes a character fear any other character.";
            case COMBAT_ABILITY.SACRIFICE:
                return "Sacrifice a friendly unit to deal AOE damage in the surrounding npcSettlement.";
            case COMBAT_ABILITY.TAUNT:
                return "Taunts enemies into attacking this character.";
            default:
                return string.Empty;
        }

    }
    #endregion

    #region Races
    public static bool UsesGenderNeutralMarkerAssets(this RACE race) {
        switch (race) {
            // case RACE.HUMANS:
            // case RACE.ELVES:
            case RACE.LESSER_DEMON:
                return false;
            default:
                return true;
        }
    }
    public static bool UsesGenderNeutralPortrait(this RACE race) {
        switch (race) {
            case RACE.HUMANS:
            case RACE.ELVES:
            case RACE.LESSER_DEMON:
                return false;
            default:
                return true;
        }
    }
    #endregion

    #region Faction
    public static string FactionRelationshipColor(this FACTION_RELATIONSHIP_STATUS relationshipStatus) {
        switch (relationshipStatus) {
            case FACTION_RELATIONSHIP_STATUS.Friendly:
                return "#19ff00";
            case FACTION_RELATIONSHIP_STATUS.Hostile:
                return "#ff0000";
            default:
                return "#F8E1A9";
        }
    }
    #endregion

    #region Tiles
    public static bool IsStructureType(this LocationGridTile.Ground_Type groundType) {
        switch (groundType) {
            case LocationGridTile.Ground_Type.Cobble:
            case LocationGridTile.Ground_Type.Wood:
            case LocationGridTile.Ground_Type.Structure_Stone:
            case LocationGridTile.Ground_Type.Ruined_Stone:
            case LocationGridTile.Ground_Type.Demon_Stone:
            case LocationGridTile.Ground_Type.Flesh:
            case LocationGridTile.Ground_Type.Cave:
            case LocationGridTile.Ground_Type.Corrupted:
            case LocationGridTile.Ground_Type.Bone:
                return true;
            default:
                return false;
        }
    }
    #endregion
}
