﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Angry : Trait {
        public Angry() {
            name = "Angry";
            description = "This character will often argue with others and may destroy objects.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(8);
            moodEffect = -3;
            isStacking = true;
            stackLimit = 5;
            stackModifier = 0.5f;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is TileObject) {
                if (UnityEngine.Random.Range(0, 100) < 3) {
                    return characterThatWillDoJob.jobComponent.TriggerDestroy(targetPOI);
                }
            } else if (targetPOI is Character) {
                Character targetCharacter = targetPOI as Character;
                if (UnityEngine.Random.Range(0, 2) == 0 && characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter) == RELATIONSHIP_EFFECT.NEGATIVE) {
                    characterThatWillDoJob.marker.AddHostileInRange(targetCharacter, false, isLethal: false);
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }
}

