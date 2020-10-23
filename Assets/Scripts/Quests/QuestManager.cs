﻿using System;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Quests.Special_Popups;
using Settings;
using Tutorial;
using UnityEngine;
using UtilityScripts;
using Debug = System.Diagnostics.Debug;
namespace Quests {
    public class QuestManager : BaseMonoBehaviour {

        public static QuestManager Instance;

        /// <summary>
        /// List of active quests. NOTE: this does not include tutorials.
        /// </summary>
        private List<Quest> _activeQuests;
        
        public enum Special_Popup { 
            Threat, Counterattack, Divine_Intervention, Special_Events, Pause_Reminder, 
            //Finished_Tutorial,
            //Wolf_Migration, Villager_Migration,
            Excalibur_Obtained, Disguised_Succubus, Activated_Ankh, Dragon_Left, Dragon_Awakened, Sleeping_Dragon,
            The_Sword, The_Crack, The_Necronomicon, Cult_Leader
        }

        
        #region getters
        /// <summary>
        /// List of active quests. NOTE: this does not include tutorials.
        /// </summary>
        public List<Quest> activeQuests => _activeQuests;
        #endregion
        
        private void Awake() {
            Instance = this;
            _activeQuests = new List<Quest>();
        }
        protected override void OnDestroy() {
            base.OnDestroy();
            Instance = null;
            Messenger.RemoveListener<List<Character>, DemonicStructure>(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, OnCharactersAttackingDemonicStructure);
            Messenger.RemoveListener<LocationStructure, Character, GoapPlanJob>(Signals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            Messenger.RemoveListener<List<Character>>(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, OnAngelsAttackingDemonicStructure);
            Messenger.RemoveListener<Character, DemonicStructure>(Signals.CHARACTER_ATTACKED_DEMONIC_STRUCTURE, OnSingleCharacterAttackedDemonicStructure);
        }

        #region Initialization
        public void InitializeAfterGameLoaded() {
            Messenger.AddListener<List<Character>, DemonicStructure>(Signals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, OnCharactersAttackingDemonicStructure);
            Messenger.AddListener<LocationStructure, Character, GoapPlanJob>(Signals.DEMONIC_STRUCTURE_DISCOVERED, OnDemonicStructureDiscovered);
            Messenger.AddListener<List<Character>>(Signals.ANGELS_ATTACKING_DEMONIC_STRUCTURE, OnAngelsAttackingDemonicStructure);
            Messenger.AddListener<Character, DemonicStructure>(Signals.CHARACTER_ATTACKED_DEMONIC_STRUCTURE, OnSingleCharacterAttackedDemonicStructure);
            Messenger.Broadcast(Signals.SHOW_SELECTABLE_GLOW, "CenterButton");
        }
        public void InitializeAfterLoadoutPicked(){
            if (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial) {
                CheckEliminateAllVillagersQuest();
                InstantiatePendingSpecialPopups();    
            }
        }
        public void InitializeAfterStartTutorial(){
            CheckEliminateAllVillagersQuest();
            InstantiatePendingSpecialPopups();    
        }
        private void InstantiatePendingSpecialPopups() {
            if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Custom) {
                return; //Tutorials shouldn't show up on Customize Worlds
            }
            List<Special_Popup> completedSpecialPopups = SaveManager.Instance.currentSaveDataPlayer.completedSpecialPopups;
            Special_Popup[] specialPopups = CollectionUtilities.GetEnumValues<Special_Popup>();
            for (int i = 0; i < specialPopups.Length; i++) {
                Special_Popup popup = specialPopups[i];
                //only instantiate popup if it has not yet been completed
                bool instantiateTutorial = completedSpecialPopups.Contains(popup) == false;
                if (instantiateTutorial) {
                    SpecialPopup specialPopup = InstantiateSpecialPopup(popup);
                    specialPopup.Initialize();
                }
            }
        }
        private SpecialPopup InstantiateSpecialPopup(Special_Popup popup) {
            string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities.
                NormalizeStringUpperCaseFirstLettersNoSpace(popup.ToString()));
            string typeName = $"Quests.Special_Popups.{ noSpacesName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                SpecialPopup specialPopup = Activator.CreateInstance(type) as SpecialPopup;
                return specialPopup;
            }
            throw new Exception($"Could not instantiate special popup {noSpacesName}");
        }
        #endregion

        #region Inquiry
        public T GetActiveQuest<T>() where T : Quest {
            for (int i = 0; i < activeQuests.Count; i++) {
                Quest quest = activeQuests[i];
                if (quest is T validQuest) {
                    return validQuest;
                }
            }
            return null;
        }
        public bool IsQuestActive<T>() where T : Quest {
            for (int i = 0; i < activeQuests.Count; i++) {
                Quest quest = activeQuests[i];
                if (quest is T) {
                    return true;
                }
            }
            return false;
        }
        public int GetActiveQuestsCount() {
            return activeQuests.Count;
        }
        #endregion
        
        #region Activation
        public void ActivateQuest(Quest quest) {
            activeQuests.Add(quest);
            quest.Activate();
            if (quest is SteppedQuest steppedQuest) {
                QuestItem questItem = UIManager.Instance.questUI.ShowQuest(steppedQuest, true);
                steppedQuest.SetQuestItem(questItem);
            }
            Messenger.Broadcast(Signals.QUEST_ACTIVATED, quest);
        }
        private void ActivateQuest<T>(params object[] arguments) where T : Quest {
            Quest quest = System.Activator.CreateInstance(typeof(T), arguments) as Quest;
            Debug.Assert(quest != null, nameof(quest) + " != null");
            ActivateQuest(quest);
        }
        private void DeactivateQuest(Quest quest) {
            activeQuests.Remove(quest);
            if (quest is SteppedQuest steppedQuest && steppedQuest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(steppedQuest);
            }
            quest.Deactivate();
        }
        #endregion
        
        #region Completion
        public void CompleteQuest(Quest quest) {
            DeactivateQuest(quest);
            if (quest is SpecialPopup specialPopup) {
                if (specialPopup.isRepeatable) {
                    //spawn popup again.
                    InstantiateSpecialPopup(specialPopup.specialPopupType).Initialize();
                } else {
                    SaveManager.Instance.currentSaveDataPlayer.AddSpecialPopupAsCompleted(specialPopup.specialPopupType);    
                }
            }
        }
        #endregion

        #region Eliminate All Villagers Quest
        private void CheckEliminateAllVillagersQuest() {
            if (WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial || 
                SettingsManager.Instance.settings.skipTutorials) {
                CreateEliminateAllVillagersQuest();
            } else {
                Messenger.AddListener(Signals.FINISHED_IMPORTANT_TUTORIALS, OnImportantTutorialsFinished);
            }
        }
        private void OnImportantTutorialsFinished() {
            CreateEliminateAllVillagersQuest();
        }
        private void CreateEliminateAllVillagersQuest() {
            Messenger.RemoveListener(Signals.FINISHED_IMPORTANT_TUTORIALS, OnImportantTutorialsFinished);
            if (!IsQuestActive<EliminateAllVillagers>()) {
                EliminateAllVillagers eliminateAllVillagers = new EliminateAllVillagers();
                ActivateQuest(eliminateAllVillagers);    
            }
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

        #region Center Button
        public void OnClickCenterButton() {
            Messenger.Broadcast(Signals.HIDE_SELECTABLE_GLOW, "CenterButton");
        }
        #endregion

        #region Under Attack
        private void OnSingleCharacterAttackedDemonicStructure(Character character, DemonicStructure demonicStructure) {
            if (demonicStructure.currentAttackers.Count == 1 && !InnerMapCameraMove.Instance.CanSee(demonicStructure)) {
                PlayerUI.Instance.ShowGeneralConfirmation("Under Attack", $"Your {demonicStructure.name} is under attack!", 
                    onClickCenter: demonicStructure.CenterOnStructure);
            }
        }
        #endregion
    }
}