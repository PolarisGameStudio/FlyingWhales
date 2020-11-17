﻿using System;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Ruinarch.Custom_UI;
using Settings;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Ruinarch {
    public class InputManager : MonoBehaviour {

        public static InputManager Instance;
        public bool isDraggingItem;

        private CursorMode cursorMode = CursorMode.ForceSoftware;

        private readonly List<System.Action> _leftClickActions = new List<System.Action>();
        private readonly List<System.Action> _pendingLeftClickActions = new List<System.Action>();
        private readonly List<System.Action> _rightClickActions = new List<System.Action>();

        [Space(10)] 
        [Header("Cursors")] 
        [SerializeField] private CursorTextureDictionary cursors;
        
        [Space(10)] 
        [Header("Buttons")] 
        public Sprite buttonGlowImage;
        
        public HashSet<string> buttonsToHighlight { get; private set; }

        public enum Cursor_Type {
            None, Default, Target, Drag_Hover, Drag_Clicked, Check, Cross, Link
        }
        public Cursor_Type currentCursorType;
        public Cursor_Type previousCursorType;
        //public PLAYER_ARCHETYPE selectedArchetype { get; private set; } //Need to move this in the future. Not the best way to store the selected archetype from the main menu scene, but for now this will do since we need an object that is carried to the Game scene
        private bool runUpdate;
        
        
        #region Monobehaviours
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetCursorTo(Cursor_Type.Default);
                previousCursorType = Cursor_Type.Default;
                // Cursor.lockState = CursorLockMode.Confined;
                SceneManager.activeSceneChanged += OnActiveSceneChanged;
                Initialize();
            } else {
                Destroy(gameObject);
            }
        }
        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) && GameManager.Instance == null) {
                if (SettingsManager.Instance.IsShowing()) {
                    SettingsManager.Instance.CloseSettings();
                    return;
                }
            } else if (Input.GetKeyDown(KeyCode.F8)) {
                if (!CanUseHotkeys()) return;
                ReportABug();
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.F8);
            }
            
            if (runUpdate == false) { return; }
            if (ReferenceEquals(PlayerManager.Instance, null) == false && PlayerManager.Instance.player != null) {
                if (PlayerManager.Instance.player.seizeComponent.hasSeizedPOI) {
                    if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
                        SetCursorTo(Cursor_Type.Default);
                        PlayerManager.Instance.player.seizeComponent.DisableFollowMousePosition();
                    } else {
                        PlayerManager.Instance.player.seizeComponent.EnableFollowMousePosition();
                        PlayerManager.Instance.player.seizeComponent.FollowMousePosition();
                        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                        if (hoveredTile != null) {
                            SetCursorTo(PlayerManager.Instance.player.seizeComponent.CanUnseizeHere(hoveredTile) ? Cursor_Type.Check : Cursor_Type.Cross);
                        } else {
                            SetCursorTo(Cursor_Type.Cross);
                        }
                    }
                } else if (PlayerManager.Instance.player.currentActivePlayerSpell != null) { 
                    if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
                        SetCursorTo(Cursor_Type.Default); 
                        PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();
                    } else { 
                        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                        bool canTarget = false; 
                        IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi; 
                        string hoverText = string.Empty; 
                        for (int i = 0; i < PlayerManager.Instance.player.currentActivePlayerSpell.targetTypes.Length; i++) {
                            switch (PlayerManager.Instance.player.currentActivePlayerSpell.targetTypes[i]) { 
                                case SPELL_TARGET.CHARACTER: 
                                case SPELL_TARGET.TILE_OBJECT: 
                                    if (hoveredPOI != null) { 
                                        canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredPOI, ref hoverText); 
                                    } 
                                    break; 
                                case SPELL_TARGET.TILE: 
                                    if (hoveredTile != null) {
                                        canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile); 
                                    } 
                                    break; 
                                case SPELL_TARGET.HEX: 
                                    if (hoveredTile != null && hoveredTile.collectionOwner.isPartOfParentRegionMap && hoveredTile.collectionOwner.partOfHextile.hexTileOwner) { 
                                        canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile.collectionOwner.partOfHextile.hexTileOwner); 
                                    } 
                                    break; 
                                default: 
                                    break; 
                            }
                            SetCursorTo(canTarget ? Cursor_Type.Check : Cursor_Type.Cross);
                        }
                        if (canTarget) {
                            PlayerManager.Instance.player.currentActivePlayerSpell.HighlightAffectedTiles(hoveredTile);
                        } else {
                            if (hoveredTile == null || PlayerManager.Instance.player.currentActivePlayerSpell.InvalidHighlight(hoveredTile, ref hoverText) == false) {
                                PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();    
                            }
                        }
                        if(!string.IsNullOrEmpty(hoverText)) {
                            UIManager.Instance.ShowSmallInfo(hoverText);
                        } else { 
                            UIManager.Instance.HideSmallInfo(); 
                        } 
                    }
                } else if (PlayerManager.Instance.player.currentActiveCombatAbility != null) {
                    // UIManager.Instance.HideSmallInfo();
                    // CombatAbility ability = PlayerManager.Instance.player.currentActiveCombatAbility;
                    // if (ability.abilityRadius == 0) {
                    //     IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi;
                    //     if (hoveredPOI != null) {
                    //         SetCursorTo(ability.CanTarget(hoveredPOI) ? Cursor_Type.Check : Cursor_Type.Cross);
                    //     }
                    // } else {
                    //     LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                    //     if (hoveredTile != null) {
                    //         SetCursorTo(Cursor_Type.Check);
                    //         List<LocationGridTile> highlightTiles = hoveredTile.GetTilesInRadius(ability.abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                    //         if (InnerMapManager.Instance.currentlyHighlightedTiles != null) {
                    //             InnerMapManager.Instance.UnhighlightTiles();
                    //             InnerMapManager.Instance.HighlightTiles(highlightTiles);
                    //         } else {
                    //             InnerMapManager.Instance.HighlightTiles(highlightTiles);
                    //         }
                    //     }
                    // }
                } else if (PlayerManager.Instance.player.currentActiveIntel != null) {
                    IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi;
                    if (hoveredPOI != null) {
                        string hoverText = string.Empty;
                        SetCursorTo(PlayerManager.Instance.player.CanShareIntel(hoveredPOI, ref hoverText)
                            ? Cursor_Type.Check
                            : Cursor_Type.Cross);
                        if(hoverText != string.Empty) {
                            UIManager.Instance.ShowSmallInfo(hoverText);
                        }
                    } else {
                        UIManager.Instance.HideSmallInfo();
                        SetCursorTo(Cursor_Type.Cross);
                    }
                }
            }
            
            if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive()) {
                //Do not allow any hotkeys while loading
                return;
            }
            if (Input.GetMouseButtonDown(0)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Mouse0);
            } else if (Input.GetMouseButtonDown(1)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Mouse1);
                CancelActionsByPriority();
            } else if (Input.GetKeyDown(KeyCode.BackQuote)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.BackQuote);
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Space);
            } else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Alpha1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Alpha2);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Alpha3);
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Escape);
                if (UIManager.Instance != null) {
                    if (!CancelActionsByPriority(true)) {
                        //if no actions were cancelled then show options menu if itt is not yet showing.
                        //if game has started then, check if options menu is not showing, if it is not, then
                        //show options menu, then do not cancel any actions.
                        if (!UIManager.Instance.IsOptionsMenuShowing()) {
                            UIManager.Instance.OpenOptionsMenu();
                            return;
                        }    
                    }
                }
                // CancelActionsByPriority();
            } else if (Input.GetKeyDown(KeyCode.F1)) {
                BroadcastHotkeyPress("Spells Tab");
            } else if (Input.GetKeyDown(KeyCode.F2)) {
                BroadcastHotkeyPress("Demons Tab");
            } else if (Input.GetKeyDown(KeyCode.F3)) {
                BroadcastHotkeyPress("Monsters Tab");
            } else if (Input.GetKeyDown(KeyCode.F4)) {
                BroadcastHotkeyPress("Intel Tab");
            } else if (Input.GetKeyDown(KeyCode.F5)) {
                BroadcastHotkeyPress("Villagers Tab");
            } else if (Input.GetKeyDown(KeyCode.F6)) {
                BroadcastHotkeyPress("Build Tab");
            } else if (Input.GetKeyDown(KeyCode.F7)) {
                BroadcastHotkeyPress("Cultist Tab");
            } else if (Input.GetKeyDown(KeyCode.M)) {
                BroadcastHotkeyPress("ToggleMapBtn");
            } else if (Input.GetKeyDown(KeyCode.F9)) {
                if (!CanUseHotkeys()) return;
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.F9);
            } else if (Input.GetKeyDown(KeyCode.Tab)) {
                if (!CanUseHotkeys()) return;
                if (HasSelectedUIObject()) { return; } //if currently selecting a UI object, ignore (This is mostly for Input fields)
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.Tab);
            } else if (Input.GetKeyDown(KeyCode.R)) {
                if (!CanUseHotkeys()) return;
                if (HasSelectedUIObject()) { return; } //if currently selecting a UI object, ignore (This is mostly for Input fields)
                Messenger.Broadcast(ControlsSignals.KEY_DOWN, KeyCode.R);
            }
        }
        private void BroadcastHotkeyPress(string buttonToActivate) {
            if (!CanUseHotkeys()) return;
            if (HasSelectedUIObject()) { return; } //if currently selecting a UI object, ignore (This is mostly for Input fields)
            Messenger.Broadcast(UISignals.HOTKEY_CLICK, buttonToActivate);
        }
        public bool CanUseHotkeys() {
            if (SaveManager.Instance.saveCurrentProgressManager.isSaving) {
                //Do not allow hotkeys while saving
                return false;
            }
            if (LevelLoaderManager.Instance.isLoadingNewScene || LevelLoaderManager.Instance.IsLoadingScreenActive()) {
                //Do not allow hotkeys while loading
                return false;
            }
            if (PlayerUI.Instance != null && PlayerUI.Instance.IsMajorUIShowing()) {
                return false;
            }
            if (UIManager.Instance != null && UIManager.Instance.IsObjectPickerOpen()) {
                return false;
            }
            return true;
        }
        #endregion

        #region Initialization
        private void Initialize() {
            buttonsToHighlight = new HashSet<string>();
            Messenger.MarkAsPermanent(UISignals.SHOW_SELECTABLE_GLOW);
            Messenger.MarkAsPermanent(UISignals.HIDE_SELECTABLE_GLOW);
            Messenger.MarkAsPermanent(UISignals.TOGGLE_SHOWN);
            Messenger.AddListener<string>(UISignals.SHOW_SELECTABLE_GLOW, OnReceiveHighlightSignal);
            Messenger.AddListener<string>(UISignals.HIDE_SELECTABLE_GLOW, OnReceiveUnHighlightSignal);
            Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_SHOWN, OnToggleShown);
            Messenger.AddListener<RuinarchButton>(UISignals.BUTTON_SHOWN, OnButtonShown);
        }
        private void OnReceiveHighlightSignal(string name) {
            buttonsToHighlight.Add(name);
        }
        private void OnReceiveUnHighlightSignal(string name) {
            buttonsToHighlight.Remove(name);
        }
        private void OnToggleShown(RuinarchToggle toggle) {
            if (buttonsToHighlight.Contains(toggle.name)) {
                toggle.StartGlow();
            }
        }
        private void OnButtonShown(RuinarchButton button) {
            if (buttonsToHighlight.Contains(button.name)) {
                button.StartGlow();
            }
        }
        #endregion

        public void SetCursorTo(Cursor_Type type) {
            if (currentCursorType == type) {
                return; //ignore 
            }
            previousCursorType = currentCursorType;
            Vector2 hotSpot = Vector2.zero;
            switch (type) {
                case Cursor_Type.Drag_Clicked:
                    isDraggingItem = true;
                    break;
                case Cursor_Type.Check:
                case Cursor_Type.Cross:
                case Cursor_Type.Link:
                    hotSpot = new Vector2(12f, 10f);
                    break;
                case Cursor_Type.Target:
                    hotSpot = new Vector2(29f, 29f);
                    break;
                default:
                    isDraggingItem = false;
                    break;
            }
            currentCursorType = type;
            Cursor.SetCursor(cursors[type], hotSpot, cursorMode);
        }
        public void RevertToPreviousCursor() {
            SetCursorTo(previousCursorType);
        }
        //public void SetSelectedArchetype(PLAYER_ARCHETYPE archetype) {
        //    selectedArchetype = archetype;
        //}
        /// <summary>
        /// Cancel actions based on a hardcoded process
        /// </summary>
        private bool CancelActionsByPriority(bool ignoreCursor = false) {
            if (SettingsManager.Instance.IsShowing()) {
                SettingsManager.Instance.CloseSettings();
                return true;
            }
            if (UIManager.Instance == null) {
                return true;
            }
            if (SaveManager.Instance != null && SaveManager.Instance.saveCurrentProgressManager.isSaving) {
                return true;
            }
            // if (PlayerManager.Instance == null) {
            //     return true;
            // }
            UIManager.Instance.SetTempDisableShowInfoUI(false);
            if (UIManager.Instance.IsOptionsMenuShowing()) {
                //if options menu is showing, check if load window is showing, if it is close load window.
                if (UIManager.Instance.optionsMenu.IsLoadWindowShowing()) {
                    UIManager.Instance.optionsMenu.CloseLoadWindow();
                    return true;
                }
                //if load window is not showing then close options menu
                UIManager.Instance.CloseOptionsMenu();
                return true;
            }
            if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActivePlayerSpell != null) {
                //cancel current spell
                PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
                return true;
            } else if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveIntel != null) {
                //cancel current intel
                PlayerManager.Instance.player.SetCurrentActiveIntel(null);
                return true;
            } else if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveItem != TILE_OBJECT_TYPE.NONE) {
                PlayerManager.Instance.player.SetCurrentlyActiveItem(TILE_OBJECT_TYPE.NONE);
                return true;
            } else if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.currentActiveArtifact != ARTIFACT_TYPE.None) {
                PlayerManager.Instance.player.SetCurrentlyActiveArtifact(ARTIFACT_TYPE.None);
                return true;
            } else {
                CustomStandaloneInputModule customModule = EventSystem.current.currentInputModule as CustomStandaloneInputModule;
                if (ignoreCursor || !EventSystem.current.IsPointerOverGameObject() || customModule.GetPointerData().pointerEnter.GetComponent<Button>() == null) {
                    if (UIManager.Instance.openedPopups.Count > 0) {
                        //close latest popup
                        UIManager.Instance.openedPopups.Last().Close();
                        return true;
                    } else {
                        if (UIManager.Instance.poiTestingUI.gameObject.activeSelf ||
                            UIManager.Instance.minionCommandsUI.gameObject.activeSelf) {
                            return true;
                        }
                        //close latest Info UI
                        if (UIManager.Instance.latestOpenedInfoUI != null) {
                            //close latest popup
                            UIManager.Instance.latestOpenedInfoUI.OnClickCloseMenu();
                            return true;
                        }
                    }
                }
                return false;
                //if (UIManager.Instance.openedPopups.Count > 0) {
                //    //close latest popup
                //    UIManager.Instance.openedPopups.Last().Close();
                //} else {
                //    if (UIManager.Instance.poiTestingUI.gameObject.activeSelf ||
                //        UIManager.Instance.minionCommandsUI.gameObject.activeSelf) {
                //        return;
                //    }
                //    //close all other menus
                //    Messenger.Broadcast(Signals.HIDE_MENUS);
                //}
            }
        }

        #region Utilities
        private void OnActiveSceneChanged(Scene current, Scene next) {
            if (next.name == "Game") {
                runUpdate = true;
            } else {
                runUpdate = false;
            }
            buttonsToHighlight.Clear();
        }
        public bool ShouldBeHighlighted(RuinarchButton button) {
            return buttonsToHighlight.Contains(button.name);
        }
        public bool ShouldBeHighlighted(RuinarchToggle button) {
            return buttonsToHighlight.Contains(button.name);
        }
        public bool HasSelectedUIObject() {
            return EventSystem.current.currentSelectedGameObject != null;
        }
        #endregion

        #region Selection
        public void Select(ISelectable objToSelect) {
            objToSelect.LeftSelectAction();
            Messenger.Broadcast(ControlsSignals.SELECTABLE_LEFT_CLICKED, objToSelect);
        }
        #endregion

        #region Report A Bug
        private void ReportABug() {
            YesNoConfirmation yesNoConfirmation = null;
            if (UIManager.Instance != null) {
                yesNoConfirmation = UIManager.Instance.yesNoConfirmation;
            } else if (MainMenuUI.Instance != null) {
                yesNoConfirmation = MainMenuUI.Instance.yesNoConfirmation;
            }
            if (yesNoConfirmation != null) {
                if (!yesNoConfirmation.isShowing) {
                    yesNoConfirmation.ShowYesNoConfirmation("Open Browser", "To report a bug, the game needs to open a Web browser, do you want to proceed?",
                        () => Application.OpenURL("https://forms.gle/gcoa8oHxywFLegNx7"), layer: 50, showCover: true);    
                }
            }
        }
        #endregion
    }
}