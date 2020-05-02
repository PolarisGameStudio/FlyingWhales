﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;
using UnityEngine.PlayerLoop;
namespace Tutorial {
    public class RegionalMap : TutorialQuest {
        
        public override int priority => 20;
        private float _notCastingTime;
        
        public RegionalMap() : base("Regional Map", TutorialManager.Tutorial.Regional_Map) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<TutorialQuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Basic_Controls),
                new PlayerHasNotCastedForSeconds(15f),
                new PlayerHasNotCompletedTutorialInSeconds(15f),
                new PlayerIsInInnerMap()
            };
            Messenger.AddListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        #endregion
        
        #region Overrides
        public override void Activate() {
            base.Activate();
            Messenger.RemoveListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        public override void Deactivate() {
            base.Deactivate();
            Messenger.RemoveListener<HexTile>(Signals.TILE_DOUBLE_CLICKED, OnTileDoubleClicked);
        }
        protected override void ConstructSteps() {
            steps = new List<TutorialQuestStepCollection>() {
                new TutorialQuestStepCollection(new HideRegionMapStep("Click on globe icon")),
                new TutorialQuestStepCollection(new SelectRegionStep()),
                new TutorialQuestStepCollection(new DoubleClickHexTileStep())
            };
        }
        #endregion

        #region Listeners
        private void OnTileDoubleClicked(HexTile hexTile) {
            CompleteTutorial();
        }
        #endregion
    }
}