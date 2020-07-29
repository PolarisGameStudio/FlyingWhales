﻿using System.Collections.Generic;
using Quests;
using Quests.Steps;
namespace Tutorial {
    public class FactionInfo : ImportantTutorial {
        public FactionInfo() : base("Faction Info", TutorialManager.Tutorial.Faction_Info) { }
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Elemental_Interactions)
            };
        }
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnCharacterStep($"Click on a Villager", validityChecker: character => character.isNormalCharacter),
                    new ToggleTurnedOnStep("CharacterInfo_Info", "Open its Info tab")
                        .SetOnTopmostActions(OnTopMostInfo, OnNoLongerTopMostInfo),
                    new EventLabelLinkClicked("FactionLbl", "Click on its Faction")
                        .SetCompleteAction(OnCompleteExecuteSpell)
                ),
                new QuestStepCollection(
                    new ToggleTurnedOnStep("Faction Overview", "Open its Overview tab")
                        .SetCompleteAction(OnClickOverview)
                        .SetOnTopmostActions(OnTopMostFactionInfo, OnNoLongerTopMostFactionInfo),
                    new ToggleTurnedOnStep("Faction Characters", "Open its Members tab")
                        .SetCompleteAction(OnClickFactionCharacters)
                        .SetOnTopmostActions(OnTopMostCharacters, OnNoLongerTopMostCharacters),
                    new ToggleTurnedOnStep("Faction Owned Locations", "Open its Locations tab")
                        .SetCompleteAction(OnClickLocations)
                        .SetOnTopmostActions(OnTopMostLocations, OnNoLongerTopMostLocations),
                    new ToggleTurnedOnStep("Faction Logs", "Open its Logs tab")
                        .SetCompleteAction(OnClickLogs)
                        .SetOnTopmostActions(OnTopMostLogs, OnNoLongerTopMostLogs)
                )
            };
        }
        
        #region Step Helpers
        private void OnCompleteExecuteSpell() {
            UIManager.Instance.generalConfirmationWithVisual.ShowGeneralConfirmation("Factions",
                "A Faction is a group of characters that belong together. " +
                "It typically has a single Faction Leader, several sets of ideologies, Villager members and claimed territories.", 
                TutorialManager.Instance.spellsVideoClip);
        }
        private void OnClickOverview() {
            PlayerUI.Instance.ShowGeneralConfirmation("Overview Tab",
                $"The Overview tab provides you with basic information about the Faction such as its " +
                $"{UtilityScripts.Utilities.ColorizeAction("Name, Banner, Faction Leader, Ideologies and Relations")}.");
        }
        private void OnClickFactionCharacters() {
            PlayerUI.Instance.ShowGeneralConfirmation("Members Tab",
                "The Members tab shows a list of all characters belonging to the Faction.");
        }
        private void OnClickLocations() {
            PlayerUI.Instance.ShowGeneralConfirmation("Locations Tab",
                "The Locations tab shows a list of all territories belonging to the Faction.");
        }
        private void OnClickLogs() {
            PlayerUI.Instance.ShowGeneralConfirmation("Logs Tab",
                $"The Logs tab provides you with a timestamped list of Faction-related events.");
        }
        #endregion
        
        #region Info Tab
        private void OnTopMostInfo() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CharacterInfo_Info");
        }
        private void OnNoLongerTopMostInfo() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CharacterInfo_Info");
        }
        #endregion
        
        #region Faction Info Tab
        private void OnTopMostFactionInfo() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Overview");
        }
        private void OnNoLongerTopMostFactionInfo() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Overview");
        }
        #endregion
        
        #region Mood Tab
        private void OnTopMostCharacters() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Characters");
        }
        private void OnNoLongerTopMostCharacters() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Characters");
        }
        #endregion
        
        #region Relations Tab
        private void OnTopMostLocations() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Owned Locations");
        }
        private void OnNoLongerTopMostLocations() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Owned Locations");
        }
        #endregion
        
        #region Logs Tab
        private void OnTopMostLogs() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Faction Logs");
        }
        private void OnNoLongerTopMostLogs() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Faction Logs");
        }
        #endregion
    }
}