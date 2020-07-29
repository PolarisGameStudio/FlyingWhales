﻿using System.Collections.Generic;
using System.Linq;
using Quests;
using Quests.Steps;
using UnityEngine.Assertions;

namespace Tutorial {
    public abstract class TutorialQuest : SteppedQuest {
        public TutorialManager.Tutorial tutorialType { get; }
        protected TutorialQuest(string _questName, TutorialManager.Tutorial _tutorialType) : base(_questName) {
            tutorialType = _tutorialType;
            Initialize();
        }

        #region Initialization
        private void Initialize() {
            ConstructCriteria();
            StartCheckingCriteria();
        }
        #endregion

        #region Criteria
        /// <summary>
        /// Construct the list of criteria that this quest needs to be activated.
        /// </summary>
        protected abstract void ConstructCriteria();
        /// <summary>
        /// Make this quest start checking for it's criteria
        /// </summary>
        private void StartCheckingCriteria() {
            Messenger.AddListener<QuestCriteria>(Signals.QUEST_CRITERIA_MET, OnCriteriaMet);
            Messenger.AddListener<QuestCriteria>(Signals.QUEST_CRITERIA_UNMET, OnCriteriaUnMet);
            for (int i = 0; i < _activationCriteria.Count; i++) {
                QuestCriteria criteria = _activationCriteria[i];
                criteria.Enable();
            }
        }
        protected void StopCheckingCriteria() {
            Messenger.RemoveListener<QuestCriteria>(Signals.QUEST_CRITERIA_MET, OnCriteriaMet);
            Messenger.RemoveListener<QuestCriteria>(Signals.QUEST_CRITERIA_UNMET, OnCriteriaUnMet);
            for (int i = 0; i < _activationCriteria.Count; i++) {
                QuestCriteria criteria = _activationCriteria[i];
                criteria.Disable();
            }
        }
        private void OnCriteriaMet(QuestCriteria criteria) {
            if (isAvailable) { return; } //do not check criteria completion if tutorial has already been made available
            if (_activationCriteria.Contains(criteria)) {
                TryMakeAvailable();
            }
        }
        private void OnCriteriaUnMet(QuestCriteria criteria) {
            if (_activationCriteria.Contains(criteria)) {
                if (isAvailable) {
                    MakeUnavailable();
                }
            }
        }
        /// <summary>
        /// Try and make this quest available, this will check if all criteria has been met. If it has
        /// then make it available.
        /// </summary>
        protected void TryMakeAvailable() {
            //check if all criteria has been met
            if (HasMetAllCriteria()) {
                MakeAvailable();
            }
        }
        protected virtual bool HasMetAllCriteria() {
            bool hasMetAllCriteria = true;
            for (int i = 0; i < _activationCriteria.Count; i++) {
                QuestCriteria c = _activationCriteria[i];
                if (c.hasCriteriaBeenMet == false) {
                    hasMetAllCriteria = false;
                    break;
                }
            }
            return hasMetAllCriteria;
        }
        #endregion
        
        // #region Availability
        // protected override void MakeAvailable() {
        //     base.MakeAvailable();
        //     TutorialManager.Instance.AddTutorialToWaitList(this);
        // }
        // protected override void MakeUnavailable() {
        //     base.MakeUnavailable();
        //     TutorialManager.Instance.RemoveTutorialFromWaitList(this);
        // }
        // #endregion

        #region Completion
        protected override void CompleteQuest() {
            TutorialManager.Instance.CompleteTutorialQuest(this);
        }
        #endregion
        
        #region Activation
        public override void Activate() {
            StopCheckingCriteria();
            base.Activate();
        }
        public override void Deactivate() {
            base.Deactivate();
            if (isAvailable == false) {
                //only stop checking criteria only if tutorial has not yet been made available but has been deactivated.  
                StopCheckingCriteria();    
            }
        }
        #endregion

        #region Failure
        protected override void FailQuest() {
            base.FailQuest();
            TutorialManager.Instance.FailTutorialQuest(this);
        }
        #endregion
        
    }
}