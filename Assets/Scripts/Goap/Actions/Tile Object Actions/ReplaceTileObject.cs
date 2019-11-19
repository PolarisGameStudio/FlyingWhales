﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class ReplaceTileObject : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public ReplaceTileObject() : base(INTERACTION_TYPE.REPLACE_TILE_OBJECT) {
        //actionLocationType = ACTION_LOCATION_TYPE.ON_TARGET;
        actionIconString = GoapActionStateDB.Work_Icon;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
    }

    #region Overrides
    public override void AddFillersToLog(Log log, Character actor, IPointOfInterest poiTarget, object[] otherData, LocationStructure targetStructure) {
        base.AddFillersToLog(log, actor, poiTarget, otherData, targetStructure);
        TileObject tileObjectToReplace = otherData[0] as TileObject;
        log.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectToReplace.tileObjectType.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        //if (tileObjectToReplace != null) {
        //    TileObjectData data = TileObjectDB.GetTileObjectData(tileObjectToReplace.tileObjectType);
        //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_SUPPLY, conditionKey = data.constructionCost, targetPOI = actor }, () => HasSupply(data.constructionCost));
        //}
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION., conditionKey = poiTarget, targetPOI = actor });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Replace Success", goapNode);
        
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, object[] otherData) {
        return 1;
    }
    //public override bool InitializeOtherData(object[] otherData) {
    //    this.otherData = otherData;
    //    if (otherData.Length == 2 && otherData[0] is TileObject && otherData[1] is LocationGridTile) {
    //        tileObjectToReplace = (TileObject) otherData[0];
    //        whereToPlace = (LocationGridTile) otherData[1];
    //        _targetStructure = whereToPlace.structure;
    //        SetTargetStructure();
    //        preconditions.Clear();
    //        expectedEffects.Clear();
    //        ConstructBasePreconditionsAndEffects();
    //        CreateThoughtBubbleLog();
    //        states["Replace Success"].OverrideDuration(TileObjectDB.GetTileObjectData(tileObjectToReplace.tileObjectType).constructionTime);
    //        return true;
    //    }
    //    return base.InitializeOtherData(otherData);
    //}
    #endregion

    #region State Effects
    private void PreReplaceSuccess(ActualGoapNode goapNode) {
        TileObject tileObjectToReplace = goapNode.otherData[0] as TileObject;
        goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeStringUpperCaseFirstLetters(tileObjectToReplace.ToString()), LOG_IDENTIFIER.STRING_1);
    }
    private void AfterReplaceSuccess(ActualGoapNode goapNode) {
        TileObject tileObjectToReplace = goapNode.otherData[0] as TileObject;
        LocationGridTile whereToPlace = goapNode.otherData[1] as LocationGridTile;
        //place the tile object at the specified location.
        goapNode.targetStructure.AddPOI(tileObjectToReplace, whereToPlace);
        tileObjectToReplace.AdjustHP(tileObjectToReplace.maxHP);
        goapNode.actor.AdjustSupply(-TileObjectDB.GetTileObjectData(tileObjectToReplace.tileObjectType).constructionCost);
        //make all residents aware of supply pile, just in case it was ever removed because of ghost collision
        for (int i = 0; i < whereToPlace.parentAreaMap.area.region.residents.Count; i++) {
            whereToPlace.parentAreaMap.area.region.residents[i].AddAwareness(tileObjectToReplace);
        }
    }
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return actor == poiTarget;
        }
        return false;
    }
    #endregion

}

public class ReplaceTileObjectData : GoapActionData {
    public ReplaceTileObjectData() : base(INTERACTION_TYPE.REPLACE_TILE_OBJECT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return actor == poiTarget;
    }
}