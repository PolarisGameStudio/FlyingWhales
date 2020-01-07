﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;

namespace Traits {
    public class SerialKiller : Trait {

        public SerialVictim victim1Requirement { get; private set; }
        //public SerialVictim victim2Requirement { get; private set; }
        public Character character { get; private set; }

        public Character targetVictim { get; private set; }
        public bool isFollowing { get; private set; }
        public bool hasStartedFollowing { get; private set; }

        public SerialKiller() {
            name = "Serial Killer";
            description = "Serial killers have a specific subset of target victims that they may kidnap and then kill.";
            thoughtText = "[Character] is a serial killer.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            
            
            
            ticksDuration = 0;
            canBeTriggered = true;
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                character = sourceCharacter as Character;
                if (victim1Requirement == null) { // || victim2Requirement == null
                    GenerateSerialVictims();
                }
                //Messenger.AddListener(Signals.TICK_STARTED, CheckSerialKiller);
                Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (character != null) {
                //Messenger.RemoveListener(Signals.TICK_STARTED, CheckSerialKiller);
                Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override void OnSeePOI(IPointOfInterest targetPOI, Character character) {
            base.OnSeePOI(targetPOI, character);
            if (targetPOI is Character) {
                Character potentialVictim = targetPOI as Character;
                CheckTargetVictimIfStillAvailable();
                if (targetVictim == null) {
                    if (DoesCharacterFitAnyVictimRequirements(potentialVictim)) {
                        SetTargetVictim(potentialVictim);

                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_new_victim");
                        log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        this.character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                    }
                }
            }
        }
        public override string TriggerFlaw(Character character) {
            if (!ForceHuntVictim()) {
                return "fail";
            }
            return base.TriggerFlaw(character);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            CheckSerialKiller();
        }
        #endregion

        public void SetVictim1Requirement(SerialVictim serialVictim) {
            victim1Requirement = serialVictim;
        }
        //public void SetVictim2Requirement(SerialVictim serialVictim) {
        //    victim2Requirement = serialVictim;
        //}
        public void SetTargetVictim(Character victim) {
            if (targetVictim != null) {
                //TODO: Add checking if character is the target of any other serial killer
                targetVictim.RemoveAdvertisedAction(INTERACTION_TYPE.RITUAL_KILLING);
            }
            if (victim != null) {
                victim.AddAdvertisedAction(INTERACTION_TYPE.RITUAL_KILLING);
            }
            targetVictim = victim;
        }
        public void SetIsFollowing(bool state) {
            isFollowing = state;
        }
        public void SetHasStartedFollowing(bool state) {
            if (hasStartedFollowing != state) {
                hasStartedFollowing = state;
            }
        }

        private void OnCharacterDied(Character deadCharacter) {
            if (deadCharacter == targetVictim) {
                CheckTargetVictimIfStillAvailable();
            }
        }

        private void CheckSerialKiller() {
            if (character.isDead || character.doNotDisturb || !character.canMove) { //character.doNotDisturb > 0 || !character.canMove //character.currentArea != InnerMapManager.Instance.currentlyShowingArea
                if (hasStartedFollowing) {
                    StopFollowing();
                    SetHasStartedFollowing(false);
                }
                return;
            }
            if (character.jobQueue.HasJob(JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM)) {
                return;
            }
            if (!hasStartedFollowing) {
                HuntVictim();
            } else {
                CheckerWhileFollowingTargetVictim();
            }
        }


        private void HuntVictim() {
            if (character.needsComponent.isForlorn || character.needsComponent.isLonely) {
                int chance = UnityEngine.Random.Range(0, 100);
                if (chance < 20) {
                    CheckTargetVictimIfStillAvailable();
                    if (targetVictim != null) {
                        character.CancelAllJobs();
                        if (character.stateComponent.currentState != null) {
                            character.stateComponent.ExitCurrentState();
                            //if (character.stateComponent.currentState != null) {
                            //    character.stateComponent.currentState.OnExitThisState();
                            //}
                        }
                        FollowTargetVictim();
                        SetHasStartedFollowing(true);
                    }
                }
            }
        }
        private bool ForceHuntVictim() {
            CheckTargetVictimIfStillAvailable();
            if (hasStartedFollowing && targetVictim != null) {
                return true;
            }
            if (targetVictim == null) {
                for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
                    Character potentialVictim = CharacterManager.Instance.allCharacters[i];
                    if (potentialVictim.currentRegion != this.character.currentRegion || potentialVictim.isDead || potentialVictim is Summon) {
                        continue;
                    }
                    if (DoesCharacterFitAnyVictimRequirements(potentialVictim)) {
                        SetTargetVictim(potentialVictim);

                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_new_victim");
                        log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        this.character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                        break;
                    }
                }
            }
            if (targetVictim != null) {
                character.CancelAllJobs();
                if (character.stateComponent.currentState != null) {
                    character.stateComponent.ExitCurrentState();
                    //if (character.stateComponent.currentState != null) {
                    //    character.stateComponent.currentState.OnExitThisState();
                    //}
                }
                FollowTargetVictim();
                SetHasStartedFollowing(true);
                return true;
            }
            return false;
        }
        private void CheckerWhileFollowingTargetVictim() {
            if (isFollowing) {
                if (!character.currentParty.icon.isTravelling || character.marker.targetPOI != targetVictim) {
                    SetIsFollowing(false);
                    if (character.marker.targetPOI != targetVictim) {
                        SetHasStartedFollowing(false);
                    }
                    return;
                }

                CheckTargetVictimIfStillAvailable();
                if (targetVictim != null) {
                    if (character.marker.inVisionCharacters.Contains(targetVictim)) {
                        StopFollowing();
                    }
                    if (character.marker.CanDoStealthActionToTarget(targetVictim)) {
                        CreateHuntVictimJob();
                    }
                }
            } else {
                CheckTargetVictimIfStillAvailable();
                if (targetVictim != null) {
                    if (!character.marker.inVisionCharacters.Contains(targetVictim)) {
                        FollowTargetVictim();
                    } else if (character.marker.CanDoStealthActionToTarget(targetVictim)) {
                        CreateHuntVictimJob();
                    }
                } else {
                    SetHasStartedFollowing(false);
                }
            }
        }
        private void StopFollowing() {
            if (isFollowing) {
                SetIsFollowing(false);
                character.marker.StopMovement();
            }
        }
        private void FollowTargetVictim() {
            if (!isFollowing) {
                SetIsFollowing(true);
                character.marker.GoToPOI(targetVictim);
            }
        }
        private void CheckTargetVictimIfStillAvailable() {
            if (targetVictim != null) {
                if (targetVictim.currentRegion != this.character.currentRegion || targetVictim.isDead || targetVictim is Summon) {
                    SetTargetVictim(null);
                    if (hasStartedFollowing) {
                        StopFollowing();
                        SetHasStartedFollowing(false);
                    }
                }
            }
        }
        public void CreateHuntVictimJob() {
            if (character.jobQueue.HasJob(JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM)) {
                return;
            }
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM, INTERACTION_TYPE.RITUAL_KILLING, targetVictim, character);
            //GoapAction goapAction6 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.BURY_CHARACTER, character, targetVictim);
            //GoapAction goapAction5 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.CARRY_CORPSE, character, targetVictim);
            //GoapAction goapAction4 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RITUAL_KILLING, character, targetVictim);
            //GoapAction goapAction3 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ABDUCT_CHARACTER, character, targetVictim);
            //GoapAction goapAction2 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RESTRAIN_CARRY_CHARACTER, character, targetVictim);
            ////GoapAction goapAction2 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RESTRAIN_CHARACTER, character, targetVictim);
            //GoapAction goapAction1 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.KNOCKOUT_CHARACTER, character, targetVictim);

            ////goapAction3.SetWillAvoidCharactersWhileMoving(true);
            ////goapAction6.SetWillAvoidCharactersWhileMoving(true);

            //LocationStructure wilderness = character.specificLocation.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            //if (character.homeStructure.residents.Count > 1) {
            //    goapAction3.InitializeOtherData(new object[] { wilderness });
            //} else {
            //    goapAction3.InitializeOtherData(new object[] { character.homeStructure });
            //}
            //goapAction6.InitializeOtherData(new object[] { wilderness });

            //GoapNode goalNode = new GoapNode(null, goapAction6.cost, goapAction6);
            ////GoapNode sixthNode = new GoapNode(goalNode, goapAction5.cost, goapAction5);
            //GoapNode fifthNode = new GoapNode(goalNode, goapAction5.cost, goapAction5);
            //GoapNode fourthNode = new GoapNode(fifthNode, goapAction4.cost, goapAction4);
            //GoapNode thirdNode = new GoapNode(fourthNode, goapAction3.cost, goapAction3);
            //GoapNode secondNode = new GoapNode(thirdNode, goapAction2.cost, goapAction2);
            //GoapNode startingNode = new GoapNode(secondNode, goapAction1.cost, goapAction1);

            //GoapPlan plan = new GoapPlan(startingNode, new GOAP_EFFECT_CONDITION[] { GOAP_EFFECT_CONDITION.REMOVE_FROM_PARTY }, GOAP_CATEGORY.WORK);
            //plan.ConstructAllNodes();
            //plan.SetDoNotRecalculate(true);
            //job.AllowDeadTargets();
            //job.SetIsStealth(true);
            //job.SetAssignedPlan(plan);
            //job.SetAssignedCharacter(character);
            //job.SetCancelOnFail(true);

            character.jobQueue.AddJobInQueue(job);

            //character.AdjustIsWaitingForInteraction(1);
            //if (character.stateComponent.currentState != null) {
            //    character.stateComponent.currentState.OnExitThisState();
            //    //This call is doubled so that it will also exit the previous major state if there's any
            //    if (character.stateComponent.currentState != null) {
            //        character.stateComponent.currentState.OnExitThisState();
            //    }
            //} else {
            //    if (character.currentParty.icon.isTravelling) {
            //        if (character.currentParty.icon.travelLine == null) {
            //            character.marker.StopMovement();
            //        } else {
            //            character.currentParty.icon.SetOnArriveAction(() => character.OnArriveAtAreaStopMovement());
            //        }
            //    }
            //    character.StopCurrentAction(false);
            //}
            //character.AdjustIsWaitingForInteraction(-1);

            //character.AddPlan(plan, true);

            //if (hasStartedFollowing) {
            //    StopFollowing();
            //    SetHasStartedFollowing(false);
            //}
        }
        private void GenerateSerialVictims() {
            SetVictim1Requirement(new SerialVictim(RandomizeVictimType(true), RandomizeVictimType(false)));

            //bool hasCreatedRequirement = false;
            //while (!hasCreatedRequirement) {
            //    SERIAL_VICTIM_TYPE victim2FirstType = RandomizeVictimType(true);
            //    SERIAL_VICTIM_TYPE victim2SecondType = RandomizeVictimType(false);

            //    string victim2FirstDesc = victim1Requirement.GenerateVictimDescription(victim2FirstType);
            //    string victim2SecondDesc = victim1Requirement.GenerateVictimDescription(victim2SecondType);

            //    if(victim1Requirement.victimFirstType == victim2FirstType && victim1Requirement.victimSecondType == victim2SecondType
            //        && victim1Requirement.victimFirstDescription == victim2FirstDesc && victim1Requirement.victimSecondDescription == victim2SecondDesc) {
            //        continue;
            //    } else {
            //        SetVictim2Requirement(new SerialVictim(victim2FirstType, victim2FirstDesc, victim2SecondType, victim2SecondDesc));
            //        hasCreatedRequirement = true;
            //        break;
            //    }
            //}

            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "became_serial_killer");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, victim1Requirement.text, LOG_IDENTIFIER.STRING_1);
            //log.AddToFillers(null, victim2Requirement.text, LOG_IDENTIFIER.STRING_2);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        }

