﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using System.Linq;

namespace Traits {
    public class SerialKiller : Trait {

        public SerialVictim victim1Requirement { get; private set; }
        //public SerialVictim victim2Requirement { get; private set; }
        public Character character { get; private set; }

        public Character targetVictim { get; private set; }
        //public bool isFollowing { get; private set; }
        //public bool hasStartedFollowing { get; private set; }
        private Dictionary<Character, OpinionData> opinionCopy;

        public SerialKiller() {
            name = "Serial Killer";
            description = "Serial killers have a specific subset of target victims that they may kidnap and then kill.";
            type = TRAIT_TYPE.FLAW;
            effect = TRAIT_EFFECT.NEUTRAL;
            ticksDuration = 0;
            canBeTriggered = true;
            opinionCopy = new Dictionary<Character, OpinionData>();
        }

        #region Overrides
        public override void OnAddTrait(ITraitable sourceCharacter) {
            base.OnAddTrait(sourceCharacter);
            if (sourceCharacter is Character) {
                character = sourceCharacter as Character;
                character.needsComponent.SetHappiness(50f);
                character.needsComponent.AdjustDoNotGetBored(1);
                CopyOpinionAndSetAllOpinionToZero();
                character.SetIsSerialKiller(true);
                //if (victim1Requirement == null) { // || victim2Requirement == null
                //    GenerateSerialVictims();
                //}
                //Messenger.AddListener(Signals.TICK_STARTED, CheckSerialKiller);
                //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                //Messenger.AddListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
                character.behaviourComponent.AddBehaviourComponent(typeof(SerialKillerBehaviour));
            }
        }
        public override void OnRemoveTrait(ITraitable sourceCharacter, Character removedBy) {
            if (character != null) {
                character.needsComponent.AdjustDoNotGetBored(-1);
                character.SetIsSerialKiller(false);
                BringBackOpinion();
                //Messenger.RemoveListener(Signals.TICK_STARTED, CheckSerialKiller);
                //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
                //Messenger.RemoveListener<Character>(Signals.CHARACTER_MISSING, OnCharacterMissing);
                character.behaviourComponent.RemoveBehaviourComponent(typeof(SerialKillerBehaviour));
            }
            base.OnRemoveTrait(sourceCharacter, removedBy);
        }
        public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) {
            if (targetPOI is Character) {
                Character potentialVictim = targetPOI as Character;
                CheckTargetVictimIfStillAvailable();
                if (targetVictim == null) {
                    if (!potentialVictim.isDead && DoesCharacterFitAnyVictimRequirements(potentialVictim)) {
                        SetTargetVictim(potentialVictim);

                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_new_victim");
                        log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        this.character.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                        return true;
                    }
                }
            }
            return base.OnSeePOI(targetPOI, characterThatWillDoJob);
        }
        public override string TriggerFlaw(Character character) {
            if (targetVictim == null) {
                for (int i = 0; i < character.currentRegion.charactersAtLocation.Count; i++) {
                    Character potentialVictim = character.currentRegion.charactersAtLocation[i];
                    if (IsCharacterApplicableAsVictim(potentialVictim)) {
                        continue;
                    }
                    if (!potentialVictim.isDead && DoesCharacterFitAnyVictimRequirements(potentialVictim)) {
                        SetTargetVictim(potentialVictim);

                        Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_new_victim");
                        log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                        log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
                        this.character.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
                        break;
                    }
                }
            }
            if (targetVictim == null || !CreateHuntVictimJob()) {
                return "fail";
            }
            return base.TriggerFlaw(character);
        }
        public override void OnTickStarted() {
            base.OnTickStarted();
            CheckSerialKiller();
        }
        #endregion

