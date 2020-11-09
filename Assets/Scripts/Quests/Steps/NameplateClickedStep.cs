﻿namespace Quests.Steps {
    public class NameplateClickedStep : QuestStep {
        private string _neededText;

        public NameplateClickedStep(string neededText, string stepDescription = "Nameplate clicked")
            : base(stepDescription) {
            _neededText = neededText;
        }
        protected override void SubscribeListeners() {
            Messenger.AddListener<string>(UISignals.NAMEPLATE_CLICKED, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<string>(UISignals.NAMEPLATE_CLICKED, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(string text) {
            if (text == _neededText) {
                Complete();
            }
        }
        #endregion
    }
}