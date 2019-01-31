﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Craftsman : Trait {

	public SPECIAL_TOKEN craftedItemName { get; private set; }

    public Craftsman() {
        name = "Craftsman";
        craftedItemName = SPECIAL_TOKEN.JUNK;
        description = "This character can create items.";
        type = TRAIT_TYPE.ABILITY;
        effect = TRAIT_EFFECT.POSITIVE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
    }

    #region Overrides
    public override void OnAddTrait(Character sourceCharacter) {
        if(sourceCharacter.race == RACE.HUMANS) {
            craftedItemName = SPECIAL_TOKEN.HEALING_POTION;
        }else if (sourceCharacter.race == RACE.ELVES) {
            craftedItemName = SPECIAL_TOKEN.GOLDEN_NECTAR;
        }else if (sourceCharacter.race == RACE.FAERY) {
            craftedItemName = SPECIAL_TOKEN.ENCHANTED_AMULET;
        }else if (sourceCharacter.race == RACE.GOBLIN) {
            craftedItemName = SPECIAL_TOKEN.JUNK;
        }
    }
    #endregion
}
