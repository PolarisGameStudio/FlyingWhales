﻿using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Settings;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Video;
using UtilityScripts;
namespace Tutorial {
    public class TutorialManager : MonoBehaviour {

        public static TutorialManager Instance;
        private const int MaxActiveTutorials = 1;

        public enum Tutorial {
            Basic_Controls = 0,
            Build_A_Kennel = 1,
            Defend_A_Structure = 2,
            Elemental_Interactions = 3,
            Character_Info = 4,
            Spawn_An_Invader = 5,
            Regional_Map = 6,
            Share_An_Intel = 9,
            Afflictions = 10,
            Prison = 11,
            Chaos_Orbs_Tutorial = 15,
            Griefstricken,
            Killed_By_Monster,
            Booby_Trap,
            Rumor,
            Zombie_Virus,
            Frame_Up,
            Faction_Info,
            Create_A_Cultist
        }

        /// <summary>
        /// Tutorial types that are part of the main tutorial.
        /// </summary>
        private readonly Tutorial[] mainTutorialTypes = new[] {
            Tutorial.Basic_Controls,
            Tutorial.Character_Info,
            Tutorial.Afflictions,
            Tutorial.Share_An_Intel,
            Tutorial.Elemental_Interactions,
            Tutorial.Build_A_Kennel,
            Tutorial.Spawn_An_Invader,
        };
        /// <summary>
        /// Tutorial types that are NOT part of the main tutorial.
        /// </summary>
        private readonly Tutorial[] bonusTutorialTypes = new[] {
            Tutorial.Defend_A_Structure,
            Tutorial.Afflictions,
            Tutorial.Prison,
            Tutorial.Chaos_Orbs_Tutorial,
            Tutorial.Griefstricken,
            Tutorial.Killed_By_Monster,
            Tutorial.Booby_Trap,
            Tutorial.Rumor,
            Tutorial.Zombie_Virus,
            Tutorial.Frame_Up,
            Tutorial.Faction_Info,
            Tutorial.Create_A_Cultist
        };

        private List<ImportantTutorial> _activeImportantTutorials;
        private List<ImportantTutorial> _waitingImportantTutorials;
        private List<Tutorial> _completedImportantTutorials;
        private List<BonusTutorial> _activeBonusTutorials;
        private List<TutorialQuest> _instantiatedTutorials;

        //Video Clips
        public VideoClip demonicStructureVideoClip;
        public VideoClip villageVideoClip;
        public VideoClip storeIntelVideoClip;
        public VideoClip shareIntelVideoClip;
        public VideoClip blessedVideoClip;
        public VideoClip timeControlsVideoClip;
        public VideoClip areaVideoClip;
        public VideoClip spellsVideoClip;
        public VideoClip afflictionsVideoClip;
        public VideoClip afflictButtonVideoClip;
        public VideoClip spellsTabVideoClip;
        public Texture buildStructureButton;
        public VideoClip chambersVideo;
        public Texture tortureButton;
        public Texture threatPicture;
        public Texture counterattackPicture;
        public Texture divineInterventionPicture;
        public Texture seizeImage;
        public VideoClip afflictionDetailsVideoClip;
        public VideoClip breedVideoClip;
        public Texture infoTab;
        public Texture moodTab;
        public Texture relationsTab;
        public Texture logsTab;
        public VideoClip homeStructureVideo;
        public Texture necronomiconPicture;
        public Texture griefstrickenLog;
        public Texture killedByMonsterLog;
        public Texture tileObjectOwner;
        public Texture structureInfoResidents;
        public Texture assumedThief;
        public Texture boobyTrapLog;
        public Texture infectedLog;
        public Texture recipientLog;
        public Texture brainWashButton;
        public VideoClip defilerChamberVideo;
        public Texture factionInfo;
        
        public bool hasCompletedImportantTutorials { get; private set; }
        

        #region Monobehaviours
        private void Awake() {
            Instance = this;
        }
        private void LateUpdate() {
            if (GameManager.Instance.gameHasStarted) {
                CheckIfNewTutorialCanBeActivated();    
            }
        }
        #endregion

