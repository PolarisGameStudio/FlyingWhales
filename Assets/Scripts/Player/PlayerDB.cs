﻿using System.Collections.Generic;

public static class PlayerDB {
    public const int MAX_LEVEL_SUMMON = 3;
    public const int MAX_LEVEL_ARTIFACT = 3;
    public const int MAX_LEVEL_COMBAT_ABILITY = 3;
    public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;
    public const int DIVINE_INTERVENTION_DURATION = 2880; //4320;
    public const int MAX_INTEL = 5;
    //public const int MAX_MINIONS = 7;
    public const int MAX_INTERVENTION_ABILITIES = 4;
    
    //actions
    public const string Zap_Action = "Zap";
    //public const string Summon_Minion_Action = "Summon Minion";
    //public const string Poison_Action = "Poison";
    //public const string Ignite_Action = "Ignite";
    //public const string Destroy_Action = "Destroy";
    //public const string Corrupt_Action = "Corrupt";
    //public const string Build_Demonic_Structure_Action = "Build Demonic Structure";
    //public const string Animate_Action = "Animate";
    //public const string Afflict_Action = "Afflict";
    public const string Seize_Character_Action = "Seize Character";
    public const string Seize_Object_Action = "Seize Object";
    //public const string Bless_Action = "Bless";
    //public const string Booby_Trap_Action = "Booby Trap";
    //public const string Torture_Action = "Torture";
    //public const string Interfere_Action = "Interfere";
    //public const string Learn_Spell_Action = "Learn Spell";
    //public const string Stop_Action = "Stop";
    //public const string Return_To_Portal_Action = "Return To Portal";
    //public const string Harass_Action = "Harass";
    //public const string Raid_Action = "Raid";
    //public const string Invade_Action = "Invade";
    //public const string End_Harass_Action = "End Harass";
    //public const string End_Raid_Action = "End Raid";
    //public const string End_Invade_Action = "End Invade";
    //public const string Breed_Monster_Action = "Breed Monster";
    //public const string Activate_Artifact_Action = "Activate Artifact";
    public const string Remove_Trait_Action = "Remove Trait";
    //public const string Share_Intel_Action = "Share Intel";
    //public const string Combat_Mode_Action = "Combat Mode";
    //public const string Raise_Skeleton_Action = "Raise Skeleton";

    //spells
    //public const string Tornado = "Tornado";
    //public const string Meteor = "Meteor";
    //public const string Poison_Cloud = "Poison Cloud";
    //public const string Lightning = "Lightning";
    //public const string Ravenous_Spirit = "Ravenous Spirit";
    //public const string Feeble_Spirit = "Feeble Spirit";
    //public const string Forlorn_Spirit = "Forlorn Spirit";
    //public const string Locust_Swarm = "Locust Swarm";
    //public const string Spawn_Boulder = "Spawn Boulder";
    //public const string Landmine = "Landmine";
    //public const string Manifest_Food = "Manifest Food";
    //public const string Brimstones = "Brimstones";
    //public const string Acid_Rain = "Acid Rain";
    //public const string Rain = "Rain";
    //public const string Heat_Wave = "Heat Wave";
    //public const string Wild_Growth = "Wild Growth";
    //public const string Spider_Rain = "Spider Rain";
    //public const string Blizzard = "Blizzard";
    //public const string Earthquake = "Earthquake";
    //public const string Fertility = "Fertility";
    //public const string Spawn_Bandit_Camp = "Spawn Bandit Camp";
    //public const string Spawn_Monster_Lair = "Spawn Monster Lair";
    //public const string Spawn_Haunted_Grounds = "Spawn Haunted Grounds";
    //public const string Water_Bomb = "Water Bomb";
    //public const string Splash_Poison = "Splash Poison";


