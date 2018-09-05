﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ECS;

public class Meditator : Attribute {

    CharacterAction meditate;
    public Meditator() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.MEDITATOR) {

    }

    #region Overrides
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        meditate = ObjectManager.Instance.CreateNewCharacterAction(ACTION_TYPE.MEDITATE);
        character.AddMiscAction(meditate);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        character.RemoveMiscAction(meditate);
    }
    #endregion
}
