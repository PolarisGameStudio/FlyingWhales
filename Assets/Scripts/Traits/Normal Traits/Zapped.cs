﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Traits {
    public class Zapped : Status {

        private GameObject electricEffectGO;
        private AudioObject _audioObject;
        public Zapped() {
            name = "Zapped";
            description = "This character cannot move.";
            type = TRAIT_TYPE.STATUS;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = GameManager.Instance.GetTicksBasedOnMinutes(15);
            hindersMovement = true;
            hindersWitness = true;
            hindersPerform = true;
            //hasOnEnterGridTile = true;
            AddTraitOverrideFunctionIdentifier(TraitManager.Enter_Grid_Tile_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Initiate_Map_Visual_Trait);
            AddTraitOverrideFunctionIdentifier(TraitManager.Destroy_Map_Visual_Trait);
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourcePOI) {
            if(sourcePOI is IPointOfInterest poi) {
                electricEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Electric);
            }
            if (sourcePOI.gridTileLocation != null) {
                _audioObject = AudioManager.Instance.CreateAudioObject(AudioManager.Instance.GetRandomZapAudio(),
                    sourcePOI.gridTileLocation, 1, false);    
            }
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                if (character.carryComponent.masterCharacter.avatar.isTravelling) {
                    if (character.carryComponent.masterCharacter.avatar.travelLine == null) {
                        character.marker.StopMovement();
                    } else {
                        character.carryComponent.masterCharacter.avatar.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
                    }
                }
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.ExitCurrentState();
                }
                character.combatComponent.ClearHostilesInRange(false);
                character.combatComponent.ClearAvoidInRange(false);
                //character.AdjustCanPerform(1);
            }
            base.OnAddTrait(sourcePOI);
        }
        public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy) {
            if (electricEffectGO != null) {
                ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
                electricEffectGO = null;
            }
            if (_audioObject != null) {
                ObjectPoolManager.Instance.DestroyObject(_audioObject);
                _audioObject = null;
            }
            if (sourcePOI is Character) {
                Character character = sourcePOI as Character;
                //character.AdjustCanPerform(-1);
                if(character.marker) {
                    character.combatComponent.ClearHostilesInRange(false);
                    character.combatComponent.ClearAvoidInRange(false);
                }
            }
            base.OnRemoveTrait(sourcePOI, removedBy);
        }
        public override void OnEnterGridTile(IPointOfInterest poiWhoEntered, IPointOfInterest owner) {
            if (!poiWhoEntered.traitContainer.HasTrait("Zapped")) {
                poiWhoEntered.traitContainer.AddTrait(poiWhoEntered as ITraitable, "Zapped", bypassElementalChance: true);
            }
        }
        public override void OnInitiateMapObjectVisual(ITraitable traitable) {
            if (traitable is IPointOfInterest poi) {
                if (electricEffectGO) {
                    ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
                    electricEffectGO = null;
                }
                electricEffectGO = GameManager.Instance.CreateParticleEffectAt(poi, PARTICLE_EFFECT.Electric);
            }
        }
        public override void OnDestroyMapObjectVisual(ITraitable traitable) {
            if (electricEffectGO) {
                ObjectPoolManager.Instance.DestroyObject(electricEffectGO);
                electricEffectGO = null;
            }
        }
        #endregion
    }
}
