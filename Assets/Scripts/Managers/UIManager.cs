﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class UIManager : MonoBehaviour {

    public static UIManager Instance = null;

    public delegate void OnAddNewBattleLog();
    public OnAddNewBattleLog onAddNewBattleLog;

    public Camera uiCamera;
    public RectTransform mainRT;
    [SerializeField] private EventSystem eventSystem;

    [Space(10)]
    [Header("Unified Settings")]
    public UnifiedUISettings settings;

    [Space(10)]
    [SerializeField] UIMenu[] allMenus;

    [Space(10)]
    [Header("Prefabs")]
    [SerializeField] private GameObject notificationPrefab;

    [Space(10)]
    [Header("Main UI Objects")]
    [SerializeField] private GameObject mainUIGO;


    [Space(10)]
    [Header("Date Objects")]
    [SerializeField] private ToggleGroup speedToggleGroup;
    [SerializeField] private Toggle pauseBtn;
    [SerializeField] private TextMeshProUGUI pauseBtnLbl;
    [SerializeField] private Toggle x1Btn;
    [SerializeField] private TextMeshProUGUI x1BtnLbl;
    [SerializeField] private Toggle x2Btn;
    [SerializeField] private TextMeshProUGUI x2BtnLbl;
    [SerializeField] private Toggle x4Btn;
    [SerializeField] private TextMeshProUGUI x4BtnLbl;
    [SerializeField] private TextMeshProUGUI dateLbl;

    [Space(10)]
    [Header("Small Info")]
    public GameObject smallInfoGO;
    public RectTransform smallInfoRT;
    public TextMeshProUGUI smallInfoLbl;
    public EnvelopContentUnityUI smallInfoEnvelopContent;

    [Space(10)]
    [Header("Detailed Info")]
    public GameObject detailedInfoGO;
    public TextMeshProUGUI detailedInfoLbl;

    [Space(10)]
    [Header("World Info Menu")]
    [SerializeField] private GameObject worldInfoCharactersSelectedGO;
    [SerializeField] private GameObject worldInfoQuestsSelectedGO;
    [SerializeField] private GameObject worldInfoStorylinesSelectedGO;
    [SerializeField] private GameObject worldInfoCharactersBtn;
    [SerializeField] private GameObject worldInfoQuestsBtn;
    [SerializeField] private GameObject worldInfoStorylinesBtn;

    [Space(10)]
    [Header("Popup Message Box")]
    [SerializeField] private PopupMessageBox popupMessageBox;

    [Space(10)]
    [Header("Notification Area")]
    [SerializeField] private PlayerNotificationArea notificationArea;

    [Space(10)]
    [Header("Character Dialog Menu")]
    [SerializeField] private CharacterDialogMenu characterDialogMenu;

    [Space(10)]
    [Header("Portraits")]
    public Transform characterPortraitsParent;

    public Color onToggleTextColor;
    public Color offToggleTextColor;

    [Space(10)] //FOR TESTING
    [Header("For Testing")]
    public ButtonToggle toggleBordersBtn;
    public ButtonToggle corruptionBtn;

    public delegate void OnPauseEventExpiration(bool state);
    public OnPauseEventExpiration onPauseEventExpiration;

    [Space(10)]
    [Header("Font Sizes")]
    [SerializeField] private int HEADER_FONT_SIZE = 25;
    [SerializeField] private int BODY_FONT_SIZE = 20;
    [SerializeField] private int TOOLTIP_FONT_SIZE = 18;
    [SerializeField] private int SMALLEST_FONT_SIZE = 12;

    internal List<object> eventLogsQueue = new List<object>();

    private List<UIMenuSettings> _menuHistory;

    #region getters/setters
    //internal GameObject minimapTexture {
    //    get { return minimapTextureGO; }
    //}
    internal List<UIMenuSettings> menuHistory {
        get { return _menuHistory; }
    }
    #endregion

    #region Monobehaviours
    private void Awake() {
        Instance = this;
        _menuHistory = new List<UIMenuSettings>();
        Messenger.AddListener<bool>(Signals.PAUSED, UpdateSpeedToggles);
    }
    private void Start() {
        Messenger.AddListener(Signals.UPDATE_UI, UpdateUI);
        NormalizeFontSizes();
        ToggleBorders();
    }
    private void Update() {
        if (Input.GetKeyDown(KeyCode.BackQuote)) {
            if (GameManager.Instance.allowConsole) {
                ToggleConsole();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (contextMenu.gameObject.activeSelf) {
                HideContextMenu();
            }
        }
        UpdateSpeedToggles(GameManager.Instance.isPaused);
    }
    #endregion

    public void SetTimeControlsState(bool state) {
        pauseBtn.interactable = state;
        x1Btn.interactable = state;
        x2Btn.interactable = state;
        x4Btn.interactable = state;
    }

    internal void InitializeUI() {
        for (int i = 0; i < allMenus.Length; i++) {
            allMenus[i].Initialize();
            allMenus[i].ApplyUnifiedSettings(settings);
        }
        UnifiedSelectableBehaviour[] selectables = this.GetComponentsInChildren<UnifiedSelectableBehaviour>(true);
        for (int i = 0; i < selectables.Length; i++) {
            selectables[i].Initialize();
        }
        //popupMessageBox.Initialize();
        Messenger.AddListener<HexTile>(Signals.TILE_RIGHT_CLICKED, ShowContextMenu);
        Messenger.AddListener<HexTile>(Signals.TILE_LEFT_CLICKED, HideContextMenu);
        Messenger.AddListener<string, int, UnityAction>(Signals.SHOW_NOTIFICATION, ShowNotification);
    }

    #region Font Utilities
    private void NormalizeFontSizes() {
        TextMeshProUGUI[] allLabels = this.GetComponentsInChildren<TextMeshProUGUI>(true);
        //Debug.Log ("ALL LABELS COUNT: " + allLabels.Length.ToString());
        for (int i = 0; i < allLabels.Length; i++) {
            NormalizeFontSizeOfLabel(allLabels[i]);
        }
    }
    private void NormalizeFontSizeOfLabel(TextMeshProUGUI lbl) {
        string lblName = lbl.name;

        TextOverflowModes overflowMethod = TextOverflowModes.Truncate;
        if (lblName.Contains("HEADER")) {
            lbl.fontSize = HEADER_FONT_SIZE;
            overflowMethod = TextOverflowModes.Truncate;
        } else if (lblName.Contains("BODY")) {
            lbl.fontSize = BODY_FONT_SIZE;
            overflowMethod = TextOverflowModes.Truncate;
        } else if (lblName.Contains("TOOLTIP")) {
            lbl.fontSize = TOOLTIP_FONT_SIZE;
            overflowMethod = TextOverflowModes.Overflow;
        } else if (lblName.Contains("SMALLEST")) {
            lbl.fontSize = SMALLEST_FONT_SIZE;
            overflowMethod = TextOverflowModes.Truncate;
        }

        if (!lblName.Contains("NO")) {
            lbl.overflowMode = overflowMethod;
        }

    }
    #endregion

    private void UpdateUI() {
        dateLbl.SetText("Day " + GameManager.Instance.Today().GetDayAndTicksString());

        UpdateCharacterInfo();
        UpdateFactionInfo();
        UpdateHexTileInfo();
        //UpdateLandmarkInfo();
        UpdateMonsterInfo();
        UpdatePartyInfo();
        UpdateCombatLogs();
        UpdateQuestSummary();
    }

    #region World Controls
    private void UpdateSpeedToggles(bool isPaused) {
        if (isPaused) {
            pauseBtn.isOn = true;
            speedToggleGroup.NotifyToggleOn(pauseBtn);
        } else {
            if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X1) {
                x1Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x1Btn);
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X2) {
                x2Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x2Btn);
            } else if (GameManager.Instance.currProgressionSpeed == PROGRESSION_SPEED.X4) {
                x4Btn.isOn = true;
                speedToggleGroup.NotifyToggleOn(x4Btn);
            }
        }
        }
    public void SetProgressionSpeed1X() {
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X1);
        Unpause();
    }
    public void SetProgressionSpeed2X() {
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X2);
        Unpause();
    }
    public void SetProgressionSpeed4X() {
        GameManager.Instance.SetProgressionSpeed(PROGRESSION_SPEED.X4);
        Unpause();
    }
    public void Pause() {
        GameManager.Instance.SetPausedState(true);
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(true);
        }
    }
    public void Unpause() {
        GameManager.Instance.SetPausedState(false);
        if (onPauseEventExpiration != null) {
            onPauseEventExpiration(false);
        }
    }
    public void UpdatePauseToggleColor(bool isOn) {
        Color color = new Color();
        if (isOn) {
            ColorUtility.TryParseHtmlString("#495D6B", out color);
        } else {
            ColorUtility.TryParseHtmlString("#F7EED4", out color);
        }
        pauseBtnLbl.color = color;
    }
    public void Updatex1ToggleColor(bool isOn) {
        Color color = new Color();
        if (isOn) {
            ColorUtility.TryParseHtmlString("#495D6B", out color);
        } else {
            ColorUtility.TryParseHtmlString("#F7EED4", out color);
        }
        x1BtnLbl.color = color;
    }
    public void Updatex2ToggleColor(bool isOn) {
        Color color = new Color();
        if (isOn) {
            ColorUtility.TryParseHtmlString("#495D6B", out color);
        } else {
            ColorUtility.TryParseHtmlString("#F7EED4", out color);
        }
        x2BtnLbl.color = color;
    }
    public void Updatex4ToggleColor(bool isOn) {
        Color color = new Color();
        if (isOn) {
            ColorUtility.TryParseHtmlString("#495D6B", out color);
        } else {
            ColorUtility.TryParseHtmlString("#F7EED4", out color);
        }
        x4BtnLbl.color = color;
    }
    #endregion

    #region Minimap
    internal void UpdateMinimapInfo() {
        CameraMove.Instance.UpdateMinimapTexture();
    }
    #endregion

    #region coroutines
    public IEnumerator RepositionGrid(UIGrid thisGrid) {
        yield return null;
        if (thisGrid != null && this.gameObject.activeSelf) {
            thisGrid.Reposition();
        }
        yield return null;
    }
    public IEnumerator RepositionTable(UITable thisTable) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        thisTable.Reposition();
    }
    public IEnumerator RepositionScrollView(UIScrollView thisScrollView, bool keepScrollPosition = false) {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        if (keepScrollPosition) {
            thisScrollView.UpdatePosition();
        } else {
            thisScrollView.ResetPosition();
            thisScrollView.Scroll(0f);
        }
        yield return new WaitForEndOfFrame();
        thisScrollView.UpdateScrollbars();
    }
    public IEnumerator LerpProgressBar(UIProgressBar progBar, float targetValue, float lerpTime) {
        float elapsedTime = 0f;
        while (elapsedTime < lerpTime) {
            progBar.value = Mathf.Lerp(progBar.value, targetValue, (elapsedTime/lerpTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
    #endregion

    #region Tooltips
    public void ShowSmallInfo(string info) {
        //return;
        smallInfoLbl.text = info;
        smallInfoGO.SetActive(true);
        smallInfoEnvelopContent.Execute();
        PositionTooltip(smallInfoRT);
    }
    public void HideSmallInfo() {
        smallInfoGO.SetActive(false);
        //smallInfoGO.transform.parent = this.transform;
    }
    public void ShowDetailedInfo(IParty party) {
        detailedInfoLbl.text = party.name;

    }
    private void PositionTooltip(RectTransform rt) {
        var v3 = Input.mousePosition;

        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);

        v3.x += 25f;
        v3.y -= 25f;
        rt.position = v3;

        Vector3[] corners = new Vector3[4]; //bottom-left, top-left, top-right, bottom-right
        List<int> cornersOutside = new List<int>();
        rt.GetWorldCorners(corners);
        for (int i = 0; i < 4; i++) {
            // Backtransform to parent space
            Vector3 localSpacePoint = mainRT.InverseTransformPoint(corners[i]);
            // If parent (canvas) does not contain checked items any point
            if (!mainRT.rect.Contains(localSpacePoint)) {
                cornersOutside.Add(i);
            }
        }

        if (cornersOutside.Count != 0) {
            string log = "Corners outside are: ";
            for (int i = 0; i < cornersOutside.Count; i++) {
                log += cornersOutside[i].ToString() + ", ";
            }
            Debug.Log(log);
            if (cornersOutside.Contains(2) && cornersOutside.Contains(3)) {
                if (cornersOutside.Contains(0)) {
                    //bottom side and right side are outside, move anchor to bottom right
                    rt.anchorMin = new Vector2(1f, 0f);
                    rt.anchorMax = new Vector2(1f, 0f);
                    rt.pivot = new Vector2(1f, 0f);
                } else {
                    //right side is outside, move anchor to top right side
                    rt.anchorMin = new Vector2(1f, 1f);
                    rt.anchorMax = new Vector2(1f, 1f);
                    rt.pivot = new Vector2(1f, 1f);
                }
            } else if (cornersOutside.Contains(0) && cornersOutside.Contains(3)) {
                //bottom side is outside, move anchor to bottom left
                rt.anchorMin = new Vector2(0f, 0f);
                rt.anchorMax = new Vector2(0f, 0f);
                rt.pivot = new Vector2(0f, 0f);
            }
            rt.position = Input.mousePosition;
        }
    }
    #endregion

    #region Notifications Area
    private void ShowNotification(string text, int expirationTicks, UnityAction onClickAction) {
        notificationArea.ShowNotification(text, expirationTicks, onClickAction);
    }
    #endregion

    #region World History
    internal void AddLogToLogHistory(Log log) {
        Messenger.Broadcast<Log>("AddLogToHistory", log);
    }
    public void ToggleNotificationHistory() {
        //worldHistoryUI.ToggleWorldHistoryUI();
        //if (notificationHistoryGO.activeSelf) {
        //    HideNotificationHistory();
        //} else {
        //    ShowLogHistory();
        //}
    }
    #endregion

    #region UI Utilities
    public void RepositionGridCallback(UIGrid thisGrid) {
        StartCoroutine(RepositionGrid(thisGrid));
    }
    private void EnableUIButton(UIButton btn, bool state) {
        if (state) {
            btn.GetComponent<BoxCollider>().enabled = true;
        } else {
            btn.GetComponent<BoxCollider>().enabled = false;
        }
    }
    /*
	 * Generic toggle function, toggles gameobject to on/off state.
	 * */
    public void ToggleObject(GameObject objectToToggle) {
        objectToToggle.SetActive(!objectToToggle.activeSelf);
    }
    /*
	 * Checker for if the mouse is currently
	 * over a UI Object
	 * */
    public bool IsMouseOnUI() {
        PointerEventData pointer = new PointerEventData(EventSystem.current);
        pointer.position = Input.mousePosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointer, raycastResults);

        if (raycastResults.Count > 0) {
            foreach (var go in raycastResults) {
                if (go.gameObject.layer == LayerMask.NameToLayer("UI")) {
                    //Debug.Log(go.gameObject.name, go.gameObject);
                    return true;
                }

            }
        }
        return false;
            //eventSystem.IsPointerOverGameObject();
        //if (uiCamera != null) {
        //if (Minimap.Instance.isDragging) {
        //    return true;
        //}
        //if (UICamera.hoveredObject != null && (UICamera.hoveredObject.layer == LayerMask.NameToLayer("UI") || UICamera.hoveredObject.layer == LayerMask.NameToLayer("PlayerActions"))) {
        //    return true;
        //}
        //}
        //return false;
    }
    #endregion

    #region Object Pooling
    /*
     * Use this to instantiate UI Objects, so that the program can normalize it's
     * font sizes.
     * */
    internal GameObject InstantiateUIObject(string prefabObjName, Transform parent) {
        //GameObject go = GameObject.Instantiate (prefabObj, parent) as GameObject;
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(prefabObjName, Vector3.zero, Quaternion.identity, parent);
        TextMeshProUGUI[] goLbls = go.GetComponentsInChildren<TextMeshProUGUI>(true);
        for (int i = 0; i < goLbls.Length; i++) {
            NormalizeFontSizeOfLabel(goLbls[i]);
        }
        return go;
    }
    #endregion

    #region For Testing
    public void ToggleBorders() {
        CameraMove.Instance.ToggleMainCameraLayer("Borders");
        CameraMove.Instance.ToggleMainCameraLayer("MinimapAndHextiles");
    }
    public void StartCorruption() {
        if (landmarkInfoUI.currentlyShowingLandmark != null) {
            landmarkInfoUI.currentlyShowingLandmark.tileLocation.SetUncorruptibleLandmarkNeighbors(0);
            landmarkInfoUI.currentlyShowingLandmark.tileLocation.SetCorruption(true, landmarkInfoUI.currentlyShowingLandmark);
        }
    }
    //public void ToggleResourceIcons() {
    //    CameraMove.Instance.ToggleResourceIcons();
    //}
    //public void ToggleGeneralCamera() {
    //    CameraMove.Instance.ToggleGeneralCamera();
    //}
    //public void ToggleTraderCamera() {
    //    CameraMove.Instance.ToggleTraderCamera();
    //}
    #endregion

    private void HideMainUI() {
        mainUIGO.SetActive(false);
    }

    public void ShowMainUI() {
        mainUIGO.SetActive(true);
    }

    #region Landmark Info
    [Space(10)]
    [Header("Landmark Info")]
    [SerializeField]
    internal LandmarkInfoUI landmarkInfoUI;
    public void ShowLandmarkInfo(BaseLandmark landmark) {
        //HideMainUI();
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.HideMenu();
        //}
        //if (monsterInfoUI.isShowing) {
        //    monsterInfoUI.HideMenu();
        //}
        landmarkInfoUI.SetData(landmark);
        landmarkInfoUI.OpenMenu();
        landmark.CenterOnLandmark();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateLandmarkInfo() {
        if (landmarkInfoUI.isShowing) {
            landmarkInfoUI.UpdateLandmarkInfo();
        }
    }
    #endregion

    #region Faction Info
    [Space(10)]
    [Header("Faction Info")]
    [SerializeField]
    internal FactionInfoUI factionInfoUI;
    public void ShowFactionInfo(Faction faction) {
        //HideMainUI();
        //if (landmarkInfoUI.isShowing) {
        //    landmarkInfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.HideMenu();
        //}
        //if (monsterInfoUI.isShowing) {
        //    monsterInfoUI.HideMenu();
        //}
        factionInfoUI.SetData(faction);
        factionInfoUI.OpenMenu();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateFactionInfo() {
        if (factionInfoUI.isShowing) {
            factionInfoUI.UpdateFactionInfo();
        }
    }
    #endregion

    #region Character Info
    [Space(10)]
    [Header("Character Info")]
    [SerializeField] internal CharacterInfoUI characterInfoUI;
    public void ShowCharacterInfo(ECS.Character character) {
        //HideMainUI();
        //if (landmarkInfoUI.isShowing) {
        //    landmarkInfoUI.HideMenu();
        //}
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.HideMenu();
        //}
        //if (monsterInfoUI.isShowing) {
        //    monsterInfoUI.HideMenu();
        //}
        characterInfoUI.SetData(character);
        if(character.role.roleType != CHARACTER_ROLE.PLAYER) {
            characterInfoUI.OpenMenu();
        } else {
            characterInfoUI.HideMenu();
        }
        character.CenterOnCharacter();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateCharacterInfo() {
        if (characterInfoUI.isShowing) {
            characterInfoUI.UpdateCharacterInfo();
        }
    }
    #endregion

    #region HexTile Info
    [Space(10)]
    [Header("HexTile Info")]
    [SerializeField] internal HextileInfoUI hexTileInfoUI;
    public void ShowHexTileInfo(HexTile hexTile) {
        //HideMainUI();
        //if (landmarkInfoUI.isShowing) {
        //    landmarkInfoUI.HideMenu();
        //}
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.HideMenu();
        //}
        //if (monsterInfoUI.isShowing) {
        //    monsterInfoUI.HideMenu();
        //}
        hexTileInfoUI.SetData(hexTile);
        hexTileInfoUI.OpenMenu();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateHexTileInfo() {
        if (hexTileInfoUI.isShowing) {
            hexTileInfoUI.UpdateHexTileInfo();
        }
    }
    #endregion

    #region Party Info
    [Space(10)]
    [Header("Party Info")]
    [SerializeField] internal PartyInfoUI partyinfoUI;
    public void ShowPartyInfo(NewParty party) {
        //HideMainUI();
        //if (landmarkInfoUI.isShowing) {
        //    landmarkInfoUI.HideMenu();
        //}
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //	questInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (monsterInfoUI.isShowing) {
        //    monsterInfoUI.HideMenu();
        //}
        partyinfoUI.SetData(party);
        partyinfoUI.OpenMenu();
    }
    public void UpdatePartyInfo() {
        if (partyinfoUI.isShowing) {
            partyinfoUI.UpdatePartyInfo();
        }
    }
    #endregion

    #region Monster Info
    [Space(10)]
    [Header("Monster Info")]
    [SerializeField]
    internal MonsterInfoUI monsterInfoUI;
    public void ShowMonsterInfo(Monster monster) {
        //HideMainUI();
        //if (landmarkInfoUI.isShowing) {
        //    landmarkInfoUI.HideMenu();
        //}
        //if (factionInfoUI.isShowing) {
        //    factionInfoUI.HideMenu();
        //}
        //if (hexTileInfoUI.isShowing) {
        //    hexTileInfoUI.HideMenu();
        //}
        //if (questInfoUI.isShowing) {
        //    questInfoUI.HideMenu();
        //}
        //if (partyinfoUI.isShowing) {
        //    partyinfoUI.HideMenu();
        //}
        //if (characterInfoUI.isShowing) {
        //    characterInfoUI.HideMenu();
        //}
        monsterInfoUI.SetData(monster);
        monsterInfoUI.OpenMenu();
        //		playerActionsUI.ShowPlayerActionsUI ();
    }
    public void UpdateMonsterInfo() {
        if (monsterInfoUI.isShowing) {
            monsterInfoUI.UpdateMonsterInfo();
        }
    }
    #endregion

    #region Player Actions
    [Space(10)]
    [Header("Player Actions")]
    [SerializeField]
    internal PlayerActionsUI playerActionsUI;
    public void ShowPlayerActions(BaseLandmark landmark) {
        playerActionsUI.SetData(landmark);
        playerActionsUI.OpenMenu();
    }
    public void UpdatePlayerActions() {
        if (playerActionsUI.isShowing) {
            playerActionsUI.UpdatePlayerActions();
        }
    }
    #endregion

    #region Combat Info
    [Space(10)]
    [Header("Combat History")]
    [SerializeField] internal CombatLogsUI combatLogUI;
    public void ShowCombatLog(ECS.Combat combat) {
        //if(questLogUI.isShowing){
        //	questLogUI.HideQuestLogs ();
        //}
        combatLogUI.ShowCombatLogs(combat);
        combatLogUI.UpdateCombatLogs();
    }
    public void UpdateCombatLogs() {
        if (combatLogUI.isShowing) {
            combatLogUI.UpdateCombatLogs();
        }
    }
    #endregion

    #region Menu History
    public void AddMenuToQueue(UIMenu menu, object data) {
        UIMenuSettings latestSetting = _menuHistory.ElementAtOrDefault(0);
        if (latestSetting != null) {
            if (latestSetting.menu == menu && latestSetting.data == data) {
                //the menu settings to be added are the same as the latest one, ignore.
                return;
            }
        }
        _menuHistory.Add(new UIMenuSettings(menu, data));
        //string text = string.Empty;
        //for (int i = 0; i < _menuHistory.Count; i++) {
        //    UIMenuSettings currSetting = _menuHistory.ElementAt(i);
        //    text += currSetting.menu.GetType().ToString();
        //    if(currSetting.data is Faction) {
        //        text += " - Faction " + (currSetting.data as Faction).name;
        //    } else if(currSetting.data is Party) {
        //        text += " - Party " + (currSetting.data as Party).name;
        //    } else if (currSetting.data is HexTile) {
        //        text += " - HexTile " + (currSetting.data as HexTile).name;
        //    } else if (currSetting.data is BaseLandmark) {
        //        text += " - Landmark " + (currSetting.data as BaseLandmark).landmarkName;
        //    } else if (currSetting.data is ECS.Character) {
        //        text += " - Character " + (currSetting.data as ECS.Character).name;
        //    } else if (currSetting.data is OldQuest.Quest) {
        //        text += " - OldQuest.Quest " + (currSetting.data as OldQuest.Quest).questType.ToString();
        //    }
        //    text += "\n";
        //}
        //Debug.Log(text);
    }
    public void ShowPreviousMenu() {
        _menuHistory.RemoveAt(_menuHistory.Count - 1);
        UIMenuSettings menuToShow = _menuHistory.ElementAt(_menuHistory.Count - 1);
        //_menuHistory.Remove(menuToShow);
        menuToShow.menu.ShowMenu();
        menuToShow.menu.SetData(menuToShow.data);
        //string text = string.Empty;
        //for (int i = 0; i < _menuHistory.Count; i++) {
        //    UIMenuSettings currSetting = _menuHistory.ElementAt(i);
        //    text += currSetting.menu.GetType().ToString();
        //    if (currSetting.data is Faction) {
        //        text += " - Faction " + (currSetting.data as Faction).name;
        //    } else if (currSetting.data is Party) {
        //        text += " - Party " + (currSetting.data as Party).name;
        //    } else if (currSetting.data is HexTile) {
        //        text += " - HexTile " + (currSetting.data as HexTile).name;
        //    } else if (currSetting.data is BaseLandmark) {
        //        text += " - Landmark " + (currSetting.data as BaseLandmark).landmarkName;
        //    } else if (currSetting.data is ECS.Character) {
        //        text += " - Character " + (currSetting.data as ECS.Character).name;
        //    } else if (currSetting.data is OldQuest.Quest) {
        //        text += " - OldQuest.Quest " + (currSetting.data as OldQuest.Quest).questType.ToString();
        //    }
        //    text += "\n";
        //}
        //Debug.Log(text);
    }
    public void ClearMenuHistory() {
        _menuHistory.Clear();
    }
    #endregion

    #region Console
    [Space(10)]
    [Header("Console")]
    [SerializeField] internal ConsoleMenu consoleUI;
    public bool IsConsoleShowing() {
        //return false;
        return consoleUI.isShowing;
    }
    public void ToggleConsole() {
        if (consoleUI.isShowing) {
            HideConsole();
        } else {
            ShowConsole();
        }
    }
    public void ShowConsole() {
        consoleUI.ShowConsole();
    }
    public void HideConsole() {
        consoleUI.HideConsole();
    }
    #endregion

    #region Characters Summary
    [Space(10)]
    [Header("Characters Summary")]
    [SerializeField] private GameObject charactersSummaryGO;
    [SerializeField] private Toggle charactersToggleBtn;
    public CharactersSummaryUI charactersSummaryMenu;
    public void ShowCharactersSummary() {
        //HideQuestsSummary();
        //HideStorylinesSummary();
        worldInfoCharactersSelectedGO.SetActive(true);
        charactersSummaryMenu.OpenMenu();
    }
    public void HideCharactersSummary() {
        worldInfoCharactersSelectedGO.SetActive(false);
        charactersSummaryMenu.CloseMenu();
    }
    //public void UpdateCharacterSummary() {
    //    string questSummary = string.Empty;
    //    questSummary += "[b]Available Quests: [/b]";
    //    for (int i = 0; i < QuestManager.Instance.availableQuests.Count; i++) {
    //        Quest currentQuest = QuestManager.Instance.availableQuests[i];
    //        if (!currentQuest.isDone) {
    //            questSummary += "\n" + currentQuest.questURLName;
    //            questSummary += "\n   Characters on Quest: ";
    //            if (currentQuest.acceptedCharacters.Count > 0) {
    //                for (int j = 0; j < currentQuest.acceptedCharacters.Count; j++) {
    //                    ECS.Character currCharacter = currentQuest.acceptedCharacters[j];
    //                    questSummary += "\n" + currCharacter.urlName + " (" + currCharacter.currentQuestPhase.phaseName + ")";
    //                }
    //            } else {
    //                questSummary += "NONE";
    //            }
    //        }
    //    }
    //    questsSummaryLbl.text = questSummary;
    //    questsSummaryLbl.ResizeCollider();
    //}
    public void ToggleCharacterSummary() {
        if (charactersSummaryMenu.isShowing) {
            HideCharactersSummary();
        } else {
            ShowCharactersSummary();
        }
    }
    public void OnCloseCharacterSummary() {
        charactersToggleBtn.isOn = false;
    }
    #endregion

    #region Faction Summary
    [Space(10)]
    [Header("Factions Summary")]
    public FactionSummaryUI factionsSummaryMenu;
    [SerializeField] private Toggle factionsToggleBtn;
    public void ShowFactionsSummary() {
        factionsSummaryMenu.OpenMenu();
    }
    public void HideFactionSummary() {
        factionsSummaryMenu.CloseMenu();
    }
    public void ToggleFactionSummary() {
        if (factionsSummaryMenu.isShowing) {
            HideFactionSummary();
        } else {
            ShowFactionsSummary();
        }
    }
    public void OnCloseFactionSummary() {
        factionsToggleBtn.isOn = false;
    }
    #endregion

    #region Context Menu
    [Space(10)]
    [Header("Context Menu")]
    public GameObject contextMenuPrefab;
    public GameObject contextMenuItemPrefab;
    public UIContextMenu contextMenu;
    private void ShowContextMenu(HexTile tile) {
        if (PlayerManager.Instance.isChoosingStartingTile) {
            //|| landmarkInfoUI.isWaitingForAttackTarget
            return;
        }
        ContextMenuSettings settings = tile.GetContextMenuSettings();
        if (settings.items.Count > 0) {
            contextMenu.LoadSettings(settings);
            contextMenu.gameObject.SetActive(true);
            //Vector2 pos;
            //RectTransformUtility.ScreenPointToLocalPointInRectangle(this.transform as RectTransform, Input.mousePosition, eventSystem.camera, out pos);
            contextMenu.transform.position = Input.mousePosition;
        }
        
    }
    public void HideContextMenu() {
        contextMenu.gameObject.SetActive(false);
    }
    public void HideContextMenu(HexTile tile) {
        HideContextMenu();
    }
    #endregion

    #region Save
    public void Save() {
        //Save savefile = new Save();
        //savefile.hextiles = new List<HextileSave>();
        //for (int i = 0; i < GridMap.Instance.hexTiles.Count; i++) {
        //    if(GridMap.Instance.hexTiles[i].landmarkOnTile != null) {
        //        HextileSave hextileSave = new HextileSave();
        //        hextileSave.SaveTile(GridMap.Instance.hexTiles[i]);
        //        savefile.hextiles.Add(hextileSave);
        //    }
        //}
        //SaveGame.Save<Save>("SavedFile1", savefile);
        //LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
    #endregion

    #region Quest Summary
    [Space(10)]
    [Header("Quest Summary")]
    [SerializeField] TextMeshProUGUI questSummaryLbl;
    public void UpdateQuestSummary() {
        string questSummary = string.Empty;
        foreach (KeyValuePair<QUEST_TYPE, List<Quest>> kvp in QuestManager.Instance.availableQuests) {
            for (int i = 0; i < kvp.Value.Count; i++) {
                Quest currQuest = kvp.Value[i];
                questSummary += "<b>" + currQuest.name + "</b>\n";
                if (currQuest is BuildStructureQuest) {
                    List<Resource> neededResources = (currQuest as BuildStructureQuest).GetNeededResources();
                    questSummary += "Needed Resources: ";
                    neededResources.ForEach(x => questSummary += x.resource.ToString() + " - " + x.amount.ToString() + "\n");
                }
                List<ECS.Character> characters = currQuest.GetAcceptedCharacters();
                for (int j = 0; j < characters.Count; j++) {
                    questSummary += "       " + characters[j].urlName + "\n";
                }
            }
           
        }
        questSummaryLbl.text = questSummary;
    }
    #endregion
}

[System.Serializable]
public class UnifiedUISettings {
    [Header("Frame Settings")]
    public Color bgColor;
    public Color outlineColor;
    public Color innerHeaderColor;
    public Color outerHeaderColor;
    public float outlineThickness;
    public float headerHeight;
    public float closeBtnSize;

    [Header("ScrollView Settings")]
    public ScrollRect.MovementType scrollMovementType;
    public float scrollSensitivity;
    public ScrollRect.ScrollbarVisibility scrollbarVisibility;

    [Header("ScrollView Element Settings")]
    public Color evenColor;
    public Color oddColor;

    [Header("Selectable Settings")]
    public Sprite hoverOverSprite;
    public Sprite hoverOutSprite;
    public Color hoverOverTextColor;
    public Color hoverOutTextColor;
    public Color toggleOnTextColor = new Color(73f/255f, 93f/255f, 107f/255f, 255f/255f);
    public Color toggleOffTextColor = new Color(247f/255f, 238f/255f, 212f/255f, 255f/255f);

}