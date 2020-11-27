﻿using System;
using JetBrains.Annotations;
namespace Quests.Steps {
    public class GoapJobFailed : QuestStep {
        
        private readonly Character _target;
        private readonly GoapPlanJob _job;
        
        public GoapJobFailed(string stepDescription, Character target, [NotNull]GoapPlanJob job) : base(stepDescription) {
            _target = target;
            _job = job;
        }
        
        protected override void SubscribeListeners() {
            Messenger.AddListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<JobQueueItem, Character>(JobSignals.JOB_REMOVED_FROM_QUEUE, OnJobRemovedFromCharacter);
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_DEATH, OnCharacterDied);
        }

        #region Completion
        private void OnJobRemovedFromCharacter(JobQueueItem job, Character character) {
            if (_target == character && job == _job && _job.finishedSuccessfully == false) {
                Complete();
            }
        }
        private void OnCharacterDied(Character character) {
            if (_target == character) {
                Complete();
            }
        }
        #endregion
    }
}