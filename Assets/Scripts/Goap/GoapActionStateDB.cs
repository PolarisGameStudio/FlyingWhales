﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public static Dictionary<INTERACTION_TYPE, StateNameAndDuration[]> goapActionStates = new Dictionary<INTERACTION_TYPE, StateNameAndDuration[]>() {
        {INTERACTION_TYPE.EAT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.RELEASE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Release Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ASSAULT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Combat Start", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.MINE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Mine Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.SLEEP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(8) },
        } },
        {INTERACTION_TYPE.PICK_UP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DAYDREAM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Daydream Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Daydream Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PLAY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Play Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PLAY_GUITAR, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Play Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Play Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CHAT_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Quick Chat", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Share Information", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Friends", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Enemies", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Argument", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Flirt", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Lovers", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Become Paramours", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Resolve Enmity", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RETURN_HOME, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Return Home Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DRINK, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Drink Poisoned", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SLEEP_OUTSIDE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Rest Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(8) },
            new StateNameAndDuration(){ name = "Rest Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REMOVE_POISON, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Remove Poison Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.POISON, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Poison Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Poison Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PRAY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Pray Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CHOP_WOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Chop Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.MAGIC_CIRCLE_PERFORM_RITUAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Perform Ritual Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Perform Ritual Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STEAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Steal Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SCRAP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Scrap Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.GET_SUPPLY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_RESOURCE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_FOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.OBTAIN_RESOURCE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Take Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Take Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TILE_OBJECT_DESTROY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "In Progress", status = InteractionManager.Goap_State_Success, duration = -1 },
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ITEM_DESTROY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RETURN_HOME_LOCATION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Return Home Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Return Home Failed", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REVERT_TO_NORMAL_FORM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RESTRAIN_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Restrain Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.FIRST_AID_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "First Aid Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CURE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Cure Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CURSE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Curse Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DISPEL_MAGIC, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Dispel Magic Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.JUDGE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Target Executed", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Released", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Exiled", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Whip", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.REPORT_CRIME, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Report Crime Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Report Crime Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.FEED, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Feed Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_ITEM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP_ITEM_WAREHOUSE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_SAVE_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STAND, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Stand Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.SIT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Sit Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Sit Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.NAP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Nap Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.BURY_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Bury Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CARRY_CORPSE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REMEMBER_FALLEN, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Remember Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_ITEM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        {INTERACTION_TYPE.SPIT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Spit Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REPORT_HOSTILE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Report Hostile Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Report Hostile Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.INVITE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Invite Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Invite Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.MAKE_LOVE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Make Love Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Make Love Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DRINK_BLOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drink Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Drink Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REPLACE_TILE_OBJECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Replace Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.CRAFT_FURNITURE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Craft Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.TANTRUM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Tantrum Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.EAT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Eat Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Eat Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.BREAK_UP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Break Up Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SHARE_INFORMATION, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Share Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.WATCH, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Watch Success", status = InteractionManager.Goap_State_Success, duration =  GameManager.Instance.GetTicksBasedOnHour(1) },//-1
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.INSPECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Inspect Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        { INTERACTION_TYPE.HUNTING_TO_DRINK_BLOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "In Progress", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(4) },
        } },
        { INTERACTION_TYPE.ROAMING_TO_STEAL, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "In Progress", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(4) },
        } },
        { INTERACTION_TYPE.PUKE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Puke Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.SEPTIC_SHOCK, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Septic Shock Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.ZOMBIE_DEATH, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Zombie Death Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.CARRY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Carry Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DROP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Drop Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.KNOCKOUT_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Knockout Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Knockout Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RITUAL_KILLING, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Killing Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Killing Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.RESOLVE_CONFLICT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Resolve Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Resolve Fail", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        { INTERACTION_TYPE.ACCIDENT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Accident Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        { INTERACTION_TYPE.STUMBLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Stumble Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.TRANSFORM_FOOD, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Transform Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.ASK_TO_STOP_JOB, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Ask Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Ask Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.WELL_JUMP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Well Jump Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.STRANGLE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Strangle Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.REPAIR_TILE_OBJECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Repair Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        { INTERACTION_TYPE.NARCOLEPTIC_NAP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Nap Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
        } },
        { INTERACTION_TYPE.SHOCK, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Shock Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.CRY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Cry Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        { INTERACTION_TYPE.CRY, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Cry Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.CRAFT_TILE_OBJECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Craft Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.PRAY_TILE_OBJECT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Pray Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnHour(1) },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.HAVE_AFFAIR, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Affair Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SLAY_CHARACTER, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Slay Success", status = InteractionManager.Goap_State_Success, duration = 0 },
            new StateNameAndDuration(){ name = "Slay Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.FEELING_CONCERNED, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Concerned Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.LAUGH_AT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Laugh Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.TEASE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Tease Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(10) },
        } },
        {INTERACTION_TYPE.FEELING_SPOOKED, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Spooked Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.FEELING_BROKENHEARTED, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Brokenhearted Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.GRIEVING, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Grieving Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.GO_TO, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Goto Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
        {INTERACTION_TYPE.SING, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Sing Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.DANCE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Dance Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
        } },
        {INTERACTION_TYPE.DESTROY_RESOURCE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Destroy Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.DESTROY_RESOURCE, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Destroy Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(30) },
            new StateNameAndDuration(){ name = "Destroy Fail", status = InteractionManager.Goap_State_Fail, duration = 0 },
            new StateNameAndDuration(){ name = "Target Missing", status = InteractionManager.Goap_State_Fail, duration = 0 },
        } },
        {INTERACTION_TYPE.SCREAM_FOR_HELP, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Scream Success", status = InteractionManager.Goap_State_Success, duration = GameManager.Instance.GetTicksBasedOnMinutes(15) },
        } },
        {INTERACTION_TYPE.REACT_TO_SCREAM, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "React Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
         {INTERACTION_TYPE.RESOLVE_COMBAT, new StateNameAndDuration[]{
            new StateNameAndDuration(){ name = "Combat Success", status = InteractionManager.Goap_State_Success, duration = 0 },
        } },
    };
}

public struct StateNameAndDuration {
    public string name;
    public string status;
    public int duration;
}