        #region Initialization
        public void Initialize() {
            _activeImportantTutorials = new List<ImportantTutorial>();
            _waitingImportantTutorials = new List<ImportantTutorial>();
            _activeBonusTutorials = new List<BonusTutorial>();
            _instantiatedTutorials = new List<TutorialQuest>();
            _completedImportantTutorials = new List<Tutorial>();
            hasCompletedImportantTutorials = WorldSettings.Instance.worldSettingsData.worldType != WorldSettingsData.World_Type.Tutorial;
            InstantiatePendingBonusTutorials();
        }
        /// <summary>
        /// Instantiate all Important tutorials. NOTE: This is called after Start Popup is hidden
        /// <see cref="DemoUI.HideStartDemoScreen"/>
        /// </summary>
        public void InstantiateImportantTutorials() {
            for (int i = 0; i < mainTutorialTypes.Length; i++) {
                Tutorial tutorial = mainTutorialTypes[i];
                InstantiateTutorial(tutorial);
            }
        }
        public void InstantiatePendingBonusTutorials() {
            //Create instances for all uncompleted tutorials.
            List<Tutorial> completedTutorials = SaveManager.Instance.currentSaveDataPlayer.completedBonusTutorials;
            for (int i = 0; i < bonusTutorialTypes.Length; i++) {
                Tutorial tutorial = bonusTutorialTypes[i];
                
                // //Do not instantiate important tutorials here. That should be handled in InstantiateImportantTutorials
                // if (importantTutorialTypes.Contains(tutorial)) { continue; }
                
                //only instantiate tutorial if it has not yet been completed and has not yet been instantiated
                bool instantiateTutorial = completedTutorials.Contains(tutorial) == false && _instantiatedTutorials.Count(quest => quest.tutorialType == tutorial) == 0;
                if (instantiateTutorial) {
                   InstantiateTutorial(tutorial);
                }
            }
        }
        public TutorialQuest InstantiateTutorial(Tutorial tutorial) {
            string noSpacesName = UtilityScripts.Utilities.RemoveAllWhiteSpace(UtilityScripts.Utilities.
                NormalizeStringUpperCaseFirstLettersNoSpace(tutorial.ToString()));
            string typeName = $"Tutorial.{ noSpacesName }, Assembly-CSharp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
            Type type = Type.GetType(typeName);
            if (type != null) {
                TutorialQuest tutorialQuest = Activator.CreateInstance(type) as TutorialQuest;
                _instantiatedTutorials.Add(tutorialQuest);
                return tutorialQuest;
            }
            throw new Exception($"Could not instantiate tutorial quest {noSpacesName}");
        }
        #endregion

        #region Inquiry
        public bool HasTutorialBeenCompleted(Tutorial tutorial) {
            return SaveManager.Instance.currentSaveDataPlayer.completedBonusTutorials.Contains(tutorial) || _completedImportantTutorials.Contains(tutorial);
        }
        public bool HasTutorialBeenCompletedInCurrentPlaythrough(Tutorial tutorial) {
            return _completedImportantTutorials.Contains(tutorial);
        }
        public bool IsTutorialCurrentlyActive(Tutorial tutorial) {
            return _instantiatedTutorials.Any(t => t.tutorialType == tutorial && t.isActivated);
        }
        private bool IsBonusTutorial(TutorialQuest tutorialQuest) {
            return tutorialQuest is BonusTutorial;
        }
        public int GetAllActiveTutorialsCount() {
            return _activeBonusTutorials.Count + _activeImportantTutorials.Count + _waitingImportantTutorials.Count;
        }
        #endregion

        #region Completion
        public void CompleteTutorialQuest(TutorialQuest tutorial) {
            if (tutorial is ImportantTutorial) {
                _completedImportantTutorials.Add(tutorial.tutorialType);
            } else {
                SaveManager.Instance.currentSaveDataPlayer.AddBonusTutorialAsCompleted(tutorial.tutorialType);    
            }
            Messenger.Broadcast(Signals.TUTORIAL_QUEST_COMPLETED, tutorial);
            DeactivateTutorial(tutorial);
            if (IsBonusTutorial(tutorial) == false) {
                CheckIfAllTutorialsCompleted();    
            }
        }
        private void CheckIfAllTutorialsCompleted() {
            if (_instantiatedTutorials.Count == 0 || _instantiatedTutorials.Count(x => IsBonusTutorial(x) == false) == 0) {
                //all non-bonus tutorials completed
                UIManager.Instance.ShowYesNoConfirmation("Finished Tutorial",
                    "You're done with the Tutorials! You can continue playing around with this world. " +
                    "Another pregenerated world has also been unlocked on the World Options. You can skip ahead to that now!",
                    yesBtnText: "Go to next world", noBtnText: "Continue with this world", 
                    onClickYesAction: OnClickGoToNextWorld, pauseAndResume: true, showCover: true, layer: 25);
                hasCompletedImportantTutorials = true;
                Messenger.Broadcast(Signals.FINISHED_IMPORTANT_TUTORIALS);
            }
        }
        private void OnClickGoToNextWorld() {
            DOTween.Clear(true);
            Messenger.Cleanup();
            // AudioManager.Instance.SetCameraParent(null);
            WorldSettings.Instance.worldSettingsData.SetSecondWorldSettings();
            MainMenuManager.Instance.StartNewGame();
        }
        #endregion

