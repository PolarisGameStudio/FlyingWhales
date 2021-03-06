﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;
using Logs;
using UnityEngine.Assertions;
using Locations.Settlements;

public class TakeResource : GoapAction {
    public TakeResource() : base(INTERACTION_TYPE.TAKE_RESOURCE) {
        actionIconString = GoapActionStateDB.Haul_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Work};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.TAKE_POI, GOAP_EFFECT_TARGET.ACTOR));
    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        if (target is ResourcePile) {
            List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
            List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
            if (baseEE != null && baseEE.Count > 0) {
                ee.AddRange(baseEE);
            }
            ResourcePile pile = target as ResourcePile;
            ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TAKE_POI, conditionKey = pile.name, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
            isOverridden = true;
            return ee;
        } 
        //NOTE: UNCOMMENT THIS IF WE WANT CHARACTERS TO TAKE FOOD FROM OTHER TABLES
        //else if (target is Table) {
        //    ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_FOOD, conditionKey = "0", isKeyANumber = true, target = GOAP_EFFECT_TARGET.ACTOR });
        //}
        return base.GetExpectedEffects(actor, target, otherData, out isOverridden);
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Take Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        int cost = 0;
        if (target.gridTileLocation != null && actor.movementComponent.structuresToAvoid.Contains(target.gridTileLocation.structure)) {
            if (!actor.partyComponent.hasParty) {
                //target is at structure that character is avoiding
                cost = 2000;
                costLog += $" +{cost}(Location of target is in avoid structure)";
                actor.logComponent.AppendCostLog(costLog);
                return cost;
            }
        }
        if (target.gridTileLocation != null) {
            BaseSettlement settlement;
            if (target.gridTileLocation.IsPartOfSettlement(out settlement)) {
                if (settlement.owner != null && actor.homeSettlement != settlement) {
                    //If target is in a claimed settlement and actor's home settlement is not the target's settlement, do not harvest, even if the faction owner of the target's settlement is also the faciton of the actor
                    costLog += $" +2000(Target's settlement is not the actor's home settlement)";
                    actor.logComponent.AppendCostLog(costLog);
                    return 2000;
                }
            }
        }
        if (job.jobType.IsFullnessRecoveryTypeJob() || job.jobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD) {
            if(target is ElfMeat || target is HumanMeat) {
                if (actor.traitContainer.HasTrait("Cannibal") && !actor.traitContainer.HasTrait("Vampire")) {
                    int currCost = 450; //UtilityScripts.Utilities.Rng.Next(450, 501);
                    cost += currCost;
                    costLog += $" +{currCost}(Obtain Personal Food, Elf/Human Meat, Cannibal)";
                } else if (actor.needsComponent.isStarving) {
                    int currCost = 700; //UtilityScripts.Utilities.Rng.Next(700, 751);
                    cost += currCost;
                    costLog += $" +{currCost}(Obtain Personal Food, Elf/Human Meat, Starving)";
                } else {
                    cost += 2000;
                    costLog += $" +2000(Obtain Personal Food, Elf/Human Meat, not Starving/Cannibal)";
                }
            } else {
                // int currCost = UtilityScripts.Utilities.Rng.Next(400, 431);
                cost = 400;
                costLog += $" +{cost}(Obtain Personal Food, not Elf/Human Meat)";
            }
        } else {
            if (target.gridTileLocation != null && target.gridTileLocation.IsPartOfSettlement(out var settlement) && 
                settlement.locationType == LOCATION_TYPE.VILLAGE && settlement != actor.homeSettlement) {
                cost = 2000;
                costLog += $" +{cost}(Resource pile is at another village)";
            } else {
                cost = 400;
                costLog += $" +{cost}(not Obtain Personal Food)";    
            }
            if (job.jobType == JOB_TYPE.BUILD_BLUEPRINT && target is ResourcePile resourcePile) {
                int neededResource = GetNeededResource(job, otherData, resourcePile);
                if (actor.homeSettlement != null) {
                    int availableResources = actor.homeSettlement.settlementJobTriggerComponent.GetTotalResource(resourcePile.providedResource);
                    if (availableResources < neededResource) {
                        cost = 2000;
                        costLog += $" +{cost}(Settlement does not have enough resources for building)";    
                    }
                }
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            ResourcePile pile = poiTarget as ResourcePile;
            if (pile.resourceInPile <= 0) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Take Fail";
            }
        }
        return goapActionInvalidity;
    }
    public override void AddFillersToLog(ref Log log, ActualGoapNode node) {
        base.AddFillersToLog(ref log, node);
        ResourcePile resourcePile = node.poiTarget as ResourcePile;
        log.AddToFillers(null, UtilityScripts.Utilities.NormalizeStringUpperCaseFirstLetterOnly(resourcePile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            if (poiTarget.gridTileLocation == null && poiTarget.isBeingCarriedBy != actor) {
                return false;
            }
            return true;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreTakeSuccess(ActualGoapNode goapNode) {
        ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
        Assert.IsNotNull(resourcePile);
        int takenResource = GetNeededResource(goapNode.associatedJob, goapNode.otherData, resourcePile);
        
        if (takenResource > resourcePile.resourceInPile) {
            takenResource = resourcePile.resourceInPile;
        }
        goapNode.descriptionLog.AddToFillers(null, takenResource.ToString(), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeString(resourcePile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    public void AfterTakeSuccess(ActualGoapNode goapNode) {
        ResourcePile resourcePile = goapNode.poiTarget as ResourcePile;
        Assert.IsNotNull(resourcePile);
        int takenResource = GetNeededResource(goapNode.associatedJob, goapNode.otherData, resourcePile);
        if (takenResource > resourcePile.resourceInPile) {
            takenResource = resourcePile.resourceInPile;
        }
        //I think possible errors with this is going to be on the part where the npcSettlement doesn't know that this new pile exist because we directly add the new pile to the actor's party, we don't add the new pile to the structure, thus, it won't be added to the list of poi
        //Hence, if it becomes a complication to the game, what we must do is add the new pile to the structure without actually placing the object to the tile, so I think we will have to create a new function wherein we don't actually place the poi on the tile but it will still be added to the list of poi in the structure
        //EDIT: Taking resource does not mean that the character must not be carrying any poi, when the actor takes a resource and it is the same type as the one he/she is carrying, just add the amount, if it is not the same type, replace the carried one with the new type
        //If the actor is not carrying anything, create new object to be carried
        //if(carriedResourcePile != null) {
        //    if(carriedResourcePile.tileObjectType != resourcePile.tileObjectType) {
        //        goapNode.actor.UncarryPOI(bringBackToInventory: true);
        //        CarryResourcePile(goapNode.actor, resourcePile, takenResource);
        //    } else {
        //        carriedResourcePile.AdjustResourceInPile(takenResource);
        //    }
        //} else {
        //    goapNode.actor.UncarryPOI(bringBackToInventory: true);
        //    CarryResourcePile(goapNode.actor, resourcePile, takenResource);
        //}
        goapNode.actor.UncarryPOI(bringBackToInventory: true);

        bool setOwnership = !(goapNode.associatedJobType == JOB_TYPE.HAUL
                              || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_NORMAL
                              || goapNode.associatedJobType == JOB_TYPE.FULLNESS_RECOVERY_URGENT
                              || goapNode.associatedJobType == JOB_TYPE.OBTAIN_PERSONAL_FOOD);

        CarryResourcePile(goapNode.actor, resourcePile, takenResource, setOwnership);
        //goapNode.actor.AdjustResource(resourcePile.providedResource, takenResource);

        //goapNode.descriptionLog.AddToFillers(null, takenResource.ToString(), LOG_IDENTIFIER.STRING_1);
        //goapNode.descriptionLog.AddToFillers(null, Utilities.NormalizeString(resourcePile.providedResource.ToString()), LOG_IDENTIFIER.STRING_2);
    }
    #endregion

    private void CarryResourcePile(Character carrier, ResourcePile pile, int amount, bool setOwnership) {
        if (pile.isBeingCarriedBy == null || pile.isBeingCarriedBy != carrier) {
            ResourcePile newPile = InnerMapManager.Instance.CreateNewTileObject<ResourcePile>(pile.tileObjectType);
            newPile.SetResourceInPile(amount);

            //This can be made into a function in the IPointOfInterest interface
            newPile.SetGridTileLocation(pile.gridTileLocation);
            newPile.InitializeMapObject(newPile);
            newPile.SetPOIState(POI_STATE.ACTIVE);
            newPile.gridTileLocation.structure.region.AddPendingAwareness(newPile);
            newPile.SetGridTileLocation(null);

            // carrier.ownParty.AddPOI(newPile);
            carrier.CarryPOI(newPile, setOwnership: setOwnership);
            carrier.ShowItemVisualCarryingPOI(newPile);
            TraitManager.Instance.CopyStatuses(pile, newPile);
            pile.AdjustResourceInPile(-amount);
        } else {
            carrier.ShowItemVisualCarryingPOI(pile);
        }
    }

    private int GetNeededResource(JobQueueItem jobQueueItem, OtherData[] otherData, ResourcePile resourcePile) {
        int takenResource;
        if (otherData != null && otherData.Length == 1) {
            OtherData data = otherData[0];
            if (data is IntOtherData intOtherData) {
                takenResource = intOtherData.integer;    
            } else if (data is TileObjectRecipeOtherData tileObjectRecipeOtherData) {
                TileObjectRecipe recipe = tileObjectRecipeOtherData.recipe;
                takenResource = recipe.GetNeededAmountForIngredient(resourcePile.tileObjectType);
            } else {
                //set amount just to prevent errors.
                takenResource = 10;
            }
        } else {
            if (jobQueueItem is GoapPlanJob job) {
                if (job.jobType == JOB_TYPE.DARK_RITUAL || job.jobType == JOB_TYPE.EVANGELIZE) {
                    //if job is dark ritual, assume that take resource is for a cultist kit, this isn't ideal, but cannot think of another solution at the moment.
                    TileObjectData data = TileObjectDB.GetTileObjectData(TILE_OBJECT_TYPE.CULTIST_KIT);
                    TileObjectRecipe recipe = data.GetRecipeThatUses(resourcePile.tileObjectType);
                    takenResource = recipe.GetNeededAmountForIngredient(resourcePile.tileObjectType);
                } else if (job.targetPOI is TileObject tileObject && !job.jobType.IsFullnessRecoveryTypeJob()) {
                    TileObjectData data = TileObjectDB.GetTileObjectData(tileObject.tileObjectType);
                    if (data != null && data.craftRecipes != null) {
                        TileObjectRecipe recipe = data.GetRecipeThatUses(resourcePile.tileObjectType);
                        takenResource = recipe.GetNeededAmountForIngredient(resourcePile.tileObjectType);    
                    } else {
                        //set amount just to prevent errors.
                        takenResource = Mathf.Min(20, resourcePile.resourceInPile);
                    }    
                } else {
                    takenResource = Mathf.Min(20, resourcePile.resourceInPile);    
                }
            } else {
                takenResource = Mathf.Min(20, resourcePile.resourceInPile);    
            }
        }

        return takenResource;
    }
}
