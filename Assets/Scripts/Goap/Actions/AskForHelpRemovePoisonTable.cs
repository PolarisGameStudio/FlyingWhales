﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AskForHelpRemovePoisonTable : GoapAction {

    private Character troubledCharacter;
    private IPointOfInterest targetTable;
    private Poisoned poison;

    public AskForHelpRemovePoisonTable(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, INTERACTION_ALIGNMENT.NEUTRAL, actor, poiTarget) {
        troubledCharacter = actor.troubledCharacter;
        actionIconString = GoapActionStateDB.Work_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
        //isNotificationAnIntel = false;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    //protected override void ConstructPreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY, conditionKey = actor.homeArea, targetPOI = poiTarget });
    //}
    public override void PerformActualAction() {
        base.PerformActualAction();
        if (!isTargetMissing && (poiTarget as Character).IsInOwnParty()) {
            SetState("Ask Success");
        } else {
            SetState("Target Missing");
        }
    }
    protected override int GetCost() {
        return 3;
    }
    //protected override void AddDefaultObjectsToLog(Log log) {
    //    log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //    log.AddToFillers(poiTarget, poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER); //Target character is only the identifier but it doesn't mean that this is a character, it can be item, etc.
    //    //if (targetTable != null) {
    //    //    log.AddToFillers(targetTable.gridTileLocation.structure, targetTable.gridTileLocation.structure.name, LOG_IDENTIFIER.LANDMARK_1);
    //    //}
    //}
    public override bool InitializeOtherData(object[] otherData) {
        this.otherData = otherData;
        if (otherData.Length == 1 && otherData[0] is IPointOfInterest) {
            targetTable = otherData[0] as IPointOfInterest;
            poison = targetTable.GetNormalTrait("Poisoned") as Poisoned;
            if (thoughtBubbleMovingLog != null) {
                thoughtBubbleMovingLog.AddToFillers(targetTable.gridTileLocation.structure, targetTable.gridTileLocation.structure.GetNameRelativeTo(actor), LOG_IDENTIFIER.LANDMARK_1);
            }
            return true;
        }
        return base.InitializeOtherData(otherData);
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        return actor != poiTarget;
    }
    #endregion

    #region State Effects
    public void PreAskSuccess() {
        currentState.AddLogFiller(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
        currentState.AddLogFiller(targetTable.gridTileLocation.structure, targetTable.gridTileLocation.structure.name, LOG_IDENTIFIER.LANDMARK_1);
        currentState.SetIntelReaction(SuccessReactions);
    }
    public void AfterAskSuccess() {
        Character target = poiTarget as Character;
        //**Effect 1**: Target will add a Remove Poison Table job on his personal queue.
        GoapPlanJob job = new GoapPlanJob(JOB_TYPE.REMOVE_POISON, new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.REMOVE_TRAIT, conditionKey = "Poisoned", targetPOI = targetTable });
        target.jobQueue.AddJobInQueue(job);
    }
    //public void PreTargetMissing() {
    //    currentState.AddLogFiller(troubledCharacter, troubledCharacter.name, LOG_IDENTIFIER.CHARACTER_3);
    //}
    #endregion

    #region Intel Reactions
    private List<string> SuccessReactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
        List<string> reactions = new List<string>();
        Character target = poiTarget as Character;
        TileObject table = targetTable as TileObject;
        if (poison.responsibleCharacters.Contains(recipient)) {
            JobQueueItem jqi = target.jobQueue.GetJob(JOB_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE, targetTable);
            if (jqi is GoapPlanJob) {
                //- Create an Attempt to Stop Job targeting Target with Remove Poison Table as the target job
                recipient.CreateAttemptToStopCurrentActionAndJob(target, jqi as GoapPlanJob);
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    //- If informed: "I will not let [Target Name] prevent me from achieving my goal."
                    reactions.Add(string.Format("I will not let {0} prevent me from achieving my goal.", target.name));
                }
            } else {
                if (status == SHARE_INTEL_STATUS.INFORMED) {
                    // - If informed: "Unfortunate that my plans have been thwarted."   
                    reactions.Add("Unfortunate that my plans have been thwarted.");
                }
            }
        } else if (table.IsOwnedBy(recipient)) {
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                //- If informed: "I appreciate [Target Name]'s help." 
                reactions.Add(string.Format("I appreciate {0}'s help.", target.name));
            }
        } else {
            if (status == SHARE_INTEL_STATUS.INFORMED) {
                //- If informed: "This isn't relevant to me."
                reactions.Add("This isn't relevant to me.");
            }
        }
        return reactions;
    }
    #endregion
}

public class AskForHelpRemovePoisonTableData : GoapActionData {
    public AskForHelpRemovePoisonTableData() : base(INTERACTION_TYPE.ASK_FOR_HELP_REMOVE_POISON_TABLE) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor != poiTarget;
    }
}