        #region Failure
        public void FailTutorialQuest(TutorialQuest tutorial) {
            DeactivateTutorial(tutorial);
        }
        #endregion

        #region Availability
        public void AddTutorialToWaitList(ImportantTutorial tutorialQuest) {
            _waitingImportantTutorials.Add(tutorialQuest);
            CheckIfNewTutorialCanBeActivated();
        }
        public void RemoveTutorialFromWaitList(ImportantTutorial tutorialQuest) {
            _waitingImportantTutorials.Remove(tutorialQuest);
        }
        #endregion

        #region Presentation
        private void CheckIfNewTutorialCanBeActivated() {
            if (_waitingImportantTutorials.Count > 0 && _activeImportantTutorials.Count < MaxActiveTutorials) {
                //new tutorial can be shown.
                //check number of tutorials that can be shown. 3 at maximum
                int missingTutorials = MaxActiveTutorials - _activeImportantTutorials.Count;
                if (missingTutorials > _waitingImportantTutorials.Count) {
                    //if number of missing tutorials is greater than the available tutorials, then just show the available ones.
                    missingTutorials = _waitingImportantTutorials.Count;
                }
                for (int i = 0; i < missingTutorials; i++) {
                    //get first tutorial in list, since tutorials are sorted by priority beforehand.
                    ImportantTutorial availableTutorial = _waitingImportantTutorials[0];
                    ActivateTutorial(availableTutorial);        
                }
            }
        }
        private void ActivateTutorial(ImportantTutorial tutorialQuest) {
            _activeImportantTutorials.Add(tutorialQuest);    
            RemoveTutorialFromWaitList(tutorialQuest);
            tutorialQuest.Activate();
            ShowTutorial(tutorialQuest);
        }
        public void ActivateTutorial(BonusTutorial bonusTutorial) {
            _activeBonusTutorials.Add(bonusTutorial);
            bonusTutorial.Activate();
            ShowTutorial(bonusTutorial);
        }
        public void ActivateTutorial(LogQuest logQuest) {
            _activeBonusTutorials.Add(logQuest);
            logQuest.Activate();
        }
        public void ShowTutorial(TutorialQuest tutorialQuest) {
            Assert.IsTrue(tutorialQuest.isActivated, $"{tutorialQuest.questName} is being shown, but has not yet been activated.");
            QuestItem questItem = UIManager.Instance.questUI.ShowQuest(tutorialQuest);
            tutorialQuest.SetQuestItem(questItem);
        }
        private void DeactivateTutorial(TutorialQuest tutorialQuest) {
            if (tutorialQuest is ImportantTutorial importantTutorial) {
                _activeImportantTutorials.Remove(importantTutorial);
                _waitingImportantTutorials.Remove(importantTutorial); //this is for cases when a tutorial is in the waiting list, but has been deactivated.    
            } else if (tutorialQuest is BonusTutorial bonusTutorial) {
                _activeBonusTutorials.Remove(bonusTutorial);
            }
            _instantiatedTutorials.Remove(tutorialQuest);
            if (tutorialQuest.questItem != null) {
                UIManager.Instance.questUI.HideQuestDelayed(tutorialQuest);
            }
            tutorialQuest.Deactivate();
        }
        #endregion

        // #region For Testing
        // public void ResetTutorials() {
        //     List<Tutorial> completedTutorials = new List<Tutorial>(SaveManager.Instance.currentSaveDataPlayer.completedTutorials);
        //     SaveManager.Instance.currentSaveDataPlayer.ResetTutorialProgress();
        //     //respawn previously completed tutorials
        //     Tutorial[] allTutorials = CollectionUtilities.GetEnumValues<Tutorial>();
        //     for (int i = 0; i < allTutorials.Length; i++) {
        //         Tutorial tutorial = allTutorials[i];
        //         if (completedTutorials.Contains(tutorial)) {
        //            InstantiateTutorial(tutorial);
        //         }
        //     }
        // }
        // #endregion


    }
}