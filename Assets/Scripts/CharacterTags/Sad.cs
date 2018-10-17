﻿using UnityEngine;
using System.Collections;
using ECS;

public class Sad : Attribute {
    public Sad() : base(ATTRIBUTE_CATEGORY.CHARACTER, ATTRIBUTE.SAD) {

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
