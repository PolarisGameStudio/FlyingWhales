﻿using System.Collections.Generic;

public static class GoapActionStateDB {

    public static string No_Icon = "None";
    public static string Eat_Icon = "Eat";
    public static string Hostile_Icon = "Hostile";
    public static string Sleep_Icon = "Sleep";
    public static string Social_Icon = "Social";
    public static string Work_Icon = "Work";
    public static string Drink_Icon = "Drink";
    public static string Entertain_Icon = "Entertain";
    public static string Explore_Icon = "Explore";
    public static string FirstAid_Icon = "First Aid";
    public static string Flee_Icon = "Flee";
    public static string Patrol_Icon = "Patrol";
    public static string Watch_Icon = "Watch";

    public static string Drink_Blood_Icon = "Drink Blood";
    public static string Flirt_Icon = "Flirt";
    public static string Pray_Icon = "Pray";
    public static string Restrain_Icon = "Restrain";
    public static string Steal_Icon = "Steal";
    public static string Stealth_Icon = "Stealth";
   

    public static string GetStateResult(INTERACTION_TYPE goapType, string stateName) {
        if (goapActionStates.ContainsKey(goapType)) {
            StateNameAndDuration[] snd = goapActionStates[goapType];
            for (int i = 0; i < snd.Length; i++) {
                StateNameAndDuration currSND = snd[i];
                if (currSND.name == stateName) {
                    return currSND.status;
                }
            }
        }
        return string.Empty;
    }

