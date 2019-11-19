﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class PlayGuitar : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.DIRECT; } }

    public PlayGuitar() : base(INTERACTION_TYPE.PLAY_GUITAR) {
        validTimeOfDays = new TIME_IN_WORDS[] {
            TIME_IN_WORDS.MORNING,
            TIME_IN_WORDS.LUNCH_TIME,
            TIME_IN_WORDS.AFTERNOON,
            TIME_IN_WORDS.EARLY_NIGHT,
        };
        actionIconString = GoapActionStateDB.Entertain_Icon;
        shouldIntelNotificationOnlyIfActorIsActive = true;
        isNotificationAnIntel = false;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Play Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget.gridTileLocation != null) {
            LocationGridTile knownLoc = poiTarget.gridTileLocation;
            if (actor.homeStructure == knownLoc.structure) {
                //- Actor is resident of the Guitar's Dwelling: 15 - 26 (If Music Lover 5 - 12)
                return Utilities.rng.Next(15, 27);
            } else {
                if (knownLoc.structure is Dwelling) {
                    Dwelling dwelling = knownLoc.structure as Dwelling;
                    if (dwelling.residents.Count > 0) {
                        for (int i = 0; i < dwelling.residents.Count; i++) {
                            Character currResident = dwelling.residents[i];
                            if (currResident.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
                                //- Actor is not a resident but has a positive relationship with the Guitar's Dwelling resident: 20-36 (If music lover 10 - 26)
                                return Utilities.rng.Next(20, 37);
                            }
                        }
                        //the actor does NOT have any positive relations with any resident
                        return 99999; //NOTE: Should never reach here since Requirement prevents this.
                    }
                }
            }
        }
        //- Guitar Structure Has No Residents 40 - 56 (If Music Lover 25 - 46)
        return Utilities.rng.Next(40, 57);
    }
    public override void OnStopWhilePerforming(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        base.OnStopWhilePerforming(actor, poiTarget, otherData);
        actor.AdjustDoNotGetLonely(-1);
        poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    public override GoapActionInvalidity IsInvalid(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(actor, poiTarget, otherData);
        if (goapActionInvalidity.isInvalid == false) {
            if (poiTarget.IsAvailable() == false) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Play Fail";
            }
        }
        return goapActionInvalidity;
    }
    #endregion

    #region State Effects
    public void PrePlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetLonely(1);
        goapNode.poiTarget.SetPOIState(POI_STATE.INACTIVE);
        //TODO: currentState.SetIntelReaction(PlaySuccessIntelReaction);
    }
    public void PerTickPlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHappiness(500);
    }
    public void AfterPlaySuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustDoNotGetLonely(-1);
        goapNode.poiTarget.SetPOIState(POI_STATE.ACTIVE);
    }
    //public void PreTargetMissing() {
    //    actor.RemoveAwareness(poiTarget);
    //}
    #endregion

    #region Requirement
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
                return false;
            }
            if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
                return false;
            }
            if (actor.traitContainer.GetNormalTrait("MusicHater") != null) {
                return false; //music haters will never play guitar
            }
            if (poiTarget.gridTileLocation == null) {
                return false;
            }
            LocationGridTile knownLoc = poiTarget.gridTileLocation;
            //**Advertised To**: Residents of the dwelling or characters with a positive relationship with a Resident
            if (knownLoc.structure is Dwelling) {
                if (actor.homeStructure == knownLoc.structure) {
                    return true;
                } else {
                    Dwelling dwelling = knownLoc.structure as Dwelling;
                    if (dwelling.residents.Count > 0) {
                        for (int i = 0; i < dwelling.residents.Count; i++) {
                            Character currResident = dwelling.residents[i];
                            if (currResident.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
                                return true;
                            }
                        }
                        //the actor does NOT have any positive relations with any resident
                        return false;
                    } else {
                        //in cases that the guitar is at a dwelling with no residents, always allow.
                        return true;
                    }
                }
            } else {
                //in cases that the guitar is not inside a dwelling, always allow.
                return true;
            }
        }
        return false;
    }
    #endregion

    //#region Intel Reactions
    //private List<string> PlaySuccessIntelReaction(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();

    //    if(status == SHARE_INTEL_STATUS.WITNESSED && recipient.traitContainer.GetNormalTrait("Music Hater") != null) {
    //        recipient.traitContainer.AddTrait(recipient, "Annoyed");
    //        if (recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.LOVER) || recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.PARAMOUR)) {
    //            if (recipient.CreateBreakupJob(actor) != null) {
    //                Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "break_up");
    //                log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //                log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //                log.AddLogToInvolvedObjects();
    //                PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            }
    //        } else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo, RELATIONSHIP_TRAIT.ENEMY)) {
    //            //Otherwise, if the Actor does not yet consider the Target an Enemy, relationship degradation will occur, log:
    //            Log log = new Log(GameManager.Instance.Today(), "Trait", "MusicHater", "degradation");
    //            log.AddToFillers(recipient, recipient.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
    //            log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //            log.AddLogToInvolvedObjects();
    //            PlayerManager.Instance.player.ShowNotificationFrom(recipient, log);
    //            RelationshipManager.Instance.RelationshipDegradation(actor, recipient);
    //        }
    //    }
    //    return reactions;
    //}
    //#endregion
}

public class PlayGuitarData : GoapActionData {
    public PlayGuitarData() : base(INTERACTION_TYPE.PLAY_GUITAR) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (!poiTarget.IsAvailable() || poiTarget.gridTileLocation == null) {
            return false;
        }
        if (poiTarget.gridTileLocation != null && actor.trapStructure.structure != null && actor.trapStructure.structure != poiTarget.gridTileLocation.structure) {
            return false;
        }
        if (actor.traitContainer.GetNormalTrait("MusicHater") != null) {
            return false; //music haters will never play guitar
        }
        if (poiTarget.gridTileLocation == null) {
            return false;
        }
        LocationGridTile knownLoc = poiTarget.gridTileLocation;
        //**Advertised To**: Residents of the dwelling or characters with a positive relationship with a Resident
        if (knownLoc.structure is Dwelling) {
            if (actor.homeStructure == knownLoc.structure) {
                return true;
            } else {
                Dwelling dwelling = knownLoc.structure as Dwelling;
                if (dwelling.residents.Count > 0) {
                    for (int i = 0; i < dwelling.residents.Count; i++) {
                        Character currResident = dwelling.residents[i];
                        if (currResident.relationshipContainer.GetRelationshipEffectWith(actor.currentAlterEgo) == RELATIONSHIP_EFFECT.POSITIVE) {
                            return true;
                        }
                    }
                    //the actor does NOT have any positive relations with any resident
                    return false;
                } else {
                    //in cases that the guitar is at a dwelling with no residents, always allow.
                    return true;
                }
            }
        } else {
            //in cases that the guitar is not inside a dwelling, always allow.
            return true;
        }
    }
}