﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class Lycanthrope : Trait {

        private Character _character;

        public override bool isPersistent { get { return true; } }

        private int _level;
        public Lycanthrope() {
            name = "Lycanthrope";
            description = "Lycanthropes transform into wolves when they sleep.";
            thoughtText = "[Character] can transform into a wolf.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            
            ticksDuration = 0;
            canBeTriggered = true;
            //effects = new List<TraitEffect>();
            //advertisedInteractions = new List<INTERACTION_TYPE>() { INTERACTION_TYPE.TRANSFORM_TO_WOLF, INTERACTION_TYPE.REVERT_TO_NORMAL };
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            if (sourceCharacter is Character) {
                _character = sourceCharacter as Character;
                //_character.RegisterLogAndShowNotifToThisCharacterOnly("NonIntel", "afflicted", null, name);
                AlterEgoData lycanthropeAlterEgo = _character.CreateNewAlterEgo("Lycanthrope");

                //setup all alter ego data
                lycanthropeAlterEgo.SetFaction(FactionManager.Instance.neutralFaction);
                lycanthropeAlterEgo.SetRace(RACE.WOLF);
                lycanthropeAlterEgo.SetRole(CharacterRole.BEAST);
                lycanthropeAlterEgo.SetCharacterClass(CharacterManager.Instance.CreateNewCharacterClass(Utilities.GetRespectiveBeastClassNameFromByRace(RACE.WOLF)));
                lycanthropeAlterEgo.SetLevel(level);
                lycanthropeAlterEgo.AddTrait(new Nocturnal());
                //foreach (List<LocationStructure> structures in _character.specificLocation.structures.Values) {
                //    for (int i = 0; i < structures.Count; i++) {
                //        for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
                //            IPointOfInterest poi = structures[i].pointsOfInterest[j];
                //            if (poi is TileObject) {
                //                TileObject tileObj = poi as TileObject;
                //                if (tileObj.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL || tileObj.tileObjectType == TILE_OBJECT_TYPE.EDIBLE_PLANT) {
                //                    lycanthropeAlterEgo.AddAwareness(tileObj);
                //                }
                //            }
                //        }
                //    }
                //}
            }

            base.OnAddTrait(sourceCharacter);
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            _character.RemoveAlterEgo("Lycanthrope");
            _character = null;
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) {
            base.ExecuteActionPerTickEffects(action, goapNode);
            //if (action == INTERACTION_TYPE.NAP || action == INTERACTION_TYPE.SLEEP || action == INTERACTION_TYPE.SLEEP_OUTSIDE || action == INTERACTION_TYPE.NARCOLEPTIC_NAP) {
            if (_character.traitContainer.GetNormalTrait<Trait>("Resting") != null) {
                    CheckForLycanthropy();
            }
        }
        #endregion

        public void CheckForLycanthropy(bool forceTransform = false) {
            int chance = UnityEngine.Random.Range(0, 100);
            //TODO:
            //if (restingTrait.lycanthropyTrait == null) {
            //    if (currentState.currentDuration == currentState.duration) {
            //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
            //        bool isTargettedByDrinkBlood = false;
            //        for (int i = 0; i < actor.targettedByAction.Count; i++) {
            //            if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
            //                isTargettedByDrinkBlood = true;
            //                break;
            //            }
            //        }
            //        if (isTargettedByDrinkBlood) {
            //            currentState.OverrideDuration(currentState.duration + 1);
            //        }
            //    }
            //} else {
            //    bool isTargettedByDrinkBlood = false;
            //    for (int i = 0; i < actor.targettedByAction.Count; i++) {
            //        if (actor.targettedByAction[i].goapType == INTERACTION_TYPE.DRINK_BLOOD && !actor.targettedByAction[i].isDone && actor.targettedByAction[i].isPerformingActualAction) {
            //            isTargettedByDrinkBlood = true;
            //            break;
            //        }
            //    }
            //    if (currentState.currentDuration == currentState.duration) {
            //        //If sleep will end, check if the actor is being targetted by Drink Blood action, if it is, do not end sleep
            //        if (isTargettedByDrinkBlood) {
            //            currentState.OverrideDuration(currentState.duration + 1);
            //        } else {
            //            if (!restingTrait.hasTransformed) {
            //                restingTrait.CheckForLycanthropy(true);
            //            }
            //        }
            //    } else {
            //        if (!isTargettedByDrinkBlood) {
            //            restingTrait.CheckForLycanthropy();
            //        }
            //    }
            //}
            if (_character.race == RACE.WOLF) {
                //Turn back to normal form
                if (forceTransform || chance < 1) {
                    PlanRevertToNormal();
                }
            } else {
                //Turn to wolf
                if (forceTransform || chance < 40) {
                    PlanTransformToWolf();
                }
            }
        }

        public void PlanTransformToWolf() {
            _character.currentActionNode?.EndPerTickEffect();
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.TRANSFORM_TO_WOLF_FORM, _character, _character);
            _character.jobQueue.AddJobInQueue(job);
        }
        public void PlanRevertToNormal() {
            _character.currentActionNode?.EndPerTickEffect();
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MISC, INTERACTION_TYPE.REVERT_TO_NORMAL_FORM, _character, _character);
            _character.jobQueue.AddJobInQueue(job);
        }
        public void TurnToWolf() {
            ////Drop all plans except for the current action
            //_character.AdjustIsWaitingForInteraction(1);
            //_character.DropAllPlans(_character.currentActionNode.action.parentPlan);
            //_character.AdjustIsWaitingForInteraction(-1);

            ////Copy non delicate data
            //data.SetData(_character);

            //_character.SetHomeStructure(null);

            ////Reset needs
            //_character.ResetFullnessMeter();
            //_character.ResetHappinessMeter();
            //_character.ResetTirednessMeter();


            ////Remove all awareness then add all edible plants and small animals of current location to awareness
            //_character.awareness.Clear();
            //foreach (List<LocationStructure> structures in _character.specificLocation.structures.Values) {
            //    for (int i = 0; i < structures.Count; i++) {
            //        for (int j = 0; j < structures[i].pointsOfInterest.Count; j++) {
            //            IPointOfInterest poi = structures[i].pointsOfInterest[j];
            //            if(poi is TileObject) {
            //                TileObject tileObj = poi as TileObject;
            //                if(tileObj.tileObjectType == TILE_OBJECT_TYPE.SMALL_ANIMAL || tileObj.tileObjectType == TILE_OBJECT_TYPE.EDIBLE_PLANT) {
            //                    _character.AddAwareness(tileObj);
            //                }
            //            }
            //        }
            //    }
            //}

            ////Copy relationship data then remove them
            ////data.SetRelationshipData(_character);
            ////_character.RemoveAllRelationships(false);
            //foreach (Character target in _character.relationships.Keys) {
            //    CharacterManager.Instance.SetIsDisabledRelationshipBetween(_character, target, true);
            //}

            ////Remove race and class
            ////This is done first so that when the traits are copied, it will not copy the traits from the race and class because if it is copied and the race and character is brought back, it will be doubled, which is not what we want
            //_character.RemoveRace();
            //_character.RemoveClass();

            ////Copy traits and then remove them
            //data.SetTraits(_character);
            //_character.RemoveAllNonRelationshipTraits("Lycanthrope");

            ////Change faction and race
            //_character.ChangeFactionTo(FactionManager.Instance.neutralFaction);
            //_character.SetRace(RACE.WOLF);

            ////Change class and role
            //_character.AssignRole(CharacterRole.BEAST);
            //_character.AssignClassByRole(_character.role);

            //Messenger.Broadcast(Signals.CHARACTER_CHANGED_RACE, _character);

            //_character.CancelAllJobsTargettingThisCharacter("target is not found", false);
            //Messenger.Broadcast(Signals.CANCEL_CURRENT_ACTION, _character, "target is not found");

            _character.SwitchAlterEgo("Lycanthrope");
            //Plan idle stroll to the wilderness
            LocationStructure wilderness = _character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            LocationGridTile targetTile = wilderness.GetRandomTile();
            _character.PlanIdleStroll(wilderness, targetTile);
        }

        public void RevertToNormal() {
            ////Drop all plans except for the current action
            //_character.AdjustIsWaitingForInteraction(1);
            //_character.DropAllPlans(_character.currentActionNode.action.parentPlan);
            //_character.AdjustIsWaitingForInteraction(-1);

            ////Revert back data including awareness
            //_character.SetFullness(data.fullness);
            //_character.SetTiredness(data.tiredness);
            //_character.SetHappiness(data.happiness);
            //_character.CopyAwareness(data.awareness);
            //_character.SetHomeStructure(data.homeStructure);
            //_character.ChangeFactionTo(data.faction);
            //_character.ChangeRace(data.race);
            //_character.AssignRole(data.role);
            //_character.AssignClass(data.characterClass);

            ////Bring back lost relationships
            //foreach (Character target in _character.relationships.Keys) {
            //    CharacterManager.Instance.SetIsDisabledRelationshipBetween(_character, target, false);
            //}

            ////Revert back the traits
            //for (int i = 0; i < data.traits.Count; i++) {
            //    _character.AddTrait(data.traits[i]);
            //}

            _character.SwitchAlterEgo(CharacterManager.Original_Alter_Ego);
        }

        public override string TriggerFlaw(Character character) {
            if (IsAlone()) {
                DoTransformWolf();
            } else {
                //go to a random tile in the wilderness
                //then check if the character is alone, if not pick another random tile,
                //repeat the process until alone, then transform to wolf
                LocationStructure wilderness = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                //LocationGridTile randomWildernessTile = wilderness.tiles[Random.Range(0, wilderness.tiles.Count)];
                //character.marker.GoTo(randomWildernessTile, CheckIfAlone);
                character.PlanAction(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, character, new object[] { wilderness });
            }
            return base.TriggerFlaw(character);
        }

        public void CheckIfAlone() {
            if (IsAlone()) {
                //alone
                DoTransformWolf();
            } else {
                //go to a different tile
                LocationStructure wilderness = _character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
                //LocationGridTile randomWildernessTile = wilderness.tiles[Random.Range(0, wilderness.tiles.Count)];
                //character.marker.GoTo(randomWildernessTile, CheckIfAlone);
                _character.PlanAction(JOB_TYPE.TRIGGER_FLAW, INTERACTION_TYPE.STEALTH_TRANSFORM, _character, new object[] { wilderness });
            }
        }
        private bool IsAlone() {
            return _character.marker.inVisionCharacters.Count == 0;
        }
        private void DoTransformWolf() {
            if (_character.currentActionNode != null) {
                _character.StopCurrentActionNode(false);
            }
            if (_character.stateComponent.currentState != null) {
                _character.stateComponent.ExitCurrentState();
            }

            PlanTransformToWolf();
        }
    }

    public class LycanthropeData {
        public float fullness { get; private set; }
        public float tiredness { get; private set; }
        public float happiness { get; private set; }
        public Faction faction { get; private set; }
        public Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>> awareness { get; private set; }
        //public List<RelationshipLycanthropyData> relationships { get; private set; }
        public List<Trait> traits { get; set; }
        public IDwelling homeStructure { get; private set; }
        public CharacterClass characterClass { get; private set; }
        public CharacterRole role { get; private set; }
        public RACE race { get; private set; }

        public void SetData(Character character) {
            this.fullness = character.needsComponent.fullness;
            this.tiredness = character.needsComponent.tiredness;
            this.happiness = character.needsComponent.happiness;
            this.faction = character.faction;
            //this.awareness = new Dictionary<POINT_OF_INTEREST_TYPE, List<IPointOfInterest>>(character.awareness);
            this.homeStructure = character.homeStructure;
            this.race = character.race;
            this.role = character.role;
            this.characterClass = character.characterClass;
        }

        //public void SetRelationshipData(Character character) {
        //    this.relationships = new List<RelationshipLycanthropyData>();
        //    foreach (KeyValuePair<Character, CharacterRelationshipData> kvp in character.relationships) {
        //        this.relationships.Add(new RelationshipLycanthropyData(kvp.Key, kvp.Value, kvp.Key.relationshipContainer.GetRelationshipDataWith(character)));
        //    }
        //}
        //public void SetTraits(Character character) {
        //    this.traits = new List<Trait>();
        //    for (int i = 0; i < character.allTraits.Count; i++) {
        //        if(character.allTraits[i].name != "Lycanthrope" && !(character.allTraits[i] is RelationshipTrait)) {
        //            this.traits.Add(character.allTraits[i]);
        //        }
        //    }
        //}
    }

    //public class RelationshipLycanthropyData {
    //    public Character target { get; private set; }
    //    public CharacterRelationshipData characterToTargetRelData { get; private set; }
    //    public CharacterRelationshipData targetToCharacterRelData { get; private set; }

    //    public RelationshipLycanthropyData(Character target, CharacterRelationshipData characterToTargetRelData, CharacterRelationshipData targetToCharacterRelData) {
    //        this.target = target;
    //        this.characterToTargetRelData = characterToTargetRelData;
    //        this.targetToCharacterRelData = targetToCharacterRelData;
    //    }
    //}

}
