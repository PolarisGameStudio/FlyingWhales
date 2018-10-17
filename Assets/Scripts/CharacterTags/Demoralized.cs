﻿using System.Collections;
using System.Collections.Generic;
using ECS;
using UnityEngine;

public class Demoralized : Attribute {
    public Demoralized() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.DEMORALIZED) {
    }
    public override void OnAddAttribute(Character character) {
        base.OnAddAttribute(character);
        //_character.AdjustMentalPoints(-2);
    }
    public override void OnRemoveAttribute() {
        base.OnRemoveAttribute();
        //_character.AdjustMentalPoints(2);
    }
}
