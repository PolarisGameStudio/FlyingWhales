﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Traits {
    [System.Serializable]
    public class Trait : IMoodModifier{
        //public virtual string nameInUI {
        //    get { return name; }
        //}
        public virtual bool isNotSavable {
            get { return false; }
        }

        //Non-changeable values
        public string name;
        public string description;
        public string thoughtText;
        public TRAIT_TYPE type;
        public TRAIT_EFFECT effect;
        public List<INTERACTION_TYPE> advertisedInteractions;
        public int ticksDuration; //Zero (0) means Permanent
        public int level;
        public int moodEffect;
        public List<TraitEffect> effects;
        public bool isHidden;
        public string[] mutuallyExclusive; //list of traits that this trait cannot be with.
        public bool canBeTriggered;
        public bool hindersWitness; //if a character has this trait, and this is true, then he/she cannot witness events
        public bool hindersMovement; //if a character has this trait, and this is true, then he/she cannot move
        public bool hindersAttackTarget; //if a character has this trait, and this is true, then he/she cannot be attacked
        public bool hindersPerform; //if a character has this trait, and this is true, then he/she cannot be attacked
        public bool isStacking;
        public int stackLimit;
        public float stackModifier;
        //public bool isNonRemovable; //determines if trait can be removed through natural process (ie. RemoveTrait, etc.), if this is set to true, it means that it can only be removed by certain functions

        public Character responsibleCharacter { get { return responsibleCharacters != null ? responsibleCharacters.FirstOrDefault() : null; } }
        public List<Character> responsibleCharacters { get; protected set; }
        //public Dictionary<ITraitable, string> expiryTickets { get; private set; } //this is the key for the scheduled removal of this trait for each object
        public ActualGoapNode gainedFromDoing { get; private set; } //what action was this poi involved in that gave it this trait.
        public GameDate dateEstablished { get; protected set; }
        //public virtual bool isRemovedOnSwitchAlterEgo { get { return false; } }
        public string moodModificationDescription => name;
        public int moodModifier => moodEffect;

        public virtual bool isPersistent { get { return false; } } //should this trait persist through all a character's alter egos

        #region Virtuals
        public virtual void OnAddTrait(ITraitable addedTo) {
            //if(type == TRAIT_TYPE.CRIMINAL && sourceCharacter is Character) {
            //    Character character = sourceCharacter as Character;
            //    character.CreateApprehendJob();
            //}
            if(addedTo is Character) {
                Character character = addedTo as Character;
                character.moodComponent.AddMoodEffect(moodEffect, this);
                if (string.IsNullOrEmpty(thoughtText) == false) {
                    character.AddOverrideThought(thoughtText);
                }
            }
            if (level == 0) {
                SetLevel(1);
            }
            SetDateEstablished(GameManager.Instance.Today());
        }
        public virtual void OnRemoveTrait(ITraitable removedFrom, Character removedBy) {
            if (removedFrom is Character) {
                Character character = removedFrom as Character;
                character.moodComponent.RemoveMoodEffect(-moodEffect, this);    
                if (name == "Criminal") {
                    if (!character.traitContainer.HasTrait("Criminal")) {
                        character.ForceCancelAllJobsTargettingThisCharacter(JOB_TYPE.APPREHEND);
                    }
                }
                if (string.IsNullOrEmpty(thoughtText) == false) {
                    character.RemoveOverrideThought(thoughtText);
                }
            }
        }
        public virtual void OnStackTrait(ITraitable addedTo) {
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.moodComponent.AddMoodEffect(Mathf.RoundToInt(moodEffect * stackModifier), this);
            }
            SetDateEstablished(GameManager.Instance.Today());
        }
        public virtual void OnUnstackTrait(ITraitable addedTo) {
            if (addedTo is Character) {
                Character character = addedTo as Character;
                character.moodComponent.RemoveMoodEffect(-Mathf.RoundToInt(moodEffect * stackModifier), this);
            }
        }
        public virtual string GetToolTipText() { return string.Empty; }
        public virtual bool IsUnique() { return true; }
        /// <summary>
        /// Only used for characters, since traits aren't removed when a character dies.
        /// This function will be called to ensure that any unneeded resources in traits can be freed up when a character dies.
        /// <see cref="Character.Death(string)"/>
        /// </summary>
        public virtual bool OnDeath(Character character) { return false; }
        //public virtual bool OnAfterDeath(Character character, string cause = "normal", ActualGoapNode deathFromAction = null, Character responsibleCharacter = null, Log _deathLog = null, LogFiller[] deathLogFillers = null) { return false; }
        /// <summary>
        /// Used to return necessary actions when a character with this trait
        /// returns to life.
        /// </summary>
        /// <param name="character">The character that returned to life.</param>
        public virtual void OnReturnToLife(Character character) { }
        public virtual string GetTestingData() { return string.Empty; }
        public virtual bool CreateJobsOnEnterVisionBasedOnTrait(IPointOfInterest traitOwner, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the target's traits?
        public virtual bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob) { return false; } //What jobs a character can create based on the his/her own traits, considering the target?
        public virtual void OnSeePOIEvenCannotWitness(IPointOfInterest targetPOI, Character character) { }
        protected virtual void OnChangeLevel() { }
        public virtual void OnOwnerInitiallyPlaced(Character owner) { }
        public virtual bool IsTangible() { return false; } //is this trait tangible? Only used for traits on tiles, so that the tile's tile object will be activated when it has a tangible trait
        public virtual bool PerTickOwnerMovement() { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
        public virtual bool OnStartPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction) { return false; } //returns true or false if it created a job/action, once a job/action is created must not check others anymore to avoid conflicts
        //Returns the string of the log key that's supposed to be logged
        public virtual string TriggerFlaw(Character character) {
            int manaCost = EditableValuesManager.Instance.triggerFlawManaCost;
            PlayerManager.Instance.player.AdjustMana(-manaCost);
            if (character.trapStructure.structure != null) {
                //clear all trap structures when triggering flaw
                character.trapStructure.SetStructureAndDuration(null, 0);
            }
            return "flaw_effect";
        }
        /// <summary>
        /// This checks if this flaw can be triggered. This checks both the requirements of the individual traits,
        /// and the mana cost. This is responsible for enabling/disabling the trigger flaw buttton.
        /// </summary>
        /// <param name="character">The character whose flaw will be triggered</param>
        /// <returns>true or false</returns>
        public virtual bool CanFlawBeTriggered(Character character) {
            //return true;
            int manaCost = EditableValuesManager.Instance.triggerFlawManaCost;

            return PlayerManager.Instance.player.mana >= manaCost
                && character.canPerform
                //&& !character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER) //disabled characters cannot be triggered
                && !character.traitContainer.HasTrait("Blessed")
                && !character.currentParty.icon.isTravellingOutside; //characters travelling outside cannot be triggered
        }
        public virtual string GetRequirementDescription(Character character) {
            return "Mana cost of triggering this flaw's negative effect depends on the character's mood. The darker the mood, the cheaper the cost.";
        }
        public virtual List<string> GetCannotTriggerFlawReasons(Character character) {
            List<string> reasons = new List<string>();
            if (PlayerManager.Instance.player.mana < EditableValuesManager.Instance.triggerFlawManaCost) {
                reasons.Add("You do not have enough mana.");
            }
            if (character.traitContainer.HasTrait("Blessed")) {
                reasons.Add("Blessed characters cannot be targeted by Trigger Flaw.");
            }
            if (!character.canPerform) {
                reasons.Add("Characters that cannot perform cannot be targeted by Trigger Flaw.");
            }
            //if (character.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER)) {
            //    reasons.Add("Inactive characters cannot be targeted by Trigger Flaw.");
            //}
            return reasons;

        }
        public virtual void OnTickStarted() {

        }
        public virtual void OnTickEnded() { }
        public virtual void OnHourStarted() { }
        #endregion

        #region Utilities
        public string GetTriggerFlawEffectDescription(Character character, string key) {
            if (LocalizationManager.Instance.HasLocalizedValue("Trait", name, key)) {
                Log log = new Log(GameManager.Instance.Today(), "Trait", name, key);
                log.AddToFillers(character, character.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
                return Ruinarch.Utilities.LogReplacer(log);
            }
            return string.Empty;
        }
        public void SetGainedFromDoing(ActualGoapNode action) {
            gainedFromDoing = action;
        }
        public void OverrideDuration(int newDuration) {
            ticksDuration = newDuration;
        }
        public void AddCharacterResponsibleForTrait(Character character) {
            if (character != null) {
                if (responsibleCharacters == null) {
                    responsibleCharacters = new List<Character>();
                }
                if (!responsibleCharacters.Contains(character)) {
                    responsibleCharacters.Add(character);
                }
            }
        }
        public bool IsResponsibleForTrait(Character character) {
            if (responsibleCharacter == character) {
                return true;
            } else if (responsibleCharacters != null) {
                return responsibleCharacters.Contains(character);
            }
            return false;
        }
        //public void SetExpiryTicket(ITraitable obj, string expiryTicket) {
        //    if (expiryTickets == null) {
        //        expiryTickets = new Dictionary<ITraitable, string>();
        //    }
        //    if (!expiryTickets.ContainsKey(obj)) {
        //        expiryTickets.Add(obj, expiryTicket);
        //    } else {
        //        expiryTickets[obj] = expiryTicket;
        //    }
        //}
        //public void RemoveExpiryTicket(ITraitable traitable) {
        //    if (expiryTickets != null) {
        //        expiryTickets.Remove(traitable);
        //    }
        //}
        public void LevelUp() {
            level++;
            level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
            OnChangeLevel();
        }
        public void SetLevel(int amount) {
            level = amount;
            level = Mathf.Clamp(level, 1, PlayerManager.MAX_LEVEL_INTERVENTION_ABILITY);
            OnChangeLevel();
        }
        public void SetDateEstablished(GameDate date) {
            dateEstablished = date;
        }
        public void SetTraitEffects(List<TraitEffect> effects) {
            this.effects = effects;
        }
        protected bool TryTransferJob(JobQueueItem currentJob, Character characterThatWillDoJob) {
            if (currentJob.originalOwner.ownerType == JOB_OWNER.LOCATION || currentJob.originalOwner.ownerType == JOB_OWNER.QUEST) {
                bool canBeTransfered = false;
                Character assignedCharacter = currentJob.assignedCharacter;
                if (assignedCharacter != null && assignedCharacter.currentActionNode.action != null
                    && assignedCharacter.currentJob != null && assignedCharacter.currentJob == currentJob) {
                    if (assignedCharacter != characterThatWillDoJob) {
                        canBeTransfered = !assignedCharacter.marker.inVisionPOIs.Contains(assignedCharacter.currentActionNode.poiTarget);
                    }
                } else {
                    canBeTransfered = true;
                }
                if (canBeTransfered && characterThatWillDoJob.CanCurrentJobBeOverriddenByJob(currentJob)) {
                    currentJob.CancelJob(shouldDoAfterEffect: false);
                    characterThatWillDoJob.jobQueue.AddJobInQueue(currentJob);
                    //TODO: characterThatWillDoJob.jobQueue.AssignCharacterToJobAndCancelCurrentAction(currentJob, characterThatWillDoJob);
                    return true;
                }
            }
            return false;
        }
        public virtual string GetNameInUI(ITraitable traitable) {
            Dictionary<string, int> stacks = traitable.traitContainer.stacks;
            if (isStacking && stacks.ContainsKey(name) && stacks[name] > 1) {
                int num = stacks[name];
                if(num > stackLimit) { num = stackLimit; }
                return name + " (x" + num + ")";
            }
            return name;
        }
        #endregion

        #region Actions
        /// <summary>
        /// If this trait modifies any costs of an action, put it here.
        /// </summary>
        /// <param name="action">The type of action.</param>
        /// <param name="cost">The cost to be modified.</param>
        public virtual void ExecuteCostModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref int cost) { }
        public virtual void ExecuteExpectedEffectModification(INTERACTION_TYPE action, Character actor, IPointOfInterest poiTarget, object[] otherData, ref List<GoapEffect> effects) { }
        public virtual void ExecuteActionPreEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) { }
        public virtual void ExecuteActionPerTickEffects(INTERACTION_TYPE action, ActualGoapNode goapNode) { }
        public virtual void ExecuteActionAfterEffects(INTERACTION_TYPE action, ActualGoapNode goapNode, ref bool isRemoved) { }
        public virtual bool TryStopAction(INTERACTION_TYPE action, Character actor, IPointOfInterest target, ref GoapActionInvalidity goapActionInvalidity) {
            return false;
        }
        #endregion
    }
}


[System.Serializable]
public class TraitEffect {
    public STAT stat;
    public float amount;
    public bool isPercentage;
    public TRAIT_REQUIREMENT_CHECKER checker;
    public TRAIT_REQUIREMENT_TARGET target;
    public DAMAGE_IDENTIFIER damageIdentifier; //Only used during combat
    public string description;

    public bool hasRequirement;
    public bool isNot;
    public TRAIT_REQUIREMENT requirementType;
    public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;
    public List<string> requirements;
}