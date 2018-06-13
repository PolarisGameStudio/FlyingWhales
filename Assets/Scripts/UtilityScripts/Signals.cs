﻿using UnityEngine;
using System.Collections;

public static class Signals {

    public static string OBTAIN_ITEM = "OnObtainItem"; //Parameters (Character characterThatObtainedItem, Item obtainedItem)
    public static string DAY_END = "OnDayEnd";
    public static string HOUR_STARTED = "OnHourStart";
    public static string HOUR_ENDED = "OnHourEnd";
    public static string DAY_START = "OnDayStart";
    public static string FOUND_ITEM = "OnItemFound"; //Parameters (Character characterThatFoundItem, Item foundItem)
    public static string FOUND_TRACE = "OnTraceFound"; //Parameters (Character characterThatFoundTrace, string traceFound)
    public static string TASK_SUCCESS = "OnTaskSuccess"; //Parameters (Character characterThatFinishedTask, CharacterTask succeededTask)
    public static string ITEM_PLACED_LANDMARK = "OnItemPlacedAtLandmark"; //Parameters (Item item, BaseLandmark landmark)
    public static string ITEM_PLACED_INVENTORY = "OnItemPlacedAtInventory"; //Parameters (Item item, Character character)
    public static string CHARACTER_DEATH = "OnCharacterDied"; //Parameters (Character characterThatDied)
    public static string CHARACTER_KILLED = "OnCharacterKilled"; //Parameters (ICombatInitializer killer, Character characterThatDied)
    public static string COLLIDED_WITH_CHARACTER = "OnCollideWithCharacter"; //Parameters (ICombatInitializer character1, ICombatInitializer character2)
    public static string HISTORY_ADDED = "OnHistoryAdded"; //Parameters (object itemThatHadHistoryAdded) either a character or a landmark
    public static string CHARACTER_CREATED = "OnCharacterCreated"; //Parameters (Character createdCharacter)
    public static string ROLE_CHANGED = "OnCharacterRoleChanged"; //Parameters (Character characterThatChangedRole)
    public static string PAUSED = "OnPauseChanged"; //Parameters (bool isGamePaused)
    public static string PROGRESSION_SPEED_CHANGED = "OnProgressionSpeedChanged"; //Parameters (PROGRESSION_SPEED progressionSpeed)
    public static string TILE_LEFT_CLICKED = "OnTileLeftClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_RIGHT_CLICKED = "OnTileRightClicked"; //Parameters (HexTile clickedTile)
    public static string TILE_HOVERED_OVER = "OnTileHoveredOver"; //Parameters (HexTile hoveredTile)
    public static string TILE_HOVERED_OUT = "OnTileHoveredOut"; //Parameters (HexTile hoveredTile)
}
