﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

namespace Traits {
    public class Consumable : Trait {
        private ITraitable _owner;

        public Consumable() {
            name = "Consumable";
            description = "This is consumable.";
            type = TRAIT_TYPE.PERSONALITY;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            _owner = addedTo;
        }
        public override bool OnDeath(Character character) {
            if(character.traitContainer.HasTrait("Burning", "Burnt")) {
                CharacterManager.Instance.CreateFoodPileForPOI(character);
                character.SetDestroyMarkerOnDeath(true);
            }
            return base.OnDeath(character);
        }
        #endregion
    }
}