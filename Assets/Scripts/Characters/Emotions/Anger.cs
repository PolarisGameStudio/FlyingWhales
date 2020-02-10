﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anger : Emotion {

    public Anger() : base(EMOTION.Anger) {

    }

    #region Overrides
    public override string ProcessEmotion(Character witness, IPointOfInterest target) {
        if(target is Character) {
            Character targetCharacter = target as Character;
            witness.relationshipContainer.AdjustOpinion(witness, targetCharacter, "Base", -14);
            witness.traitContainer.AddTrait(witness, "Angry");
            if(UnityEngine.Random.Range(0, 100) < 25) {
                int chance = UnityEngine.Random.Range(0, 3);
                if(chance == 0) {
                    CreateKnockoutJob(witness, targetCharacter);
                }else if (chance == 1) {
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