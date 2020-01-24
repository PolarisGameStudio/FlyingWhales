﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Betrayal : Emotion {

    public Betrayal() : base(EMOTION.Betrayal) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            witness.opinionComponent.AdjustOpinion(targetCharacter, "Base", -60);
            witness.traitContainer.AddTrait(witness, "Betrayed");
            if (UnityEngine.Random.Range(0, 100) < 50) {
                int chance = UnityEngine.Random.Range(0, 3);
                if (chance == 0) {
                    CreateKnockoutJob(witness, targetCharacter);
                } else if (chance == 1) {
                    CreateKillJob(witness, targetCharacter);
                } else if (chance == 2) {
                    witness.CreateUndermineJobOnly(targetCharacter, "provoke");
                }
            }
        }
        return base.ProcessEmotion(witness, target);
    }
    #endregion
}
