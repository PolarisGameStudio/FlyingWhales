﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resting : Trait {

    private Character _character;
    private Lycanthropy _lycanthropyTrait;

    public Resting() {
        name = "Resting";
        description = "This character is resting.";
        thoughtText = "[Character] is resting.";
        type = TRAIT_TYPE.DISABLER;
        effect = TRAIT_EFFECT.NEUTRAL;
        trigger = TRAIT_TRIGGER.OUTSIDE_COMBAT;
        associatedInteraction = INTERACTION_TYPE.NONE;
        crimeSeverity = CRIME_CATEGORY.NONE;
        daysDuration = 0;
        effects = new List<TraitEffect>();
        //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.DRINK_BLOOD };
    }

    #region Overrides
    public override void OnAddTrait(IPointOfInterest sourceCharacter) {
        _character = sourceCharacter as Character;
        _lycanthropyTrait = _character.GetNormalTrait("Lycanthropy") as Lycanthropy;
        if(_lycanthropyTrait != null) {
            Messenger.AddListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        }
        Messenger.AddListener(Signals.TICK_STARTED, RecoverHP);
        base.OnAddTrait(sourceCharacter);
    }
    public override void OnRemoveTrait(IPointOfInterest sourceCharacter) {
        if (_lycanthropyTrait != null) {
            Messenger.RemoveListener(Signals.HOUR_STARTED, CheckForLycanthropy);
        }
        Messenger.RemoveListener(Signals.TICK_STARTED, RecoverHP);
        _character = null;
        base.OnRemoveTrait(sourceCharacter);
    }
    #endregion

    private void CheckForLycanthropy() {
        int chance = UnityEngine.Random.Range(0, 100);
        if(_character.race == RACE.WOLF) {
            //Turn back to normal form
            if (chance < 30) {
                _lycanthropyTrait.PlanRevertToNormal();
                _character.currentAction.currentState.EndPerTickEffect();
            }
        } else {
            //Turn to wolf
            if (chance < 30) {
                _lycanthropyTrait.PlanTransformToWolf();
                _character.currentAction.currentState.EndPerTickEffect();
            }
        }
    }

    private void RecoverHP() {
        _character.HPRecovery(0.02f);
    }
}
