﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ShareInformation : GoapAction {
    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public ShareInformation() : base(INTERACTION_TYPE.SHARE_INFORMATION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Share Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 3;
    }
    public override void AddFillersToLog(Log log, Character actor, IPointOfInterest poiTarget, object[] otherData, LocationStructure targetStructure) {
        base.AddFillersToLog(log, actor, poiTarget, otherData, targetStructure);
        if (otherData.Length == 1 && otherData[0] is GoapAction) {
            GoapAction eventToBeShared = otherData[0] as GoapAction;
            //TODO: log.AddToFillers(null, Utilities.LogDontReplace(eventToBeShared.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.OTHER);
            log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.OTHER_2);
            //TODO: log.AddToFillers(eventToBeShared.currentState.descriptionLog.fillers);
        }
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, poiTarget, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if (targetCharacter.IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region State Effects
    public void PreShareSuccess(ActualGoapNode goapNode) {
        //TODO: goapNode.descriptionLog.AddToFillers(null, Utilities.LogDontReplace(eventToBeShared.currentState.descriptionLog), LOG_IDENTIFIER.APPEND);
        goapNode.descriptionLog.AddToFillers(goapNode.actor, goapNode.actor.name, LOG_IDENTIFIER.OTHER);
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget, goapNode.poiTarget.name, LOG_IDENTIFIER.OTHER_2);
        //TODO: currentState.AddLogFillers(eventToBeShared.currentState.descriptionLog.fillers);
    }
    public void AfterShareSuccess(ActualGoapNode goapNode) {
        //TODO:
        //if(eventToBeShared.currentState.shareIntelReaction != null) {
        //    eventToBeShared.currentState.shareIntelReaction.Invoke(poiTarget as Character, null, SHARE_INTEL_STATUS.INFORMED);
        //}
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            Character target = poiTarget as Character;
            return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
        }
        return false;
    }
    #endregion
}

public class ShareInformationData : GoapActionData {
    public ShareInformationData() : base(INTERACTION_TYPE.SHARE_INFORMATION) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        Character target = poiTarget as Character;
        return actor != target && target.role.roleType != CHARACTER_ROLE.BEAST;
    }
}
