﻿using System;
namespace Traits {
    public class DeMooder : Trait {
        
        public override bool isSingleton => true;
        
        public DeMooder() {
            name = "DeMooder";
            description = "Applies a temporary debuff on all hostiles in range. It will immediately perish afterwards.";
            type = TRAIT_TYPE.NEUTRAL;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            isHidden = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character character) {
                character.behaviourComponent.ChangeDefaultBehaviourSet(CharacterManager.DeMooder_Behaviour);
                character.SetDestroyMarkerOnDeath(true);
                if (character is Summon summon) {
                    summon.SetShowNotificationOnDeath(false);
                }
            }
        }
        public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            base.OnRemoveTrait(removedFrom, removedBy);
            if (removedFrom is Character character) {
                character.behaviourComponent.UpdateDefaultBehaviourSet();
            }
        }
        #endregion
    }
}