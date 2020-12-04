﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkillTreeUI : MonoBehaviour {
    public ParentPlayerSkillTreeUI parentSkillTreeUI;
    public PlayerSkillTreeNodeItemDictionary skillTreeItems;

    public void LoadSkillTree() {
        foreach (KeyValuePair<PLAYER_SKILL_TYPE, PlayerSkillTreeItem> item in skillTreeItems) {
            if (parentSkillTreeUI.skillTree.nodes.ContainsKey(item.Key)) {
                item.Value.SetData(item.Key, parentSkillTreeUI.skillTree.nodes[item.Key], OnClickSkillTreeButton);    
            }
        }
    }

    public void OnClickSkillTreeButton(PLAYER_SKILL_TYPE skillType, PlayerSkillTreeItem skillTreeItem) {
        parentSkillTreeUI.OnClickSkillTreeButton(skillType, skillTreeItem);
    }
}