        public void SetVictimRequirements(SerialVictim serialVictim) {
            victim1Requirement = serialVictim;
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "became_serial_killer");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(null, victim1Requirement.text, LOG_IDENTIFIER.STRING_1);
            log.AddLogToInvolvedObjects();
            PlayerManager.Instance.player.ShowNotification(log);
        }
        public void SetVictimRequirements(SERIAL_VICTIM_TYPE victimFirstType, List<string> victimFirstDesc, SERIAL_VICTIM_TYPE victimSecondType, List<string> victimSecondDesc) {
            SetVictimRequirements(new SerialVictim(victimFirstType, victimFirstDesc, victimSecondType, victimSecondDesc));

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
        //public void SetIsFollowing(bool state) {
        //    isFollowing = state;
        //}
        //public void SetHasStartedFollowing(bool state) {
        //    if (hasStartedFollowing != state) {
        //        hasStartedFollowing = state;
        //    }
        //}
        private void OnCharacterDied(Character deadCharacter) {
            if (deadCharacter == targetVictim) {
                //CheckTargetVictimIfStillAvailable();
                SetTargetVictim(null);
            }
        }
        private void OnCharacterMissing(Character missingCharacter) {
            if (missingCharacter == targetVictim) {
                SetTargetVictim(null);
            }
        }
        private void CheckSerialKiller() {
            CheckTargetVictimIfStillAvailable();
            //if (character.isDead || !character.canPerform || !character.canMove) { //character.doNotDisturb > 0 || !character.canMove //character.currentArea != InnerMapManager.Instance.currentlyShowingArea
            //    if (hasStartedFollowing) {
            //        StopFollowing();
            //        SetHasStartedFollowing(false);
            //    }
            //    return;
            //}
            //if (character.jobQueue.HasJob(JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM)) {
            //    return;
            //}
            //if (!hasStartedFollowing) {
            //    HuntVictim();
            //} else {
            //    CheckerWhileFollowingTargetVictim();
            //}
        }
        //private void HuntVictim() {
        //    if (character.needsComponent.isSulking || character.needsComponent.isBored) {
        //        int chance = UnityEngine.Random.Range(0, 100);
        //        if (chance < 20) {
        //            //CheckTargetVictimIfStillAvailable();
        //            if (targetVictim != null) {
        //                character.CancelAllJobs();
        //                if (character.stateComponent.currentState != null) {
        //                    character.stateComponent.ExitCurrentState();
        //                    //if (character.stateComponent.currentState != null) {
        //                    //    character.stateComponent.currentState.OnExitThisState();
        //                    //}
        //                }
        //                FollowTargetVictim();
        //                SetHasStartedFollowing(true);
        //            }
        //        }
        //    }
        //}
        //private bool ForceHuntVictim() {
        //    CheckTargetVictimIfStillAvailable();
        //    if (hasStartedFollowing && targetVictim != null) {
        //        return true;
        //    }
        //    if (targetVictim == null) {
        //        for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++) {
        //            Character potentialVictim = CharacterManager.Instance.allCharacters[i];
        //            if (potentialVictim.currentRegion != this.character.currentRegion || potentialVictim.isDead || potentialVictim is Summon) {
        //                continue;
        //            }
        //            if (DoesCharacterFitAnyVictimRequirements(potentialVictim)) {
        //                SetTargetVictim(potentialVictim);

        //                Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_new_victim");
        //                log.AddToFillers(this.character, this.character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //                log.AddToFillers(targetVictim, targetVictim.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        //                this.character.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        //                break;
        //            }
        //        }
        //    }
        //    if (targetVictim != null) {
        //        character.CancelAllJobs();
        //        if (character.stateComponent.currentState != null) {
        //            character.stateComponent.ExitCurrentState();
        //            //if (character.stateComponent.currentState != null) {
        //            //    character.stateComponent.currentState.OnExitThisState();
        //            //}
        //        }
        //        FollowTargetVictim();
        //        SetHasStartedFollowing(true);
        //        return true;
        //    }
        //    return false;
        //}
        //private void CheckerWhileFollowingTargetVictim() {
        //    if (isFollowing) {
        //        if (!character.currentParty.icon.isTravelling || character.marker.targetPOI != targetVictim) {
        //            SetIsFollowing(false);
        //            if (character.marker.targetPOI != targetVictim) {
        //                SetHasStartedFollowing(false);
        //            }
        //            return;
        //        }

        //        CheckTargetVictimIfStillAvailable();
        //        if (targetVictim != null) {
        //            if (character.marker.inVisionCharacters.Contains(targetVictim)) {
        //                StopFollowing();
        //            }
        //            if (character.marker.CanDoStealthActionToTarget(targetVictim)) {
        //                CreateHuntVictimJob();
        //            }
        //        }
        //    } else {
        //        CheckTargetVictimIfStillAvailable();
        //        if (targetVictim != null) {
        //            if (!character.marker.inVisionCharacters.Contains(targetVictim)) {
        //                FollowTargetVictim();
        //            } else if (character.marker.CanDoStealthActionToTarget(targetVictim)) {
        //                CreateHuntVictimJob();
        //            }
        //        } else {
        //            SetHasStartedFollowing(false);
        //        }
        //    }
        //}
        //private void StopFollowing() {
        //    if (isFollowing) {
        //        SetIsFollowing(false);
        //        character.marker.StopMovement();
        //    }
        //}
        //private void FollowTargetVictim() {
        //    if (!isFollowing) {
        //        SetIsFollowing(true);
        //        character.marker.GoToPOI(targetVictim);
        //    }
        //}
        public void CheckTargetVictimIfStillAvailable() {
            if (targetVictim != null) {
                if (IsCharacterApplicableAsVictim(targetVictim)) {
                    SetTargetVictim(null);
                    //if (hasStartedFollowing) {
                    //    StopFollowing();
                    //    SetHasStartedFollowing(false);
                    //}
                }
            }
        }
        private bool IsCharacterApplicableAsVictim(Character target) {
            return target.currentRegion != character.currentRegion || target.isBeingSeized || target.isDead || target.isMissing;
        }
        public bool CreateHuntVictimJob() {
            if (character.jobQueue.HasJob(JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM)) {
                return false;
            }
            GoapPlanJob job = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_SERIAL_KILLER_VICTIM, INTERACTION_TYPE.RITUAL_KILLING, targetVictim, character);
            LocationStructure wilderness = character.currentRegion.GetRandomStructureOfType(STRUCTURE_TYPE.WILDERNESS);
            job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { character.homeStructure });
            // if (character.homeStructure != null && character.homeStructure.residents.Count > 1) {
            //     job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { wilderness });
            // } else {
            //     job.AddOtherData(INTERACTION_TYPE.DROP, new object[] { character.homeStructure });
            // }
            job.SetIsStealth(true);
            character.jobQueue.AddJobInQueue(job);
            return true;
            //GoapAction goapAction6 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.BURY_CHARACTER, character, targetVictim);
            //GoapAction goapAction5 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.CARRY_CORPSE, character, targetVictim);
            //GoapAction goapAction4 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RITUAL_KILLING, character, targetVictim);
            //GoapAction goapAction3 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.ABDUCT_CHARACTER, character, targetVictim);
            //GoapAction goapAction2 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RESTRAIN_CARRY_CHARACTER, character, targetVictim);
            ////GoapAction goapAction2 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.RESTRAIN_CHARACTER, character, targetVictim);
            //GoapAction goapAction1 = InteractionManager.Instance.CreateNewGoapInteraction(INTERACTION_TYPE.KNOCKOUT_CHARACTER, character, targetVictim);

            ////goapAction3.SetWillAvoidCharactersWhileMoving(true);
            ////goapAction6.SetWillAvoidCharactersWhileMoving(true);

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
        //private void GenerateSerialVictims() {
        //    SetVictim1Requirement(new SerialVictim(RandomizeVictimType(true), RandomizeVictimType(false)));

        //    //bool hasCreatedRequirement = false;
        //    //while (!hasCreatedRequirement) {
        //    //    SERIAL_VICTIM_TYPE victim2FirstType = RandomizeVictimType(true);
        //    //    SERIAL_VICTIM_TYPE victim2SecondType = RandomizeVictimType(false);

        //    //    string victim2FirstDesc = victim1Requirement.GenerateVictimDescription(victim2FirstType);
        //    //    string victim2SecondDesc = victim1Requirement.GenerateVictimDescription(victim2SecondType);

        //    //    if(victim1Requirement.victimFirstType == victim2FirstType && victim1Requirement.victimSecondType == victim2SecondType
        //    //        && victim1Requirement.victimFirstDescription == victim2FirstDesc && victim1Requirement.victimSecondDescription == victim2SecondDesc) {
        //    //        continue;
        //    //    } else {
        //    //        SetVictim2Requirement(new SerialVictim(victim2FirstType, victim2FirstDesc, victim2SecondType, victim2SecondDesc));
        //    //        hasCreatedRequirement = true;
        //    //        break;
        //    //    }
        //    //}

        //    Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "became_serial_killer");
        //    log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
        //    log.AddToFillers(null, victim1Requirement.text, LOG_IDENTIFIER.STRING_1);
        //    //log.AddToFillers(null, victim2Requirement.text, LOG_IDENTIFIER.STRING_2);
        //    log.AddLogToInvolvedObjects();
        //    PlayerManager.Instance.player.ShowNotification(log);
        //}

        //private SERIAL_VICTIM_TYPE RandomizeVictimType(bool isPrefix) {
        //    int chance = UnityEngine.Random.Range(0, 2);
        //    if (isPrefix) {
        //        if (chance == 0) {
        //            return SERIAL_VICTIM_TYPE.GENDER;
        //        }
        //        return SERIAL_VICTIM_TYPE.ROLE;
        //    } else {
        //        //if (chance == 0) {
        //        //    return SERIAL_VICTIM_TYPE.TRAIT;
        //        //}
        //        return SERIAL_VICTIM_TYPE.STATUS;
        //    }
        //}

        private bool DoesCharacterFitAnyVictimRequirements(Character target) {
            return victim1Requirement.DoesCharacterFitVictimRequirements(target); //|| victim2Requirement.DoesCharacterFitVictimRequirements(target)
        }

        public void SerialKillerSawButWillNotAssist(Character targetCharacter, Trait negativeTrait) {
            Log log = new Log(GameManager.Instance.Today(), "Character", "NonIntel", "serial_killer_saw_no_assist");
            log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
            log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.TARGET_CHARACTER);
            log.AddToFillers(null, negativeTrait.name, LOG_IDENTIFIER.STRING_1);
            character.logComponent.RegisterLogAndShowNotifToThisCharacterOnly(log, onlyClickedCharacter: false);
        }

        #region Opinion
        private void CopyOpinionAndSetAllOpinionToZero() {
            foreach (KeyValuePair<Character, OpinionData> kvp in character.opinionComponent.opinions) {
                OpinionData data = ObjectPoolManager.Instance.CreateNewOpinionData();
                data.SetCompatibilityValue(kvp.Value.compatibilityValue);
                List<string> keys = kvp.Value.allOpinions.Keys.ToList();
                for (int i = 0; i < keys.Count; i++) {
                    string key = keys[i];
                    data.SetOpinion(key, kvp.Value.allOpinions[key]);
                    kvp.Value.allOpinions[key] = 0;
                }
                opinionCopy.Add(kvp.Key, data);
            }
        }
        private void BringBackOpinion() {
            //if(opinionCopy.Count > 0) {
            //    List<Character> keys = opinionCopy.Keys.ToList();
            //    for (int i = 0; i < keys.Count; i++) {
            //        Character currChar = keys[i];
                    
            //    }
            //}
            foreach (KeyValuePair<Character, OpinionData> kvp in opinionCopy) {
                if (character.opinionComponent.HasOpinion(kvp.Key)) {
                    foreach (KeyValuePair<string, int> dataKvp in kvp.Value.allOpinions) {
                        if(dataKvp.Key == "Base") { continue; }
                        if (character.opinionComponent.HasOpinion(kvp.Key, dataKvp.Key)) {
                            character.opinionComponent.SetOpinion(kvp.Key, dataKvp.Key, dataKvp.Value);
                        }
                    }
                }
                ObjectPoolManager.Instance.ReturnOpinionDataToPool(kvp.Value);
            }
            opinionCopy.Clear();
        }
        public void AdjustOpinion(Character target, string opinionText, int opinionValue) {
            if (opinionText == "Base") {
                //Do not copy Base opinion
                return;
            }
            if (!opinionCopy.ContainsKey(target)) {
                opinionCopy.Add(target, ObjectPoolManager.Instance.CreateNewOpinionData());
            }
            opinionCopy[target].AdjustOpinion(opinionText, opinionValue);
        }
        #endregion
    }

    [System.Serializable]
    public class SerialVictim {
        public SERIAL_VICTIM_TYPE victimFirstType;
        public SERIAL_VICTIM_TYPE victimSecondType;
        public List<string> victimFirstDescription;
        public List<string> victimSecondDescription;

        public string text { get; private set; }

        //public SerialVictim(SERIAL_VICTIM_TYPE victimFirstType, SERIAL_VICTIM_TYPE victimSecondType) {
        //    this.victimFirstType = victimFirstType;
        //    this.victimSecondType = victimSecondType;
        //    GenerateVictim();
        //}
        public SerialVictim(SERIAL_VICTIM_TYPE victimFirstType, List<string> victimFirstDesc, SERIAL_VICTIM_TYPE victimSecondType, List<string> victimSecondDesc) {
            this.victimFirstType = victimFirstType;
            this.victimSecondType = victimSecondType;
            victimFirstDescription = victimFirstDesc;
            victimSecondDescription = victimSecondDesc;
            GenerateText();
        }

        //private void GenerateVictim() {
        //    victimFirstDescription = GenerateVictimDescription(victimFirstType);
        //    victimSecondDescription = GenerateVictimDescription(victimSecondType);
        //    GenerateText();
        //}
        //public string GenerateVictimDescription(SERIAL_VICTIM_TYPE victimType) {
        //    if (victimType == SERIAL_VICTIM_TYPE.GENDER) {
        //        GENDER gender = Utilities.GetRandomEnumValue<GENDER>();
        //        return gender.ToString();
        //    } else if (victimType == SERIAL_VICTIM_TYPE.ROLE) {
        //        //CHARACTER_ROLE[] roles = new CHARACTER_ROLE[] { CHARACTER_ROLE.CIVILIAN, CHARACTER_ROLE.SOLDIER, CHARACTER_ROLE.ADVENTURER };
        //        string[] roles = new string[] { "Worker", "Combatant", "Royalty" };
        //        return roles[UnityEngine.Random.Range(0, roles.Length)];
        //    } 
        //    else if (victimType == SERIAL_VICTIM_TYPE.TRAIT) {
        //        string[] traits = new string[] { "Builder", "Criminal", "Drunk", "Sick", "Lazy", "Hardworking" }; //, "Curious"
        //        return traits[UnityEngine.Random.Range(0, traits.Length)];
        //    }
        //    else if (victimType == SERIAL_VICTIM_TYPE.STATUS) {
        //        string[] statuses = new string[] { "Hungry", "Tired", "Lonely" };
        //        return statuses[UnityEngine.Random.Range(0, statuses.Length)];
        //    }
        //    return string.Empty;
        //}
        private void GenerateText() {
            string firstText = string.Empty;
            string secondText = string.Empty;
            bool isFirstTypeProcessed = true;
            //if (victimSecondDescription != "Builder" && victimSecondDescription != "Criminal") {
            //    firstText = victimSecondDescription;
            //    secondText = PluralizeText(Utilities.NormalizeStringUpperCaseFirstLetters(victimFirstDescription));
            //} else {
            //    firstText = Utilities.NormalizeStringUpperCaseFirstLetters(victimFirstDescription);
            //    secondText = PluralizeText(victimSecondDescription);
            //}

            //If there is a Gender, it is always the first text
            if(victimFirstType == SERIAL_VICTIM_TYPE.GENDER) {
                isFirstTypeProcessed = true;
                firstText = Utilities.NormalizeStringUpperCaseFirstLetterOnly(victimFirstDescription[0]);
            } else if (victimSecondType == SERIAL_VICTIM_TYPE.GENDER) {
                isFirstTypeProcessed = false;
                firstText = Utilities.NormalizeStringUpperCaseFirstLetterOnly(victimSecondDescription[0]);
            }
            if(firstText == string.Empty) {
                //If there is no Gender, the first text must be Race
                if (victimFirstType == SERIAL_VICTIM_TYPE.RACE) {
                    isFirstTypeProcessed = true;
                    firstText = Utilities.NormalizeStringUpperCaseFirstLetterOnly(victimFirstDescription[0]);
                } else if (victimSecondType == SERIAL_VICTIM_TYPE.RACE) {
                    isFirstTypeProcessed = false;
                    firstText = Utilities.NormalizeStringUpperCaseFirstLetterOnly(victimSecondDescription[0]);
                }
                if (firstText == string.Empty) {
                    //If there is no Race or Gender victim type, generate description normally
                    firstText = GetDescriptionText(false);
                    secondText = GetDescriptionText(true);
                    if (firstText != string.Empty && secondText != string.Empty) {
                        secondText = secondText.Insert(0, "and ");
                    }
                } else {
                    secondText = GetDescriptionText(isFirstTypeProcessed);
                }

            } else {
                secondText = GetDescriptionText(isFirstTypeProcessed);
            }
            this.text = firstText + " " + secondText;
        }
        private string GetDescriptionText(bool fromSecondType) {
            List<string> secondDescriptions = victimSecondDescription;
            if (!fromSecondType) {
                secondDescriptions = victimFirstDescription;
            }
            string newDesc = string.Empty;
            if(secondDescriptions != null) {
                for (int i = 0; i < secondDescriptions.Count; i++) {
                    if (i > 0) {
                        newDesc += ", ";
                    }
                    newDesc += Utilities.PluralizeString(Utilities.NormalizeStringUpperCaseFirstLetters(secondDescriptions[i]));
                }
            }
            return newDesc;
        }
        //private string PluralizeText(string text) {
        //    string newText = text + "s";
        //    if (text.EndsWith("man")) {
        //        newText = text.Replace("man", "men");
        //    }else if (text.EndsWith("ty")) {
        //        newText = text.Replace("ty", "ties");
        //    }
        //    return newText;
        //}

        public bool DoesCharacterFitVictimRequirements(Character character) {
            return DoesCharacterFitVictimTypeDescription(victimFirstType, victimFirstDescription, character)
                && DoesCharacterFitVictimTypeDescription(victimSecondType, victimSecondDescription, character);
        }
        private bool DoesCharacterFitVictimTypeDescription(SERIAL_VICTIM_TYPE victimType, List<string> victimDesc, Character character) {
            if(victimType == SERIAL_VICTIM_TYPE.NONE) {
                return true;
            }
            string comparer = string.Empty;
            if (victimType == SERIAL_VICTIM_TYPE.GENDER) {
                comparer = character.gender.ToString();
                //return victimDesc == character.gender.ToString();
            } else if (victimType == SERIAL_VICTIM_TYPE.RACE) {
                comparer = character.race.ToString();
                //return character.traitContainer.GetNormalTrait<Trait>(victimDesc) != null;
            } else if (victimType == SERIAL_VICTIM_TYPE.CLASS) {
                comparer = character.characterClass.className;
                //return character.traitContainer.GetNormalTrait<Trait>(victimDesc) != null;
            } else if (victimType == SERIAL_VICTIM_TYPE.TRAIT) {
                for (int i = 0; i < victimDesc.Count; i++) {
                    if(character.traitContainer.GetNormalTrait<Trait>(victimDesc[i]) != null) {
                        return true;
                    }
                }
                //return character.traitContainer.GetNormalTrait<Trait>(victimDesc) != null;
            }
            if(comparer != string.Empty) {
                for (int i = 0; i < victimDesc.Count; i++) {
                    if (victimDesc[i] == comparer) {
                        return true;
                    }
                }
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

            //isFollowing = derivedTrait.isFollowing;
            //hasStartedFollowing = derivedTrait.hasStartedFollowing;

            if (derivedTrait.targetVictim != null) {
                targetVictimID = derivedTrait.targetVictim.id;
            } else {
                targetVictimID = -1;
            }
        }

        public override Trait Load(ref Character responsibleCharacter) {
            Trait trait = base.Load(ref responsibleCharacter);
            SerialKiller derivedTrait = trait as SerialKiller;
            derivedTrait.SetVictimRequirements(victim1Requirement);
            //derivedTrait.SetVictim2Requirement(victim2Requirement);

            //derivedTrait.SetIsFollowing(isFollowing);
            //derivedTrait.SetHasStartedFollowing(hasStartedFollowing);

            if (targetVictimID != -1) {
                derivedTrait.SetTargetVictim(CharacterManager.Instance.GetCharacterByID(targetVictimID));
            }
            return trait;
        }
    }
}
