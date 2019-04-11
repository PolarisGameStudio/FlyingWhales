﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReportCrime : GoapAction {

    GoapAction crimeToReport;

    public ReportCrime(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.REPORT_CRIME, INTERACTION_ALIGNMENT.GOOD, actor, poiTarget) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.Social_Icon;
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
            TIME_IN_WORDS.LATE_NIGHT,
        };
    }

    public void SetCrimeToReport(GoapAction goapAction) {
        crimeToReport = goapAction;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    public override void PerformActualAction() {
        if (actor.currentStructure == poiTarget.gridTileLocation.structure) {
            SetState("Report Crime Success");
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        return 3;
    }
    #endregion

    #region State Effects
    public void PreReportCrimeSuccess() {
        //**Effect 1**: The reported criminal will gain the associated Crime trait
        crimeToReport.actor.AddCriminalTrait(crimeToReport.committedCrime);
    }
    #endregion

    #region Requirements
    private bool Requirement() {
        //**Advertiser**: All Faction Leaders, Nobles and Soldiers
        if (poiTarget is Character) {
            Character character = poiTarget as Character;
            if (character.role.roleType == CHARACTER_ROLE.LEADER || character.role.roleType == CHARACTER_ROLE.NOBLE || character.role.roleType == CHARACTER_ROLE.SOLDIER) {
                return true;
            }
        }
        return false;
    }
    #endregion
}
