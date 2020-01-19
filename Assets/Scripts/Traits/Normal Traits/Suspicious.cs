﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Suspicious : Trait {

        public Suspicious() {
            name = "Suspicious";
            description = "Suspicious characters will destroy Artifacts placed by the Ruinarch instead of inspecting them.";
            type = TRAIT_TYPE.BUFF;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            //effects = new List<TraitEffect>();
        }

        #region Overrides
        public override bool CreateJobsOnEnterVisionBasedOnOwnerTrait(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is TileObject) {
                TileObject objectToBeInspected = targetPOI as TileObject;
                if (objectToBeInspected.isSummonedByPlayer) {
                    characterThatWillDoJob.jobComponent.TriggerDestroy(objectToBeInspected);
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnOwnerTrait(targetPOI, characterThatWillDoJob);
        }
        #endregion
    }

}