    public static List<PLAYER_SKILL_TYPE> spells = new List<PLAYER_SKILL_TYPE>() {
        PLAYER_SKILL_TYPE.TORNADO, PLAYER_SKILL_TYPE.METEOR, PLAYER_SKILL_TYPE.POISON_CLOUD, PLAYER_SKILL_TYPE.LIGHTNING,
        PLAYER_SKILL_TYPE.RAVENOUS_SPIRIT, PLAYER_SKILL_TYPE.FEEBLE_SPIRIT, PLAYER_SKILL_TYPE.FORLORN_SPIRIT,
        PLAYER_SKILL_TYPE.LOCUST_SWARM, PLAYER_SKILL_TYPE.BLIZZARD, PLAYER_SKILL_TYPE.SPAWN_BOULDER, PLAYER_SKILL_TYPE.MANIFEST_FOOD,
        PLAYER_SKILL_TYPE.BRIMSTONES, PLAYER_SKILL_TYPE.EARTHQUAKE, PLAYER_SKILL_TYPE.WATER_BOMB, PLAYER_SKILL_TYPE.SPLASH_POISON, PLAYER_SKILL_TYPE.RAIN, //Landmine, Acid_Rain, Rain, Heat_Wave, Wild_Growth, Spider_Rain, Fertility, Spawn_Bandit_Camp, Spawn_Monster_Lair, Spawn_Haunted_Grounds,
        PLAYER_SKILL_TYPE.BALL_LIGHTNING, PLAYER_SKILL_TYPE.ELECTRIC_STORM, PLAYER_SKILL_TYPE.FROSTY_FOG, PLAYER_SKILL_TYPE.VAPOR, PLAYER_SKILL_TYPE.FIRE_BALL,
        PLAYER_SKILL_TYPE.POISON_BLOOM, PLAYER_SKILL_TYPE.LANDMINE, PLAYER_SKILL_TYPE.TERRIFYING_HOWL, PLAYER_SKILL_TYPE.FREEZING_TRAP, PLAYER_SKILL_TYPE.SNARE_TRAP, PLAYER_SKILL_TYPE.WIND_BLAST,
        PLAYER_SKILL_TYPE.ICETEROIDS, PLAYER_SKILL_TYPE.HEAT_WAVE,
    };

    public static List<PLAYER_SKILL_TYPE> afflictions = new List<PLAYER_SKILL_TYPE>() { 
        PLAYER_SKILL_TYPE.PARALYSIS, PLAYER_SKILL_TYPE.UNFAITHFULNESS, PLAYER_SKILL_TYPE.KLEPTOMANIA, PLAYER_SKILL_TYPE.AGORAPHOBIA, 
        PLAYER_SKILL_TYPE.PSYCHOPATHY, PLAYER_SKILL_TYPE.PLAGUE, PLAYER_SKILL_TYPE.LYCANTHROPY, 
        PLAYER_SKILL_TYPE.VAMPIRISM/*, SPELL_TYPE.ZOMBIE_VIRUS*/, PLAYER_SKILL_TYPE.COWARDICE, PLAYER_SKILL_TYPE.PYROPHOBIA, PLAYER_SKILL_TYPE.NARCOLEPSY, PLAYER_SKILL_TYPE.GLUTTONY
    };
    
    private static string[] unlockableActions = new[] {
        Seize_Object_Action,
        Seize_Character_Action,
        Remove_Trait_Action,
        //Share_Intel_Action,
        Zap_Action,
    };
    private static string[] unlockableStructures = new[] {
        "THE_KENNEL",
        "THE_PIT",
        "TORTURE_CHAMBER",
        "THE_EYE",
        "THE_PROFANE",
    };

    public static string[] GetChoicesForUnlockableType(ARTIFACT_UNLOCKABLE_TYPE type) {
        switch (type) {
            case ARTIFACT_UNLOCKABLE_TYPE.Action:
                return unlockableActions;
            case ARTIFACT_UNLOCKABLE_TYPE.Structure:
                return unlockableStructures;
            default:
                return null;
        }
    }

}
