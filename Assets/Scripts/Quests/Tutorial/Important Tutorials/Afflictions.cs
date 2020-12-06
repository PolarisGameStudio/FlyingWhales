﻿using System.Collections.Generic;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class Afflictions : ImportantTutorial {
        
        public Afflictions() : base("Afflictions", TutorialManager.Tutorial.Afflictions) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Character_Info),
            };
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerManager.Instance.player.playerSkillComponent.HasAnyAvailableAffliction();
            }
            return false;
        }
        #endregion
        
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Click on a Villager", IsCharacterValid)
                        .SetHoverOverAction(OnHoverSelectCharacterStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ButtonClickedStep("Afflict", "Click on Afflict button")
                        .SetHoverOverAction(OnHoverAfflictButtonStep)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostAfflict, OnNoLongerTopMostAfflict)
                        .SetCompleteAction(OnCompleteExecuteAffliction),
                    new ExecuteAfflictionStep("Apply Vampirism", PLAYER_SKILL_TYPE.VAMPIRISM, OnApplyVampirism)
                        .SetOnTopmostActions(OnTopMostVampirism, OnNoLongerTopMostVampirism)
                ),
                new QuestStepCollection(
                    new ButtonClickedStep("Trigger Flaw", "Click on Trigger Flaw button")
                        .SetOnTopmostActions(OnTopMostTriggerFlawButton, OnNoLongerTopMostTriggerFlawButton)
                        .SetCompleteAction(OnCompleteTriggerFlaw)
                        .SetObjectsToCenter(TriggerFlawTargetCenterGetter),
                    new FlawTriggeredStep("Select Vampirism", "Vampire")
                        .SetOnTopmostActions(OnTopMostTriggerVampiric, OnNoLongerTopMostTriggerVampiric)
                )
                
            };
        }

        #region Step Helpers
        private bool IsCharacterValid(Character character) {
            return character.isNormalCharacter;
        }
        private void OnHoverSelectCharacterStep(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("There are some characters that are <color=\"green\">Blessed</color>. " +
                                             "These characters cannot be directly affected by your spells. " +
                                             "You will need to find other ways to deal with them.",
                TutorialManager.Instance.blessedVideoClip, "Blessed Characters", item.hoverPosition
            );
        }
        private void OnCompleteExecuteAffliction() {
            PlayerUI.Instance.ShowGeneralConfirmation("Afflictions",
                $"Afflictions are {UtilityScripts.Utilities.ColorizeAction("Flaws")} that you may apply to a " +
                $"\nVillager that will affect their behavior. " +
                "Afflictions have a limited number of Charges. " +
                "Once you've exhausted its Charges, there is a long cooldown before it is replenished.\n\n" +
                "There are a vast number of different types of Afflictions you may experiment with. " +
                "You can turn someone into a Psychopath or a Vampire, or you can afflict one with a Zombie Virus."
            );
        }
        private void OnHoverAfflictButtonStep(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo("The afflict button can be seen beside the selected character's nameplate",
                TutorialManager.Instance.afflictButtonVideoClip, "How to Afflict", item.hoverPosition
            );
        }
        private void OnHoverAfflictDetails(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo($"Open the Villager's Info Menu and click on the recently added Affliction.",
                TutorialManager.Instance.afflictionDetailsVideoClip, "Affliction Details", item.hoverPosition
            );
        }
        private void OnCompleteTriggerFlaw() {
            PlayerUI.Instance.ShowGeneralConfirmation("Trigger Flaw",
                $"Trigger Flaw is a special ability that allows you to force a Villager to perform actions " +
                $"related to one of their Flaws. Not all Flaws have associated Trigger Flaw effects but most do."
            );
        }
        #endregion

        #region Trigger Flaw Button
        private void OnTopMostTriggerFlawButton() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Trigger Flaw");
        }
        private void OnNoLongerTopMostTriggerFlawButton() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Trigger Flaw");
        }
        #endregion
        
        #region Affliction Button
        private void OnTopMostAfflict() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Afflict");
        }
        private void OnNoLongerTopMostAfflict() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Afflict");
        }
        #endregion
        
        #region Vampirism Button
        private void OnTopMostVampirism() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Vampirism");
        }
        private void OnNoLongerTopMostVampirism() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Vampirism");
        }
        private Character _afflictedCharacter;
        private void OnApplyVampirism(Character character) {
            _afflictedCharacter = character;
        }
        #endregion
        
        #region Vampiric Trigger Flaw
        private void OnTopMostTriggerVampiric() {
            Messenger.Broadcast(UISignals.SHOW_SELECTABLE_GLOW, "Vampire");
        }
        private void OnNoLongerTopMostTriggerVampiric() {
            Messenger.Broadcast(UISignals.HIDE_SELECTABLE_GLOW, "Vampire");
        }
        private List<ISelectable> TriggerFlawTargetCenterGetter() {
            return new List<ISelectable>() { _afflictedCharacter };
        }
        #endregion
    }
}