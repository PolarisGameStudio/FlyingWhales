﻿using System;
using System.Collections.Generic;
namespace Tutorial {
    public class TriggerPoisonExplosion : TutorialQuest {
        public TriggerPoisonExplosion() : base("Trigger a Poison Explosion", TutorialManager.Tutorial.Trigger_Poison_Explosion) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new SpellExecuted(new [] {SPELL_TYPE.POISON, SPELL_TYPE.SPLASH_POISON})
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return TutorialManager.Instance.HasActiveTutorial() == false;
            }
            return false;
        }
        #endregion
        
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(
                    new TriggerPoisonExplosionStep("Deal fire damage")
                        .SetHoverOverAction(OnHoverTriggerPoisonExplosionStep)
                        .SetHoverOutAction(OnHoverOutTriggerPoisonExplosionStep)
                )
            };
        }

        #region Step Helpers
        private void OnHoverTriggerPoisonExplosionStep(TutorialQuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("All damage in Ruinarch can have an Elemental Type (Fire, Water, Wind, etc.)." +
                                             "You can trigger a poison explosion by dealing <color=\"red\">Fire Damage</color>" +
                                             " on a <color=\"green\">Poisoned</color> object.", 
                TutorialManager.Instance.fireDamageVideoClip, "Dealing Fire Damage", stepItem.hoverPosition);
        }
        private void OnHoverOutTriggerPoisonExplosionStep() {
            UIManager.Instance.HideSmallInfo();
        }
        #endregion
    }
}