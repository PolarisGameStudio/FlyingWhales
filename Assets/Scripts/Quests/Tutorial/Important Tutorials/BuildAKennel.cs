﻿using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests;
using Quests.Steps;
using UnityEngine;
namespace Tutorial {
    public class BuildAKennel : ImportantTutorial {

        public BuildAKennel() : base("Kennel", TutorialManager.Tutorial.Build_A_Kennel) { }

        #region Criteria
        protected override void ConstructCriteria() {
            _activationCriteria = new List<QuestCriteria>() {
                new HasCompletedTutorialQuest(TutorialManager.Tutorial.Elemental_Interactions)
            };
            Messenger.AddListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        protected override bool HasMetAllCriteria() {
            bool hasMetAllCriteria = base.HasMetAllCriteria();
            if (hasMetAllCriteria) {
                return PlayerSkillManager.Instance.GetDemonicStructureSkillData(SPELL_TYPE.KENNEL).charges > 0;
            }
            return false;
        }
        #endregion
      
        protected override void ConstructSteps() {
            steps = new List<QuestStepCollection>() {
                new QuestStepCollection(
                    // new ClickOnEmptyAreaStep()
                    //     .SetHoverOverAction(OnHoverClickEmptyTile)
                    //     .SetHoverOutAction(UIManager.Instance.HideSmallInfo), 
                    // new ObjectPickerShownStep("Click on Build Structure button", "Demonic Structure")
                    //     .SetOnTopmostActions(OnTopMostBuildStructure, OnNoLongerTopMostBuildStructure), 
                    // new StructureBuiltStep(STRUCTURE_TYPE.KENNEL, "Choose the Kennel")
                    //     .SetOnTopmostActions(OnTopMostKennel, OnNoLongerTopMostKennel)
                    new ToggleTurnedOnStep("Build Tab", "Click on the Build tab")
                        .SetOnTopmostActions(OnTopMostBuildTab, OnNoLongerTopMostBuildTab),
                    new ToggleTurnedOnStep("Kennel", "Choose the Kennel")
                        .SetOnTopmostActions(OnTopMostKennel, OnNoLongerTopMostKennel),
                    new StructureBuiltStep(STRUCTURE_TYPE.KENNEL, "Build the Kennel")
                ),
                new QuestStepCollection(
                    new ExecutedPlayerActionStep(SPELL_TYPE.SEIZE_MONSTER, $"Seize a monster.")
                        .SetOnTopmostActions(OnTopMostSeizeMonster, OnNoLongerTopMostSeizeMonster),
                    new DropPOIAtStructureStep((structure, pointOfInterest) => structure.structureType == STRUCTURE_TYPE.KENNEL,
                        poi => poi is Summon, "Drop at the Kennel."),
                    new ClickOnCharacterStep($"Click on the monster", IsCharacterValid),
                    new ExecutedPlayerActionStep(SPELL_TYPE.BREED_MONSTER, "Breed it.")
                        .SetHoverOverAction(OnHoverBreed)
                        .SetHoverOutAction(UIManager.Instance.HideSmallInfo)
                        .SetOnTopmostActions(OnTopMostBreedMonster, OnNoLongerTopMostBreedMonster)
                )
            };
        }
        public override void Activate() {
            base.Activate();
            Messenger.Broadcast(Signals.UPDATE_BUILD_LIST);
            //stop listening for structure building, since another listener will be used to listen for step completion
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }
        public override void Deactivate() {
            base.Deactivate();
            //remove listener, this is for when the tutorial is completed without it being activated 
            Messenger.RemoveListener<LocationStructure>(Signals.STRUCTURE_OBJECT_PLACED, OnAlreadyBuiltStructure);
        }

        #region Availability Listeners
        private void OnAlreadyBuiltStructure(LocationStructure structure) {
            if (structure is Inner_Maps.Location_Structures.Kennel) {
                CompleteQuest(); //player already built a kennel
            }
        }
        #endregion

        #region Step Completion Actions
        private bool IsCharacterValid(Character character) {
            return character is Summon && character.currentStructure is Inner_Maps.Location_Structures.Kennel;
        }
        #endregion

        #region Step Helpers
        private void OnHoverBreed(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo(
                $"Breeding a monster inside the Kennel gives you 1 Summon Charge of that monster type. " +
                $"You can use this charge for various actions - defend Structures, invade Villages, kill Villagers.",
                TutorialManager.Instance.breedVideoClip, "Breeding", stepItem.hoverPosition
            );
        }
        private void OnHoverClickEmptyTile(QuestStepItem stepItem) {
            UIManager.Instance.ShowSmallInfo("Suggestion: choose an empty area far away from the Village", 
                stepItem.hoverPosition, "Choosing where to Build");
        }
        #endregion
        
        #region Build Structure Button
        private void OnTopMostBuildStructure() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Build Structure");
        }
        private void OnNoLongerTopMostBuildStructure() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Build Structure");
        }
        #endregion
        
        #region Choose Kennel
        private void OnTopMostKennel() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Kennel");
        }
        private void OnNoLongerTopMostKennel() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Kennel");
        }
        #endregion
        
        #region Seize Montser
        private void OnTopMostSeizeMonster() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Seize Monster");
        }
        private void OnNoLongerTopMostSeizeMonster() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Seize Monster");
        }
        #endregion
        
        #region Breed Monster
        private void OnTopMostBreedMonster() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Breed Monster");
        }
        private void OnNoLongerTopMostBreedMonster() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Breed Monster");
        }
        #endregion
        
        #region Build Tab
        private void OnTopMostBuildTab() {
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "Build Tab");
        }
        private void OnNoLongerTopMostBuildTab() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "Build Tab");
        }
        #endregion
    }
}