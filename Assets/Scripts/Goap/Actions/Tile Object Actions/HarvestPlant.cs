﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Logs;
using UnityEngine;  
using Traits;
using UtilityScripts;

public class HarvestPlant : GoapAction {

    public HarvestPlant() : base(INTERACTION_TYPE.HARVEST_PLANT) {
        actionIconString = GoapActionStateDB.Harvest_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_FOOD, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Harvest Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            //target is at structure that character is avoiding
            costLog += $" +2000(Location of target is in avoid structure)";
            actor.logComponent.AppendCostLog(costLog);
            return 2000;
        }
        if(job.jobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
            if (target.gridTileLocation != null && target.gridTileLocation.collectionOwner.isPartOfParentRegionMap && actor.gridTileLocation != null
                && actor.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                LocationGridTile centerGridTileOfTarget = target.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                LocationGridTile centerGridTileOfActor = actor.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.GetCenterLocationGridTile();
                float distance = centerGridTileOfActor.GetDistanceTo(centerGridTileOfTarget);
                int distanceToCheck = (InnerMapManager.BuildingSpotSize.x * 2) * 3;

                if(distance > distanceToCheck) {
                    //target is at structure that character is avoiding
                    costLog += $" +2000(Location of target too far from actor)";
                    actor.logComponent.AppendCostLog(costLog);
                    return 2000;
                }
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(40, 51);
        costLog += $" +{cost.ToString()}(Random Cost Between 40-50)";
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override void AddFillersToLog(ref Log log, ActualGoapNode node) {
        base.AddFillersToLog(ref log, node);
        log.AddToFillers(null, GetTargetString(node.poiTarget), LOG_IDENTIFIER.STRING_2);
    }
    public override void OnStopWhilePerforming(ActualGoapNode node) {
        base.OnStopWhilePerforming(node);
        if (node.actor.characterClass.IsCombatant()) {
            node.actor.needsComponent.AdjustDoNotGetBored(-1);
        }
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is Crops crops && crops.currentGrowthState != Crops.Growth_State.Ripe) {
                return false;
            }
            return poiTarget.IsAvailable() &&
                   poiTarget.gridTileLocation != null; //&& actor.traitContainer.HasTrait("Worker");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreHarvestSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(null, "50", LOG_IDENTIFIER.STRING_1);
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustDoNotGetBored(1);
        }
    }
    public void PerTickHarvestSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustHappiness(-2);
        }
    }
    public void AfterHarvestSuccess(ActualGoapNode goapNode) {
        if (goapNode.actor.characterClass.IsCombatant()) {
            goapNode.actor.needsComponent.AdjustDoNotGetBored(-1);
        }
        IPointOfInterest poiTarget = goapNode.poiTarget;
        if (poiTarget is Crops crop) {
            crop.SetGrowthState(Crops.Growth_State.Growing);
            
            List<LocationGridTile> choices = poiTarget.gridTileLocation.GetTilesInRadius(1, includeTilesInDifferentStructure: true, includeImpassable: false);
            if (choices.Count > 0) {
                FoodPile foodPile = CharacterManager.Instance.CreateFoodPileForPOI(poiTarget, CollectionUtilities.GetRandomElement(choices));
                if(goapNode.associatedJobType == JOB_TYPE.PRODUCE_FOOD_FOR_CAMP) {
                    if(goapNode.actor.partyComponent.hasParty && goapNode.actor.partyComponent.currentParty.targetCamp != null) {
                        goapNode.actor.partyComponent.currentParty.jobComponent.CreateHaulForCampJob(foodPile, goapNode.actor.partyComponent.currentParty.targetCamp);
                        goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
                    }
                } else {
                    if (foodPile != null && goapNode.actor.homeSettlement != null && goapNode.actor.isNormalCharacter) {
                        goapNode.actor.homeSettlement.settlementJobTriggerComponent.TryCreateHaulJob(foodPile);
                        goapNode.actor.marker.AddPOIAsInVisionRange(foodPile); //automatically add pile to character's vision so he/she can take haul job immediately after
                    }
                }
            }
        }else {
            LocationGridTile tile = poiTarget.gridTileLocation;
            tile.structure.RemovePOI(poiTarget);
            
            FoodPile foodPile = InnerMapManager.Instance.CreateNewTileObject<FoodPile>(TILE_OBJECT_TYPE.VEGETABLES);
            foodPile.SetResourceInPile(50);
            tile.structure.AddPOI(foodPile, tile);
        }
    }
    #endregion

    #region Utilities
    private string GetTargetString(IPointOfInterest poi) {
        if (poi is BerryShrub) {
            return "berries";
        } else if (poi is CornCrop) {
            return "corn";
        } else if (poi is Mushroom) {
            return "mushrooms";
        } else {
            return poi.name;
        }
    }
    #endregion
}