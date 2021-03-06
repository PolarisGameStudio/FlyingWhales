﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Ravenous : Trait {

        public RavenousSpirit owner { get; private set; }
        public Ravenous() {
            name = "Ravenous";
            description = "This is ravenous.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
            //hasOnCollideWith = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Collision_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is RavenousSpirit) {
                owner = addedTo as RavenousSpirit;
            }
        }
        public override void LoadTraitOnLoadTraitContainer(ITraitable addTo) {
            base.LoadTraitOnLoadTraitContainer(addTo);
            if (addTo is RavenousSpirit) {
                owner = addTo as RavenousSpirit;
            }
        }
        public override bool OnCollideWith(IPointOfInterest collidedWith, IPointOfInterest owner) {
            if (collidedWith is Character) {
                Character target = collidedWith as Character;
                if (target.needsComponent.HasNeeds() && !target.isDead) {
                    this.owner.StartSpiritPossession(target);
                }
            }
            return true;       
        }
        #endregion
    }
}
