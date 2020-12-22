﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resentment : Emotion {

    public Resentment() : base(EMOTION.Resentment) {
        responses = new[] {"Resentful"};
    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target, REACTION_STATUS status,
        ActualGoapNode goapNode = null, string reason = "") {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Resentment", -15);
            witness.traitContainer.AddTrait(witness, "Annoyed", targetCharacter);
        }
        return base.ProcessEmotion(witness, target, status, goapNode, reason);
    }
    #endregion
}