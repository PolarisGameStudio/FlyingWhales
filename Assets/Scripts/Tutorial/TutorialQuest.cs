﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
namespace Tutorial {
    public abstract class TutorialQuest {

        public string questName { get; }
        public TutorialManager.Tutorial tutorialType { get; }
        public List<TutorialQuestStep> steps { get; protected set; }
        public virtual int priority => 0; //priority of this tutorial
        public TutorialQuestItem tutorialQuestItem { get; private set; }
        public bool isAvailable { get; private set; }

        protected TutorialQuest(string _questName, TutorialManager.Tutorial _tutorialType) {
            questName = _questName;
            tutorialType = _tutorialType;
        }

        /// <summary>
        /// Initialize this quest, this usually means subscribing to listeners/waiting for activation criteria to be met.
        /// </summary>
        public abstract void WaitForAvailability();
        protected virtual void StopWaitingForAvailability() { }
        /// <summary>
        /// Make this quest available, this means that this quest is put on the list of available tutorials that the
        /// player can undertake. Usually this is preceded by this quests' criteria being met.  
        /// </summary>
        protected virtual void MakeAvailable() {
            isAvailable = true;
            StopWaitingForAvailability();
            ConstructSteps();
            TutorialManager.Instance.AddTutorialToWaitList(this);
        }
        /// <summary>
        /// Make this tutorial unavailable again. This assumes that this tutorial is currently on wait list.
        /// </summary>
        protected virtual void MakeUnavailable() {
            isAvailable = false;
            Assert.IsTrue(TutorialManager.Instance.IsInWaitList(this), $"{questName} is being made unavailable even though it is not the the current tutorial wait list.");
            TutorialManager.Instance.RemoveTutorialFromWaitList(this);
            WaitForAvailability();
        }
        /// <summary>
        /// Activate this tutorial, meaning this quest should be listening for whether its steps are completed.
        /// </summary>
        public virtual void Activate() {
            Messenger.AddListener<TutorialQuestStep>(Signals.TUTORIAL_STEP_COMPLETED, OnTutorialStepCompleted);
            //activate quest steps
            for (int i = 0; i < steps.Count; i++) {
                TutorialQuestStep step = steps[i];
                step.Activate();
            }
        }
        public virtual void Deactivate() {
            if (isAvailable == false) {
                //only stop waiting for availability only if tutorial has not yet been made available but has been deactivated.  
                StopWaitingForAvailability();    
            }
            if (steps != null) {
                //cleanup steps
                for (int i = 0; i < steps.Count; i++) {
                    TutorialQuestStep step = steps[i];
                    step.Cleanup();
                }    
            }
        }
        /// <summary>
        /// Construct this quests' steps.
        /// </summary>
        public abstract void ConstructSteps();


        #region Listeners
        private void OnTutorialStepCompleted(TutorialQuestStep completedStep) {
            if (steps.Contains(completedStep)) {
                CheckForCompletion();
            }
        }
        #endregion

        #region Completion
        private void CheckForCompletion() {
            if (steps.Any(s => s.isCompleted == false) == false) {
                //check if any steps are not yet completed, if there are none, then this tutorial has been completed
                CompleteTutorial();
            }
        }
        protected void CompleteTutorial() {
            TutorialManager.Instance.CompleteTutorialQuest(this);
        }
        #endregion

        #region UI
        public void SetTutorialQuestItem(TutorialQuestItem tutorialQuestItem) {
            this.tutorialQuestItem = tutorialQuestItem;
        }
        #endregion
    }
}