﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Paralyzed : Trait {

        public Character owner { get; private set; }
        //public List<Character> charactersThatKnow { get; private set; }

        public Paralyzed() {
            name = "Paralyzed";
            description = "This character is paralyzed.";
            thoughtText = "[Character] is paralyzed.";
            type = TRAIT_TYPE.DISABLER;
            effect = TRAIT_EFFECT.NEGATIVE;
            advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.FEED };
            ticksDuration = 0;
            //charactersThatKnow = new List<Character>();
            hindersMovement = true;
            hindersPerform = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable addedTo) {
            base.OnAddTrait(addedTo);
            if (addedTo is Character) {
                owner = addedTo as Character;
                owner.CancelAllJobs();
                //Messenger.AddListener(Signals.TICK_STARTED, CheckParalyzedTrait);
                //Messenger.AddListener(Signals.HOUR_STARTED, CheckParalyzedTraitPerHour);
                Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (owner != null) {
                //Messenger.RemoveListener(Signals.TICK_STARTED, CheckParalyzedTrait);
                //Messenger.RemoveListener(Signals.HOUR_STARTED, CheckParalyzedTraitPerHour);
                Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) {
            if (traitOwner is Character) {
                Character targetCharacter = traitOwner as Character;
                if (!targetCharacter.isDead) {
                    if (targetCharacter.faction == characterThatWillDoJob.faction) {
                        if (characterThatWillDoJob.opinionComponent.GetRelationshipEffectWith(targetCharacter) != RELATIONSHIP_EFFECT.NEGATIVE) {
                            if (/*owner.CanPerformEndTickJobs() &&*/!owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
                                if (!CreateDropJobForTirednessRecovery(characterThatWillDoJob)) {
                                    CreateDropJobForHappinessRecovery(characterThatWillDoJob);
                                }
                            }
                        }
                    }
                }
            }
            return base.CreateJobsOnEnterVisionBasedOnTrait(traitOwner, characterThatWillDoJob);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            CheckParalyzedTrait();
        }
        // public override void OnHourStarted() {
        //     base.OnHourStarted();
        //     CheckParalyzedTraitPerHour();
        // }
        #endregion

        //public void AddCharacterThatKnows(Character character) {
        //    if (!charactersThatKnow.Contains(character)) {
        //        charactersThatKnow.Add(character);
        //    }
        //}
        //private bool CanPlanGoap() {
        //    //If there is no settlement, it means that there is no inner map, so character must not do goap actions, jobs, and plans
        //    //characters that cannot witness, cannot plan actions.
        //    return character.minion == null && !character.isDead && character.isStoppedByOtherCharacter <= 0 && character.canWitness
        //        && character.currentActionNode == null && character.planner.status == GOAP_PLANNING_STATUS.NONE && character.jobQueue.jobsInQueue.Count <= 0
        //        && !character.marker.hasFleePath && character.stateComponent.currentState == null && character.IsInOwnParty();
        //}
        private void CheckParalyzedTrait() {
            if(owner.marker == null) {
                return;
            }
            if (!owner.CanPerformEndTickJobs()) {
                return;
            }
            if (owner.HasJobTargetingThis(JOB_TYPE.MOVE_CHARACTER)) {
                return;
            }
            if (owner.jobQueue.jobsInQueue.Count > 0) {
                owner.PerformTopPriorityJob();
            } else {
                if (!PlanTirednessRecovery()) {
                    PlanHappinessRecovery();
                }
            }
        }
        // private void CheckParalyzedTraitPerHour() {
        //     if (owner.marker == null) {
        //         return;
        //     }
        //     if (/*owner.CanPerformEndTickJobs()*/
        //             /*&& */(owner.needsComponent.isStarving || owner.needsComponent.isExhausted || owner.needsComponent.isForlorn || owner.traitContainer.GetNormalTrait<Trait>("Burning") != null)
        //             && UnityEngine.Random.Range(0, 100) < 75 && !owner.jobQueue.HasJob(JOB_TYPE.SCREAM)
        //             && owner.traitContainer.GetNormalTrait<Trait>("Unconscious", "Resting") == null
        //             && !owner.HasJobTargetingThis(JOB_TYPE.DROP, JOB_TYPE.FEED)) {
        //         GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.SCREAM, INTERACTION_TYPE.SCREAM_FOR_HELP, owner, owner);
        //         owner.jobQueue.AddJobInQueue(job);
        //     }
        // }

        #region Carry/Drop
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, owner, characterThatWillDoJob);
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure });
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private bool CreateActualDropJob(Character characterThatWillDoJob, LocationStructure dropLocationStructure, LocationGridTile dropGridTile) {
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MOVE_CHARACTER, INTERACTION_TYPE.DROP, owner, characterThatWillDoJob);
            //job.SetCanTakeThisJobChecker(InteractionManager.Instance.CanCharacterTakeDropJob);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { dropLocationStructure, dropGridTile });
            characterThatWillDoJob.jobQueue.AddJobInQueue(job);
            return true;
        }
        private void OnCharacterFinishedAction(ActualGoapNode node) {
            if (node.action.goapType == INTERACTION_TYPE.DROP && node.poiTarget == this.owner) {
                if (this.owner.gridTileLocation.objHere != null && this.owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(this.owner.gridTileLocation.objHere as Bed);
                } else if (this.owner.gridTileLocation.structure == this.owner.homeStructure) {
                    CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                } else {
                    CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                }
            }
        }
        #endregion

        #region Happiness Recovery
        private bool CreateDropJobForHappinessRecovery(Character characterThatWillDoJob) {
            if (owner.needsComponent.isForlorn || owner.needsComponent.isLonely) {
                if ((owner.homeStructure != null && owner.currentStructure != owner.homeStructure) &&
                    (owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS) && owner.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS)) {
                    int chance = UnityEngine.Random.Range(0, 2);
                    if (chance == 0) {
                        return CreateActualDropJob(characterThatWillDoJob, owner.homeStructure.GetLocationStructure());
                    } else {
                        return CreateActualDropJob(characterThatWillDoJob, owner.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS));
                    }
                }
            }
            return false;
        }
        private bool PlanHappinessRecovery() {
            if ((owner.needsComponent.isForlorn || owner.needsComponent.isLonely) && !owner.HasJobTargetingThis(JOB_TYPE.HAPPINESS_RECOVERY)) {
                return CreateDaydreamOrPrayJob();
            }
            return false;
        }
        private bool CreateDaydreamOrPrayJob() {
            if (owner.currentRegion.IsResident(owner)) {
                if (owner.homeStructure != null && owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
                    if (owner.currentStructure == owner.homeStructure) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                        return true;
                    } else if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                        return true;
                    }
                } else {
                    if (UnityEngine.Random.Range(0, 2) == 0) {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                        return true;
                    } else {
                        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                        return true;
                    }
                }
            }
            return false;
        }
        private bool CreatePrayJob() {
            if (owner.homeStructure == null || owner.currentStructure == owner.homeStructure) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
                return true;
            }
            //else {
            //    if (character.homeStructure != null) {
            //        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.PRAY);
            //        return true;
            //    }
            //}
            return false;
        }
        private bool CreateDaydreamJob() {
            if (owner.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS || !owner.currentRegion.HasStructure(STRUCTURE_TYPE.WILDERNESS)) {
                CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
                return true;
            }
            //else {
            //    LocationStructure structure = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WORK_AREA);
            //    if (structure != null) {
            //        return CreateActualDropJob(characterThatWillDoJob, structure);
            //    } else {
            //        CreateActualHappinessRecoveryJob(INTERACTION_TYPE.DAYDREAM);
            //        return true;
            //    }
            //}
            return false;
        }
        private void CreateActualHappinessRecoveryJob(INTERACTION_TYPE actionType) {
            bool triggerBrokenhearted = false;
            Heartbroken heartbroken = owner.traitContainer.GetNormalTrait<Trait>("Heartbroken") as Heartbroken;
            if (heartbroken != null) {
                triggerBrokenhearted = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerBrokenhearted) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HAPPINESS_RECOVERY, actionType, owner, owner);
                //job.AddOtherData(actionType, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                owner.jobQueue.AddJobInQueue(job);
            } else {
                heartbroken.TriggerBrokenhearted();
            }
        }
        #endregion

        #region Tiredness Recovery
        private bool CreateDropJobForTirednessRecovery(Character characterThatWillDoJob) {
            if (owner.needsComponent.isExhausted || owner.needsComponent.isTired) {
                if (owner.homeStructure != null && (owner.gridTileLocation.objHere == null || !(owner.gridTileLocation.objHere is Bed))) {
                    TileObject bed = owner.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                    if (bed != null) {
                        return CreateActualDropJob(characterThatWillDoJob, owner.homeStructure.GetLocationStructure(), bed.gridTileLocation);
                    }
                }
            }
            return false;
        }
        private bool PlanTirednessRecovery() {
            if ((owner.needsComponent.isExhausted || owner.needsComponent.isTired) && !owner.HasJobTargetingThis(JOB_TYPE.ENERGY_RECOVERY_NORMAL, JOB_TYPE.ENERGY_RECOVERY_URGENT)) {
                return CreateSleepJob();
            }
            return false;
        }
        private bool CreateSleepJob() {
            if (owner.homeStructure != null) {
                if (owner.gridTileLocation.objHere != null && owner.gridTileLocation.objHere is Bed) {
                    CreateActualSleepJob(owner.gridTileLocation.objHere as Bed);
                    return true;
                }
                //else {
                //    TileObject bed = character.homeStructure.GetUnoccupiedTileObject(TILE_OBJECT_TYPE.BED);
                //    if(bed != null){
                //        return CreateActualDropJob(characterThatWillDoJob, character.homeStructure, bed.gridTileLocation);
                //    }
                //}
            }
            return false;
        }
        private void CreateActualSleepJob(Bed bed) {
            JOB_TYPE jobType = JOB_TYPE.ENERGY_RECOVERY_NORMAL;
            if (owner.needsComponent.isExhausted) {
                jobType = JOB_TYPE.ENERGY_RECOVERY_URGENT;
            }
            bool triggerSpooked = false;
            Spooked spooked = owner.traitContainer.GetNormalTrait<Trait>("Spooked") as Spooked;
            if (spooked != null) {
                triggerSpooked = UnityEngine.Random.Range(0, 100) < 20;
            }
            if (!triggerSpooked) {
                GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(jobType, INTERACTION_TYPE.SLEEP, bed, owner);
                //job.AddOtherData(INTERACTION_TYPE.SLEEP, new object[] { ACTION_LOCATION_TYPE.IN_PLACE });
                owner.jobQueue.AddJobInQueue(job);
            } else {
                spooked.TriggerFeelingSpooked();
            }
        }
        #endregion
    }


    //public class SaveDataParalyzed : SaveDataTrait {
    //    public List<int> charactersThatKnow;

    //    public override void Save(Trait trait) {
    //        base.Save(trait);
    //        Paralyzed paralyzed = trait as Paralyzed;
    //        charactersThatKnow = new List<int>();
    //        for (int i = 0; i < paralyzed.charactersThatKnow.Count; i++) {
    //            charactersThatKnow.Add(paralyzed.charactersThatKnow[i].id);
    //        }
    //    }

    //    public override Trait Load(ref Character responsibleCharacter) {
    //        Trait trait = base.Load(ref responsibleCharacter);
    //        Paralyzed paralyzed = trait as Paralyzed;
    //        for (int i = 0; i < charactersThatKnow.Count; i++) {
    //            paralyzed.AddCharacterThatKnows(CharacterManager.Instance.GetCharacterByID(charactersThatKnow[i]));
    //        }
    //        return trait;
    //    }
    //}
}
