﻿using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Tutorial;
using UnityEngine;
namespace Quests {
    public class QuestManager : MonoBehaviour {

        public static QuestManager Instance;

        private List<Quest> _activeQuests;
        
        private void Awake() {
            Instance = this;
        }

        #region Initialization
        public void InitializeAfterGameLoaded() {
            _activeQuests = new List<Quest>();
            CheckEliminateAllVillagersQuest();
            Messenger.AddListener<List<Character>, DemonicStructure>(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, 
                OnCharactersAttackingDemonicStructure);
            Messenger.AddListener<LocationStructure, Character, GoapPlanJob>(Signals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            Messenger.AddListener<List<Character>>(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, 
                OnAngelsAttackingDemonicStructure);
        }
        #endregion
        
        #region Activation
        private void ActivateQuest(Quest quest) {
            _activeQuests.Add(quest);
            quest.Activate();
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(quest, true);
            quest.SetTutorialQuestItem(questItem);
        }
        private void ActivateQuest<T>(params object[] arguments) where T : Quest {
            Quest quest = System.Activator.CreateInstance(typeof(T), arguments) as Quest;
            _activeQuests.Add(quest);
            quest.Activate();
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(quest, true);
            quest.SetTutorialQuestItem(questItem);
        }
        private void DeactivateQuest(Quest quest) {
            _activeQuests.Remove(quest);
            if (quest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(quest);
            }
            quest.Deactivate();
        }
        #endregion
        
        #region Completion
        public void CompleteQuest(Quest quest) {
            DeactivateQuest(quest);
        }
        #endregion

        #region Eliminate All Villagers Quest
        private void CheckEliminateAllVillagersQuest() {
            if (SaveManager.Instance.currentSaveDataPlayer.completedTutorials
                .Contains(TutorialManager.Tutorial.Build_A_Kennel)) {
                CreateEliminateAllVillagersQuest();
            } else {
                Messenger.AddListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
            }
        }
        private void OnTutorialQuestCompleted(TutorialQuest completedQuest) {
            if (completedQuest.tutorialType == TutorialManager.Tutorial.Build_A_Kennel) {
                Messenger.RemoveListener<TutorialQuest>(Signals.TUTORIAL_QUEST_COMPLETED, OnTutorialQuestCompleted);
                CreateEliminateAllVillagersQuest();
            }
        }
        private void CreateEliminateAllVillagersQuest() {
            EliminateAllVillagers eliminateAllVillagers = new EliminateAllVillagers();
            ActivateQuest(eliminateAllVillagers);
        }
        #endregion

        #region Counterattack
        private void OnCharactersAttackingDemonicStructure(List<Character> attackers, DemonicStructure targetStructure) {
            ActivateQuest<Counterattack>(attackers, targetStructure);
        }
        #endregion

        #region Report Demonic Structure
        private void OnDemonicStructureDiscovered(LocationStructure structure, Character reporter, GoapPlanJob job) {
            ActivateQuest<DemonicStructureDiscovered>(structure, reporter, job);
        }
        #endregion

        #region Divine Intervention
        private void OnAngelsAttackingDemonicStructure(List<Character> angels) {
            ActivateQuest<DivineIntervention>(angels);
        }
        #endregion
    }
}