        private SERIAL_VICTIM_TYPE RandomizeVictimType(bool isPrefix) {
            int chance = UnityEngine.Random.Range(0, 2);
            if (isPrefix) {
                if (chance == 0) {
                    return SERIAL_VICTIM_TYPE.GENDER;
                }
                return SERIAL_VICTIM_TYPE.ROLE;
            } else {
                //if (chance == 0) {
                //    return SERIAL_VICTIM_TYPE.TRAIT;
                //}
                return SERIAL_VICTIM_TYPE.STATUS;
            }
        }

        private bool DoesCharacterFitAnyVictimRequirements(Character target) {
            return victim1Requirement.DoesCharacterFitVictimRequirements(target); //|| victim2Requirement.DoesCharacterFitVictimRequirements(target)
        }

        public void SerialKillerSawButWillNotAssist(Character targetCharacter, Trait negativeTrait) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_saw_no_assist");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(null, negativeTrait.name, LOG_IDENTIFIER.STRING_1);
            character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }
    }

    [System.Serializable]
    public class SerialVictim {
        public SERIAL_VICTIM_TYPE victimFirstType;
        public SERIAL_VICTIM_TYPE victimSecondType;
        public string victimFirstDescription;
        public string victimSecondDescription;

        public string text { get; private set; }

        public SerialVictim(SERIAL_VICTIM_TYPE victimFirstType, SERIAL_VICTIM_TYPE victimSecondType) {
            this.victimFirstType = victimFirstType;
            this.victimSecondType = victimSecondType;
            GenerateVictim();
        }
        public SerialVictim(SERIAL_VICTIM_TYPE victimFirstType, string victimFirstDesc, SERIAL_VICTIM_TYPE victimSecondType, string victimSecondDesc) {
            this.victimFirstType = victimFirstType;
            this.victimSecondType = victimSecondType;
            victimFirstDescription = victimFirstDesc;
            victimSecondDescription = victimSecondDesc;
            GenerateText();
        }

        private void GenerateVictim() {
            victimFirstDescription = GenerateVictimDescription(victimFirstType);
            victimSecondDescription = GenerateVictimDescription(victimSecondType);
            GenerateText();
        }
        public string GenerateVictimDescription(SERIAL_VICTIM_TYPE victimType) {
            if (victimType == SERIAL_VICTIM_TYPE.GENDER) {
                GENDER gender = Utilities.GetRandomEnumValue<GENDER>();
                return gender.ToString();
            } else if (victimType == SERIAL_VICTIM_TYPE.ROLE) {
                //CHARACTER_ROLE[] roles = new CHARACTER_ROLE[] { CHARACTER_ROLE.CIVILIAN, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER };
                string[] roles = new string[] { "Worker", "Combatant", "Royalty" };
                return roles[UnityEngine.Random.Range(0, roles.Length)];
            } 
            else if (victimType == SERIAL_VICTIM_TYPE.TRAIT) {
                string[] traits = new string[] { "Builder", "Criminal", "Drunk", "Sick", "Lazy", "Hardworking" }; //, "Curious"
                return traits[UnityEngine.Random.Range(0, traits.Length)];
            }
            else if (victimType == SERIAL_VICTIM_TYPE.STATUS) {
                string[] statuses = new string[] { "Hungry", "Tired", "Lonely" };
                return statuses[UnityEngine.Random.Range(0, statuses.Length)];
            }
            return string.Empty;
        }
        private void GenerateText() {
            string firstText = string.Empty;
            string secondText = string.Empty;
            if (victimSecondDescription != "Builder" && victimSecondDescription != "Criminal") {
                firstText = victimSecondDescription;
                secondText = PluralizeText(Utilities.NormalizeStringUpperCaseFirstLetters(victimFirstDescription));
            } else {
                firstText = Utilities.NormalizeStringUpperCaseFirstLetters(victimFirstDescription);
                secondText = PluralizeText(victimSecondDescription);
            }
            this.text = firstText + " " + secondText;
        }

        private string PluralizeText(string text) {
            string newText = text + "s";
            if (text.EndsWith("man")) {
                newText = text.Replace("man", "men");
            }else if (text.EndsWith("ty")) {
                newText = text.Replace("ty", "ties");
            }
            return newText;
        }

        public bool DoesCharacterFitVictimRequirements(Character character) {
            return DoesCharacterFitVictimTypeDescription(victimFirstType, victimFirstDescription, character)
                && DoesCharacterFitVictimTypeDescription(victimSecondType, victimSecondDescription, character);
        }
        private bool DoesCharacterFitVictimTypeDescription(SERIAL_VICTIM_TYPE victimType, string victimDesc, Character character) {
            if (victimType == SERIAL_VICTIM_TYPE.GENDER) {
                return victimDesc == character.gender.ToString();
            } else if (victimType == SERIAL_VICTIM_TYPE.ROLE) {
                return character.traitContainer.GetNormalTrait<Trait>(victimDesc) != null;
            } else if (victimType == SERIAL_VICTIM_TYPE.TRAIT) {
                return character.traitContainer.GetNormalTrait<Trait>(victimDesc) != null;
            } else if (victimType == SERIAL_VICTIM_TYPE.STATUS) {
                return character.traitContainer.GetNormalTrait<Trait>(victimDesc) != null;
            }
            return false;
        }
    }

    public class SaveDataSerialKiller : SaveDataTrait {
        public SerialVictim victim1Requirement;
        //public SerialVictim victim2Requirement;

        public int targetVictimID;
        public bool isFollowing;
        public bool hasStartedFollowing;

        public override void Save(Trait trait) {
            base.Save(trait);
            SerialKiller derivedTrait = trait as SerialKiller;
            victim1Requirement = derivedTrait.victim1Requirement;
            //victim2Requirement = derivedTrait.victim2Requirement;

            isFollowing = derivedTrait.isFollowing;
            hasStartedFollowing = derivedTrait.hasStartedFollowing;

            if (derivedTrait.targetVictim != null) {
                targetVictimID = derivedTrait.targetVictim.id;
            } else {
                targetVictimID = -1;
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            SerialKiller derivedTrait = trait as SerialKiller;
            derivedTrait.SetVictim1Requirement(victim1Requirement);
            //derivedTrait.SetVictim2Requirement(victim2Requirement);

            derivedTrait.SetIsFollowing(isFollowing);
            derivedTrait.SetHasStartedFollowing(hasStartedFollowing);

            if (targetVictimID != -1) {
                derivedTrait.SetTargetVictim(CharacterManager.Instance.GetCharacterByID(targetVictimID));
            }
            return trait;
        }
    }
}
