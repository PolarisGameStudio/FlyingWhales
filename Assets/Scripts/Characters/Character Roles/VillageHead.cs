﻿/*
 The head of a village. 
 The Village Head stays in his village and mostly creates Quests intended to help the village.
 Place functions unique to village heads here.
 */
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VillageHead : CharacterRole {

	public VillageHead(ECS.Character character): base (character) {
        _roleType = CHARACTER_ROLE.VILLAGE_HEAD;
        _allowedRoadTypes = new List<ROAD_TYPE>();
        _canAcceptQuests = true;
        _canPassHiddenRoads = false;
        _allowedQuestTypes = new List<QUEST_TYPE>() {
            QUEST_TYPE.OBTAIN_MATERIAL,
            QUEST_TYPE.BUILD_STRUCTURE
        };
    }
}
