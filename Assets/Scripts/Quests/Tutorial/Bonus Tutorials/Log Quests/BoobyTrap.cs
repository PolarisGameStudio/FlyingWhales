﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Quests;
using Quests.Steps;
using UnityEngine;
using UnityEngine.Assertions;
namespace Tutorial {
    public class BoobyTrap : LogQuest {

        private ActualGoapNode _targetAction;
        
        public BoobyTrap() : base("Booby Trap", TutorialManager.Tutorial.Booby_Trap) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new CharacterFinishedAction(INTERACTION_TYPE.BOOBY_TRAP).SetOnMeetAction(OnCharacterFinishedAction)
            };
        }
        private void OnCharacterFinishedAction(QuestCriteria criteria) {
            if (criteria is CharacterFinishedAction metCriteria) {
                _targetAction = metCriteria.finishedAction;
                Messenger.AddListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
            }
        }
        #endregion

        #region Availability
        protected override void MakeAvailable() {
            base.MakeAvailable();
            PlayerUI.Instance.ShowGeneralConfirmation("Booby Trap", 
                $"Someone just {UtilityScripts.Utilities.ColorizeAction("placed a Booby Trap")} on an object! A Tutorial Quest has been added to walk you through the details.", 
                onClickOK: () => TutorialManager.Instance.ShowTutorial(this)
            );
        }
        #endregion

        #region Activation
        public override void Activate() {
            base.Activate();
            Messenger.AddListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemoved);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<Log>(UISignals.LOG_REMOVED_FROM_DATABASE, OnLogRemoved);
            Messenger.RemoveListener<TileObject, Character, LocationGridTile>(GridTileSignals.TILE_OBJECT_REMOVED, OnTileObjectRemoved);
        }
        #endregion
        
        #region Steps
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    new ClickOnObjectStep("Find the trapped object", tileObject => tileObject == _targetAction.poiTarget)
                        .SetObjectsToCenter(_targetAction.poiTarget),
                    new ToggleTurnedOnStep("TileObject_Info", "Click its Info tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetAction.poiTarget)
                        .SetHoverOverAction(OnHoverOwner)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ToggleTurnedOnStep("TileObject_Logs", "Click on Log tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetAction.poiTarget),
                    new LogHistoryItemClicked("Click Culprit's name", IsClickedLogObjectValid)
                        .SetHoverOverAction(OnHoverCulprit)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo),
                    new ToggleTurnedOnStep("CharacterInfo_Relations", "Click on Relations tab", () => UIManager.Instance.GetCurrentlySelectedPOI() == _targetAction.actor)
                        .SetCompleteAction(OnCompletePlaceTrap)
                )
            };
        }
        #endregion

        #region Step Helpers
        private bool IsClickedLogObjectValid(object obj, string log, IPointOfInterest owner) {
            if (owner == _targetAction.poiTarget && obj is Character && log.Contains("trapped")) {
                return true;
            }
            return false;
        }
        private void OnHoverOwner(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"An object may have a {UtilityScripts.Utilities.ColorizeAction("personal owner")}. The Culprit may have booby trapped this one to victimize its owner!",
                TutorialManager.Instance.tileObjectOwner, "Object Owner", item.hoverPosition
            );
        }
        private void OnCompletePlaceTrap() {
            TutorialManager.Instance.StartCoroutine(DelayedPlaceTrapTooltip());
        }
        private IEnumerator DelayedPlaceTrapTooltip() {
            yield return new WaitForSecondsRealtime(1.5f);
            PlayerUI.Instance.ShowGeneralConfirmation("Placing Traps",
                $"{UtilityScripts.Utilities.ColorizeAction("Check out the relationship")} between the Culprit and the object's Owner. " +
                "You may also find some hints in the Culprit's Log about their history.\n\n" +
                $"Create more tensions between Villagers to induce more of them to do dastardly deeds like this!"
            );
        }
        private void OnHoverCulprit(QuestStepItem item) {
            UIManager.Instance.ShowSmallInfo(
                $"A {UtilityScripts.Utilities.ColorizeAction("booby-trap log")} should have been registered in the object's Log tab. " +
                $"{UtilityScripts.Utilities.ColorizeAction("Click on the name")} of the Villager that placed it.",
                TutorialManager.Instance.boobyTrapLog, "The culprit", item.hoverPosition
            );
        }
        #endregion

        #region Failure
        private void OnLogRemoved(Log log) {
            if (log.persistentID == _targetAction.descriptionLog.persistentID) {
                //consider this quest as failed if log associated with action was removed from the target object
                TutorialManager.Instance.FailTutorialQuest(this);
            }
        }
        private void OnTileObjectRemoved(TileObject tileObject, Character removedBy, LocationGridTile removedFrom) {
            if (tileObject == _targetAction.poiTarget) {
                //consider this quest as failed if target tile object was removed
                TutorialManager.Instance.FailTutorialQuest(this);
            }
        }
        protected override void FailQuest() {
            base.FailQuest();
            //respawn this Quest.
            TutorialManager.Instance.InstantiateTutorial(TutorialManager.Tutorial.Booby_Trap);
        }
        #endregion
    }
}