    public static readonly Dictionary<INTERACTION_TYPE, StateNameAndDuration[]> goapActionStates = new Dictionary<INTERACTION_TYPE, StateNameAndDuration[]>() {
        {INTERACTION_TYPE.EAT, new[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.RELEASE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Release Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.ASSAULT, new[]{
            new StateNameAndDuration(){ name = "Combat Start", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.MINE_METAL, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.MINE_STONE, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.SLEEP, new[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(8) },
        } },
        {INTERACTION_TYPE.PICK_UP, new[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.DAYDREAM, new[]{
            new StateNameAndDuration(){ name = "Daydream Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.PLAY, new[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.PLAY_GUITAR, new[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.CHAT_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Chat Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.RETURN_HOME, new[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.DRINK, new[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.SLEEP_OUTSIDE, new[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(8), animationName = "Sleep Ground" },
        } },
        {INTERACTION_TYPE.REMOVE_POISON, new[]{
            new StateNameAndDuration(){ name = "Remove Poison Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.POISON, new[]{
            new StateNameAndDuration(){ name = "Poison Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.PRAY, new[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.CHOP_WOOD, new[]{
            new StateNameAndDuration(){ name = "Chop Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.STEAL, new[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.SCRAP, new[]{
            new StateNameAndDuration(){ name = "Scrap Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.DEPOSIT_RESOURCE_PILE, new[]{
            new StateNameAndDuration(){ name = "Deposit Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.DROP_RESOURCE, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.TAKE_RESOURCE, new[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.RETURN_HOME_LOCATION, new[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.REVERT_TO_NORMAL_FORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.RESTRAIN_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Restrain Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.FIRST_AID_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "First Aid Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.CURE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Cure Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.CURSE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Curse Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.DISPEL_MAGIC, new[]{
            new StateNameAndDuration(){ name = "Dispel Magic Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.JUDGE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Judge Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.FEED, new[]{
            new StateNameAndDuration(){ name = "Feed Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.DROP_ITEM, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, new[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.STAND, new[]{
            new StateNameAndDuration(){ name = "Stand Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.SIT, new[]{
            new StateNameAndDuration(){ name = "Sit Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.NAP, new[]{
            new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.BURY_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Bury Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.REMEMBER_FALLEN, new[]{
            new StateNameAndDuration(){ name = "Remember Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.CRAFT_ITEM, new[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.SPIT, new[]{
            new StateNameAndDuration(){ name = "Spit Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.INVITE, new[]{
            new StateNameAndDuration(){ name = "Invite Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.MAKE_LOVE, new[]{
            new StateNameAndDuration(){ name = "Make Love Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.DRINK_BLOOD, new[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.REPLACE_TILE_OBJECT, new[]{
            new StateNameAndDuration(){ name = "Replace Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.CRAFT_FURNITURE, new[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.TANTRUM, new[]{
            new StateNameAndDuration(){ name = "Tantrum Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.BREAK_UP, new[]{
            new StateNameAndDuration(){ name = "Break Up Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.SHARE_INFORMATION, new[]{
            new StateNameAndDuration(){ name = "Share Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.WATCH, new[]{
            new StateNameAndDuration(){ name = "Watch Success", status = InteractionManager.Goap_State_Success, duration =  GameManager.Instance.GetTicksBasedOnHour(1) },//-1
        } },
        {INTERACTION_TYPE.INSPECT, new[]{
            new StateNameAndDuration(){ name = "Inspect Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        { INTERACTION_TYPE.PUKE, new[]{
            new StateNameAndDuration(){ name = "Puke Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.SEPTIC_SHOCK, new[]{
            new StateNameAndDuration(){ name = "Septic Shock Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.ZOMBIE_DEATH, new[]{
            new StateNameAndDuration(){ name = "Zombie Death Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.CARRY, new[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.DROP, new[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.KNOCKOUT_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Knockout Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.RITUAL_KILLING, new[]{
            new StateNameAndDuration(){ name = "Killing Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.RESOLVE_CONFLICT, new[]{
            new StateNameAndDuration(){ name = "Resolve Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        { INTERACTION_TYPE.ACCIDENT, new[]{
            new StateNameAndDuration(){ name = "Accident Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        { INTERACTION_TYPE.STUMBLE, new[]{
            new StateNameAndDuration(){ name = "Stumble Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10), animationName = "Sleep Ground" },
        } },
        {INTERACTION_TYPE.BUTCHER, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.ASK_TO_STOP_JOB, new[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.WELL_JUMP, new[]{
            new StateNameAndDuration(){ name = "Well Jump Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.STRANGLE, new[]{
            new StateNameAndDuration(){ name = "Strangle Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.REPAIR, new[]{
            new StateNameAndDuration(){ name = "Repair Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.NARCOLEPTIC_NAP, new[]{
            new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1), animationName = "Sleep Ground" },
        } },
        // { INTERACTION_TYPE.SHOCK, new[]{
        //     new StateNameAndDuration(){ name = "Shock Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        // } },
        { INTERACTION_TYPE.CRY, new[]{
            new StateNameAndDuration(){ name = "Cry Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.CRAFT_TILE_OBJECT, new[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.PRAY_TILE_OBJECT, new[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.HAVE_AFFAIR, new[]{
            new StateNameAndDuration(){ name = "Affair Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.SLAY_CHARACTER, new[]{
            new StateNameAndDuration(){ name = "Slay Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.FEELING_CONCERNED, new[]{
            new StateNameAndDuration(){ name = "Concerned Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.LAUGH_AT, new[]{
            new StateNameAndDuration(){ name = "Laugh Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.TEASE, new[]{
            new StateNameAndDuration(){ name = "Tease Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.FEELING_SPOOKED, new[]{
            new StateNameAndDuration(){ name = "Spooked Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.FEELING_BROKENHEARTED, new[]{
            new StateNameAndDuration(){ name = "Brokenhearted Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.GRIEVING, new[]{
            new StateNameAndDuration(){ name = "Grieving Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.GO_TO, new[]{
            new StateNameAndDuration(){ name = "Goto Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.SING, new[]{
            new StateNameAndDuration(){ name = "Sing Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.DANCE, new[]{
            new StateNameAndDuration(){ name = "Dance Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.DESTROY_RESOURCE, new[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.SCREAM_FOR_HELP, new[]{
            new StateNameAndDuration(){ name = "Scream Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.REACT_TO_SCREAM, new[]{
            new StateNameAndDuration(){ name = "React Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.RESOLVE_COMBAT, new[]{
            new StateNameAndDuration(){ name = "Combat Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.CHANGE_CLASS, new[]{
            new StateNameAndDuration(){ name = "Change Class Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.VISIT, new[]{
            new StateNameAndDuration(){ name = "Visit Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.PLACE_BLUEPRINT, new[]{
            new StateNameAndDuration(){ name = "Place Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.BUILD_STRUCTURE, new[]{
            new StateNameAndDuration(){ name = "Build Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.STEALTH_TRANSFORM, new[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.HARVEST_PLANT, new[]{
            new StateNameAndDuration(){ name = "Harvest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.REPAIR_STRUCTURE, new[]{
            new StateNameAndDuration(){ name = "Repair Success", status = InteractionManager.Goap_State_Success, duration = 10 },
        } },
        {INTERACTION_TYPE.HARVEST_FOOD_REGION, new[]{
            new StateNameAndDuration(){ name = "Harvest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(2) },
        } },
        {INTERACTION_TYPE.CLEANSE_REGION, new[]{
            new StateNameAndDuration(){ name = "Cleanse Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(4) },
        } },
        {INTERACTION_TYPE.CLAIM_REGION, new[]{
            new StateNameAndDuration(){ name = "Claim Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(4) },
        } },
        {INTERACTION_TYPE.INVADE_REGION, new[]{
            new StateNameAndDuration(){ name = "Invade Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(4) },
        } },
        {INTERACTION_TYPE.STUDY, new[]{
            new StateNameAndDuration(){ name = "Study Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(2) },
        } },
        {INTERACTION_TYPE.FORAGE_FOOD_REGION, new[]{
            new StateNameAndDuration(){ name = "Forage Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(2) },
        } },
        {INTERACTION_TYPE.CHOP_WOOD_REGION, new[]{
            new StateNameAndDuration(){ name = "Chop Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(2) },
        } },
        {INTERACTION_TYPE.MINE_METAL_REGION, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(2) },
        } },
        {INTERACTION_TYPE.MINE_STONE_REGION, new[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(2) },
        } },
        {INTERACTION_TYPE.HOLY_INCANTATION, new[]{
            new StateNameAndDuration(){ name = "Incantation Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(6) },
        } },
        {INTERACTION_TYPE.DEMONIC_INCANTATION, new[]{
            new StateNameAndDuration(){ name = "Incantation Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(4) },
        } },
        {INTERACTION_TYPE.ATTACK_REGION, new[]{
            new StateNameAndDuration(){ name = "Attack Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(3) },
        } },
        {INTERACTION_TYPE.CORRUPT_CULTIST, new[]{
            new StateNameAndDuration(){ name = "Corrupt Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(6) },
        } },
        {INTERACTION_TYPE.OUTSIDE_SETTLEMENT_IDLE, new[]{
            new StateNameAndDuration(){ name = "Idle Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(6) },
        } },
        {INTERACTION_TYPE.SEARCHING, new[]{
            new StateNameAndDuration(){ name = "Search Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(3) },
        } },
        {INTERACTION_TYPE.SNUFF_TORNADO, new[]{
            new StateNameAndDuration(){ name = "Snuff Success", status = InteractionManager.Goap_State_Success, duration = 1 },
        } },
        {INTERACTION_TYPE.ROAM, new[]{
            new StateNameAndDuration(){ name = "Roam Success", status = InteractionManager.Goap_State_Success, duration = 3 },
        } },
    };
}

public struct StateNameAndDuration {
    public string name;
    public string status;
    public int duration;
    public string animationName;
}