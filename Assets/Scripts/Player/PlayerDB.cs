﻿using System.Collections.Generic;

public static class PlayerDB {
    public const int MAX_LEVEL_SUMMON = 3;
    public const int MAX_LEVEL_ARTIFACT = 3;
    public const int MAX_LEVEL_COMBAT_ABILITY = 3;
    public const int MAX_LEVEL_INTERVENTION_ABILITY = 3;
    public const int DIVINE_INTERVENTION_DURATION = 2880; //4320;
    public const int MAX_INTEL = 3;
    public const int MAX_MINIONS = 7;
    public const int MAX_INTERVENTION_ABILITIES = 4;
    
    //actions
    public const string Zap_Action = "Zap";
    public const string Summon_Minion_Action = "Summon Minion";
    public const string Poison_Action = "Poison";
    public const string Ignite_Action = "Ignite";
    public const string Destroy_Action = "Destroy";
    public const string Corrupt_Action = "Corrupt";
    public const string Build_Demonic_Structure_Action = "Build Demonic Structure";
    public const string Animate_Action = "Animate";
    public const string Afflict_Action = "Afflict";
    public const string Seize_Character_Action = "Seize Character";
    public const string Seize_Object_Action = "Seize Object";
    public const string Bless_Action = "Bless";
    public const string Booby_Trap_Action = "Booby Trap";
    public const string Torture_Action = "Torture";
    public const string Interfere_Action = "Interfere";
    public const string Learn_Spell_Action = "Learn Spell";
    public const string Stop_Action = "Stop";
    public const string Return_To_Portal_Action = "Return To Portal";

    //spells
    public const string Tornado = "Tornado";
    public const string Meteor = "Meteor";
    public const string Poison_Cloud = "Poison Cloud";
    public const string Lightning = "Lightning";
    public const string Ravenous_Spirit = "Ravenous Spirit";
    public const string Feeble_Spirit = "Feeble Spirit";
    public const string Forlorn_Spirit = "Forlorn Spirit";
    public const string Locust_Swarm = "Locust Swarm";
    public const string Spawn_Boulder = "Spawn Boulder";
    public const string Landmine = "Landmine";
    public const string Manifest_Food = "Manifest Food";
    public const string Brimstones = "Brimstones";
    public const string Acid_Rain = "Acid Rain";
    public const string Rain = "Rain";
    public const string Heat_Wave = "Heat Wave";
    public const string Wild_Growth = "Wild Growth";
    public const string Spider_Rain = "Spider Rain";
    public const string Blizzard = "Blizzard";
    public const string Earthquake = "Earthquake";
    public const string Fertility = "Fertility";
    public const string Spawn_Bandit_Camp = "Spawn Bandit Camp";
    public const string Spawn_Monster_Lair = "Spawn Monster Lair";
    public const string Spawn_Haunted_Grounds = "Spawn Haunted Grounds";
    
    
    public static List<string> spells = new List<string>() { 
        Tornado, Meteor, Poison_Cloud, Lightning, 
        Ravenous_Spirit, Feeble_Spirit, Forlorn_Spirit, 
        Locust_Swarm, Spawn_Boulder, Landmine, Manifest_Food, 
        Brimstones, Acid_Rain, Rain, Heat_Wave, Wild_Growth, 
        Spider_Rain, Blizzard, Earthquake, Fertility, 
        Spawn_Bandit_Camp, Spawn_Monster_Lair, Spawn_Haunted_Grounds 
    };

    public static List<SPELL_TYPE> afflictions = new List<SPELL_TYPE>() { 
        SPELL_TYPE.PARALYSIS, SPELL_TYPE.UNFAITHFULNESS, SPELL_TYPE.KLEPTOMANIA, SPELL_TYPE.AGORAPHOBIA, 
        SPELL_TYPE.PSYCHOPATHY, SPELL_TYPE.PESTILENCE, SPELL_TYPE.LYCANTHROPY, 
        SPELL_TYPE.VAMPIRISM, SPELL_TYPE.ZOMBIE_VIRUS, SPELL_TYPE.CURSED_OBJECT, SPELL_TYPE.LULLABY, 
    };
}
