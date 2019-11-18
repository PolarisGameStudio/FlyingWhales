﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Soldier : CharacterRole {
    public override int reservedSupply { get { return 30; } }

    public Soldier() : base(CHARACTER_ROLE.SOLDIER, "Normal", new INTERACTION_CATEGORY[] { INTERACTION_CATEGORY.SUBTERFUGE, INTERACTION_CATEGORY.OFFENSE, INTERACTION_CATEGORY.DEFENSE }) {
        allowedInteractions = new INTERACTION_TYPE[] {
            INTERACTION_TYPE.GET_SUPPLY,
            INTERACTION_TYPE.ASSAULT,
        };
        requiredItems = new SPECIAL_TOKEN[] {
            SPECIAL_TOKEN.HEALING_POTION,
            SPECIAL_TOKEN.HEALING_POTION
        };
    }
}
