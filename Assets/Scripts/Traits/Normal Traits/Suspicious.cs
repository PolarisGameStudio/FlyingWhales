﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Suspicious : Trait {
        public override bool isSingleton => true;

        public Suspicious() {
            name = "Suspicious";
            description = "Thinks danger is lurking at every corner. Might destroy things the player touches.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            AddTraitOverrideFunctionIdentifier(TraitManager.See_Poi_Trait);
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (characterThatWillDoJob.canPerform && characterThatWillDoJob.canMove && !characterThatWillDoJob.isDead && targetPOI is TileObject objectToBeInspected) {
                if (objectToBeInspected.lastManipulatedBy is Player) {
                    characterThatWillDoJob.jobComponent.TriggerDestroy(objectToBeInspected);
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }

}
