﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Freezing : Trait {

        //public ITraitable traitable { get; private set; }
        private GameObject _freezingGO;

        public Freezing() {
            name = "Freezing";
            description = "This is freezing.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnHour(1);
            isStacking = true;
            moodEffect = -5;
            stackLimit = 5;
            stackModifier = 1f;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if(addedTo is IPointOfInterest) {
                _freezingGO = GameManager.Instance.CreateParticleEffectAt(addedTo as IPointOfInterest, PARTICLE_EFFECT.Freezing);
            }
            if(addedTo is Character) {
                Character character = addedTo as Character;
                character.needsComponent.AdjustComfortDecreaseRate(1f);
                character.needsComponent.AdjustTirednessDecreaseRate(1f);
                character.AdjustSpeedModifier(-0.15f);
            }
        }
        public override void OnStackTrait(ITraitable addedTo) {
            base.OnStackTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.AdjustSpeedModifier(-0.15f);
            }
        }
        public override void OnUnstackTrait(ITraitable addedTo) {
            base.OnUnstackTrait(addedTo);
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.AdjustSpeedModifier(0.15f);
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (_freezingGO) {
                ObjectPoolManager.Instance.DestroyObject(_freezingGO);
            }
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                character.needsComponent.AdjustComfortDecreaseRate(-1f);
                character.needsComponent.AdjustTirednessDecreaseRate(-1f);
                character.AdjustSpeedModifier(0.15f);
            }
        }
        #endregion
    }
}
