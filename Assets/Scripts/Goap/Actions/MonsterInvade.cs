﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MonsterInvade : GoapAction {

    public MonsterInvade() : base(INTERACTION_TYPE.MONSTER_INVADE) {
        actionIconString = GoapActionStateDB.No_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL, RACE.DEMON
        };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Invade Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return !actor.partyComponent.hasParty;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterInvadeSuccess(ActualGoapNode goapNode) {
        object[] otherData = goapNode.otherData;
        if (otherData != null && otherData.Length == 1) {
            Party party = CharacterManager.Instance.CreateNewParty(PARTY_TYPE.Monster_Invade, goapNode.actor);
            MonsterInvadeParty monsterInvadeParty = party as MonsterInvadeParty;
            if(otherData[0] is LocationStructure targetStructure) {
                monsterInvadeParty.SetTargetStructure(targetStructure);
            } else if (otherData[0] is HexTile hex) {
                monsterInvadeParty.SetTargetHex(hex);
            }
        }
    }
    #endregion

}