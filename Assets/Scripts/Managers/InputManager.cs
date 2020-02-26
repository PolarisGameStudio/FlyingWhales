﻿using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

namespace Ruinarch {
    public class InputManager : MonoBehaviour {

        public static InputManager Instance;
        public bool isDraggingItem;

        private CursorMode cursorMode = CursorMode.ForceSoftware;

        private readonly List<System.Action> _leftClickActions = new List<System.Action>();
        private readonly List<System.Action> _pendingLeftClickActions = new List<System.Action>();
        private readonly List<System.Action> _rightClickActions = new List<System.Action>();

        [SerializeField] private CursorTextureDictionary cursors;

        public enum Cursor_Type {
            None, Default, Target, Drag_Hover, Drag_Clicked, Check, Cross, Link
        }
        public Cursor_Type currentCursorType;
        public Cursor_Type previousCursorType;
        public PLAYER_ARCHETYPE selectedArchetype { get; private set; } //Need to move this in the future. Not the best way to store the selected archetype from the main menu scene, but for now this will do since we need an object that is carried to the Game scene

        #region Monobehaviours
        private void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetCursorTo(Cursor_Type.Default);
                previousCursorType = Cursor_Type.Default;
                Cursor.lockState = CursorLockMode.Confined;
            } else {
                Destroy(gameObject);
            }
        }
        private void Update() { 
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
                            SetCursorTo(hoveredTile.objHere != null ? Cursor_Type.Cross : Cursor_Type.Check);
                        } else {
                            SetCursorTo(Cursor_Type.Cross);
                        }
                    }
                } else if (PlayerManager.Instance.player.currentActivePlayerSpell != null) { 
                    if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
                        SetCursorTo(Cursor_Type.Default); 
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
                                    if (hoveredTile != null && hoveredTile.buildSpotOwner.hexTileOwner) { 
                                        canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile.buildSpotOwner.hexTileOwner); 
                                    } 
                                    break; 
                                default: 
                                    break; 
                            }
                            SetCursorTo(canTarget ? Cursor_Type.Check : Cursor_Type.Cross);
                        }
                        if(hoveredPOI != null) { 
                            if (hoverText != string.Empty) { 
                                UIManager.Instance.ShowSmallInfo(hoverText); 
                            } 
                        } else { 
                            UIManager.Instance.HideSmallInfo(); 
                        } 
                    }
                } else if (PlayerManager.Instance.player.currentActiveCombatAbility != null) {
                    UIManager.Instance.HideSmallInfo();
                    CombatAbility ability = PlayerManager.Instance.player.currentActiveCombatAbility;
                    if (ability.abilityRadius == 0) {
                        IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi;
                        if (hoveredPOI != null) {
                            SetCursorTo(ability.CanTarget(hoveredPOI) ? Cursor_Type.Check : Cursor_Type.Cross);
                        }
                    } else {
                        LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                        if (hoveredTile != null) {
                            SetCursorTo(Cursor_Type.Check);
                            List<LocationGridTile> highlightTiles = hoveredTile.GetTilesInRadius(ability.abilityRadius, includeCenterTile: true, includeTilesInDifferentStructure: true);
                            if (InnerMapManager.Instance.currentlyHighlightedTiles != null) {
                                InnerMapManager.Instance.UnhighlightTiles();
                                InnerMapManager.Instance.HighlightTiles(highlightTiles);
                            } else {
                                InnerMapManager.Instance.HighlightTiles(highlightTiles);
                            }
                        }
                    }
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
            
            if (Input.GetMouseButtonDown(0)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Mouse0);
            } else if (Input.GetMouseButtonDown(1)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Mouse1);
                CancelActionsByPriority();
            } else if (Input.GetKeyDown(KeyCode.BackQuote)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.BackQuote);
            } else if (Input.GetKeyDown(KeyCode.Space)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Space);
            } else if (Input.GetKeyDown(KeyCode.Alpha1)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Alpha1);
            } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Alpha2);
            } else if (Input.GetKeyDown(KeyCode.Alpha3)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Alpha3);
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                Messenger.Broadcast(Signals.KEY_DOWN, KeyCode.Escape);
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
        public void SetSelectedArchetype(PLAYER_ARCHETYPE archetype) {
            selectedArchetype = archetype;
        }
        /// <summary>
        /// Cancel actions based on a hardcoded process
        /// </summary>
        private void CancelActionsByPriority() {
            if (PlayerManager.Instance.player.currentActivePlayerSpell != null) {
                //cancel current spell
                PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
            } else if (PlayerManager.Instance.player.currentActiveIntel != null) {
                //cancel current intel
                PlayerManager.Instance.player.SetCurrentActiveIntel(null);
            } else {
                if (UIManager.Instance.openedPopups.Count > 0) {
                    //close latest popup
                    UIManager.Instance.openedPopups.Last().Close();
                } else {
                    if (UIManager.Instance.poiTestingUI.gameObject.activeSelf ||
                        UIManager.Instance.minionCommandsUI.gameObject.activeSelf) {
                        return;
                    }
                    //close all other menus
                    Messenger.Broadcast(Signals.HIDE_MENUS);    
                }
            }
        }
    }
}