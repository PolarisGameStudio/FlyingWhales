﻿namespace Quests.Steps {
    public class StoreIntelStep : QuestStep {
        public StoreIntelStep(string stepDescription = "Store an intel") 
            : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IIntel>(Signals.PLAYER_OBTAINED_INTEL, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IIntel>(Signals.PLAYER_OBTAINED_INTEL, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(IIntel intel) {
            Complete();
        }
        #endregion
    }
}