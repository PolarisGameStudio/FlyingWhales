﻿namespace Quests.Steps {
    public class TriggerPoisonExplosionStep : QuestStep {
        public TriggerPoisonExplosionStep(string stepDescription = "Trigger Poison Explosion") : base(stepDescription) { }
        protected override void SubscribeListeners() {
            Messenger.AddListener<IPointOfInterest>(PlayerSignals.POISON_EXPLOSION_TRIGGERED_BY_PLAYER, CheckForCompletion);
        }
        protected override void UnSubscribeListeners() {
            Messenger.RemoveListener<IPointOfInterest>(PlayerSignals.POISON_EXPLOSION_TRIGGERED_BY_PLAYER, CheckForCompletion);
        }

        #region Listeners
        private void CheckForCompletion(IPointOfInterest poi) {
            Complete();
        }
        #endregion
    }
}