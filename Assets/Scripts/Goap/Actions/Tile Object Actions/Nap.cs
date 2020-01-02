﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class Nap : GoapAction {

    public Nap() : base(INTERACTION_TYPE.NAP) {
        actionIconString = GoapActionStateDB.Sleep_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, conditionKey = string.Empty, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Nap Success", goapNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (CanSleepInBed(actor, poiTarget as TileObject) == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Nap Fail";
            } else if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        LocationStructure targetStructure = target.gridTileLocation.structure;
        if(targetStructure.structureType == STRUCTURE_TYPE.DWELLING) {
            Dwelling dwelling = targetStructure as Dwelling;
            if (dwelling.IsResident(actor)) {
                return 8;
            } else {
                for (int i = 0; i < dwelling.residents.Count; i++) {
                    Character resident = dwelling.residents[i];
                    if (resident != actor) {
                        if (actor.opinionComponent.HasOpinion(resident) && actor.opinionComponent.GetTotalOpinion(resident) > 0) {
                            return 25;
                        }
                    }
                }
                return 45;
            }
        } else if(targetStructure.structureType == STRUCTURE_TYPE.INN) {
            return 45;
        }
        return 100;
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        Character actor = node.actor;
        actor.traitContainer.RemoveTrait(actor, "Resting");
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }

            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.AddTrait(goapNode.actor, "Resting");
    }
    public void PerTickNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.needsComponent.AdjustTiredness(30);
    }
    public void AfterNapSuccess(ActualGoapNode goapNode) {
        goapNode.actor.traitContainer.RemoveTrait(goapNode.actor, "Resting");
    }
    //public void PreNapFail() {
    //    goapNode.descriptionLog.AddToFillers(targetStructure.location, targetStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    //public void PreNapMissing() {
    //    goapNode.descriptionLog.AddToFillers(actor.currentStructure.location, actor.currentStructure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
    //}
    #endregion

    private bool CanSleepInBed(Character character, TileObject tileObject) {
        for (int i = 0; i < tileObject.users.Length; i++) {
            if (tileObject.users[i] != null) {
                RELATIONSHIP_EFFECT relEffect = character.opinionComponent.GetRelationshipEffectWith(tileObject.users[i]);
                if (relEffect == RELATIONSHIP_EFFECT.NEGATIVE || relEffect == RELATIONSHIP_EFFECT.NONE) {
                    return false;
                }
            }
        }
        return true;
    }
}

public class NapData : GoapActionData {
    public NapData() : base(INTERACTION_TYPE.NAP) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
    }
}