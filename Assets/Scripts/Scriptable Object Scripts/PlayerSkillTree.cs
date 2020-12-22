﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Skill Tree", menuName = "Scriptable Objects/Player Skill Tree")]
public class PlayerSkillTree : ScriptableObject {
    public PlayerSkillTreeNodeDictionary nodes;
    public PLAYER_SKILL_TYPE[] initialLearnedSkills;
}
