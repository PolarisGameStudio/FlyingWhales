﻿/*
 The head of a tribe. 
 The chieftain performs various actions like defending his cities and attacking enemy cities. 
 He also explores his tribe territory. He creates various Quests for other characters.
 This role is considered to be the highest of roles (Like a King).
 Place functions unique to chieftains here. 
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chieftain : CharacterRole {

	public Chieftain(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.CHIEFTAIN;
        this._allowedRoadTypes = new List<ROAD_TYPE>() {
            ROAD_TYPE.MAJOR, ROAD_TYPE.MINOR
        };
        this._canPassHiddenRoads = true;
        this._allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.ATTACK,
            QUEST_TYPE.DEFEND,
            QUEST_TYPE.EXPLORE_TILE
        };
    }
}
