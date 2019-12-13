﻿using UnityEngine;
using System.Collections;
using Inner_Maps;
using UnityEngine.EventSystems;

public class AreaMapCameraMove : MonoBehaviour {

	public static AreaMapCameraMove Instance = null;

	[SerializeField] private float _minFov;
	[SerializeField] private float _maxFov;
	[SerializeField] private float zoomSensitivity;
    [SerializeField] private float _zoomSpeed = 5f;
    public Camera areaMapsCamera;

    private float dampTime = 0.2f;
	private Vector3 velocity = Vector3.zero;
	[SerializeField] private Transform _target;

	private const float MIN_Z = -10f;
    private const float MAX_Z = -10f;
    [SerializeField] private float MIN_X;
    [SerializeField] private float MAX_X;
    [SerializeField] private float MIN_Y;
    [SerializeField] private float MAX_Y;

    [SerializeField] private bool allowZoom = true;

    [Header("Dragging")]
    private float dragThreshold = 0.1f;
    private float currDragTime;
    private Vector3 dragOrigin;
    private bool isDragging = false;

    [Header("Edging")]
    [SerializeField] private float edgingSpeed = 30f;
    private bool allowEdgePanning = false;

    private float previousCameraFOV;

    [SerializeField] private bool cameraControlEnabled = false;
    [SerializeField] private float xSeeLimit;

    #region getters/setters
    public Transform target {
        get { return _target; }
        private set {
            _target = value;
            if (_target == null) {
                Messenger.RemoveListener<GameObject>(Signals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
            } else {
                Messenger.AddListener<GameObject>(Signals.POOLED_OBJECT_DESTROYED, OnPooledObjectDestroyed);
            }
        }
    }
    public float currentFOV {
        get { return areaMapsCamera.orthographicSize; }
    }
    public float maxFOV {
        get { return _maxFov; }
    }
    #endregion

    private void Awake(){
		Instance = this;
	}
    private void Update() {
        if (!cameraControlEnabled) {
            return;
        }
        ArrowKeysMovement();
        Dragging();
        Edging();
        //Zooming();
        Targetting();
        ConstrainCameraBounds();
    }

    public void Initialize() {
        //SetInitialCameraPosition();
        gameObject.SetActive(false);
        Messenger.AddListener<Area>(Signals.AREA_MAP_OPENED, OnAreaMapOpened);
        Messenger.AddListener<Area>(Signals.AREA_MAP_CLOSED, OnAreaMapClosed);
        
    }

    #region Listeners
    private void OnAreaMapOpened(Area area) {
        gameObject.SetActive(true);
        SetCameraControlState(true);
        SetCameraBordersForMap(area.areaMap);
        ConstrainCameraBounds();
    }
    private void OnAreaMapClosed(Area area) {
        gameObject.SetActive(false);
        SetCameraControlState(false);
    }
    private void OnPooledObjectDestroyed(GameObject obj) {
        if (target == obj.transform) {
            target = null;
        }
    }
    #endregion

    #region Utilities
    public void ZoomToTarget(float targetZoom) {
        StartCoroutine(lerpFieldOfView(areaMapsCamera, targetZoom, 0.1f));
    }
    private IEnumerator lerpFieldOfView(Camera targetCamera, float toFOV, float duration) {
        float counter = 0;

        float fromFOV = targetCamera.orthographicSize;

        while (counter < duration) {
            counter += Time.deltaTime;

            float fOVTime = counter / duration;
            //Debug.Log(fOVTime);

            //Change FOV
            targetCamera.orthographicSize = Mathf.Lerp(fromFOV, toFOV, fOVTime);
            //Wait for a frame
            yield return null;
        }
    }
    #endregion

    #region Positioning
    private void SetInitialCameraPosition() {
        Vector3 initialPos = new Vector3(-200f, 0f, -10f);
        this.transform.position = initialPos;
    }
    public void MoveCamera(Vector3 newPos) {
        //Vector3 point = areaMapsCamera.WorldToViewportPoint(newPos);
        //Vector3 delta = newPos - areaMapsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
        //Vector3 destination = transform.position + delta;
        transform.position = newPos;
        //ConstrainCameraBounds();
    }
    public void JustCenterCamera(bool instantCenter) {
        if (instantCenter) {
            Vector3 center = new Vector3((MIN_X + MAX_X) * 0.5f, (MIN_Y + MAX_Y) * 0.5f);
            MoveCamera(center);
        } else {
            InnerMapManager.Instance.currentlyShowingMap.centerGo.transform.position = new Vector3((MIN_X + MAX_X) * 0.5f, (MIN_Y + MAX_Y) * 0.5f);
            target = InnerMapManager.Instance.currentlyShowingMap.centerGo.transform;
        }
    }
    public void CenterCameraOn(GameObject GO, bool instantCenter = false) {
        if (GO == null) {
            target = null;
        } else {
            if (instantCenter) {
                MoveCamera(GO.transform.position);
            } 
            //else {
                target = GO.transform;
            //}
        }
    }
    private void ArrowKeysMovement() {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.DownArrow) ||Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S)) {
            if (!UIManager.Instance.IsConsoleShowing() && !UIManager.Instance.IsMouseOnInput()) {
                float zAxisValue = Input.GetAxis("Vertical");
                iTween.MoveUpdate(areaMapsCamera.gameObject, iTween.Hash("y", areaMapsCamera.transform.position.y + zAxisValue, "time", 0.1f));
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)) {
            if (!UIManager.Instance.IsConsoleShowing() && !UIManager.Instance.IsMouseOnInput()) {
                float xAxisValue = Input.GetAxis("Horizontal");
                iTween.MoveUpdate(areaMapsCamera.gameObject, iTween.Hash("x", areaMapsCamera.transform.position.x + xAxisValue, "time", 0.1f));
            }
        }
    }
    private void Zooming() {
        Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
        if (allowZoom && screenRect.Contains(Input.mousePosition)) {
            //camera scrolling code
            float fov = areaMapsCamera.orthographicSize;
            float adjustment = Input.GetAxis("Mouse ScrollWheel") * (zoomSensitivity);
            if (adjustment != 0f && !UIManager.Instance.IsMouseOnUI()) {
                //Debug.Log(adjustment);
                fov -= adjustment;
                //fov = Mathf.Round(fov * 100f) / 100f;
                fov = Mathf.Clamp(fov, _minFov, _maxFov);

                if (!Mathf.Approximately(previousCameraFOV, fov)) {
                    previousCameraFOV = fov;
                    areaMapsCamera.orthographicSize = Mathf.Lerp(areaMapsCamera.orthographicSize, fov, Time.deltaTime * _zoomSpeed);
                } else {
                    areaMapsCamera.orthographicSize = fov;
                }
                CalculateCameraBounds();
            }
        }
    }
    private void Targetting() {
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D) || isDragging) {
            //reset target when player pushes a button to pan the camera
            //if(target != null) {
            //    Messenger.Broadcast(Signals.CAMERA_OUT_OF_FOCUS);
            //}
            target = null;
        }

        if (target) { //smooth camera center
            Vector3 point = areaMapsCamera.WorldToViewportPoint(target.position);
            Vector3 delta = target.position - areaMapsCamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, point.z)); //(new Vector3(0.5, 0.5, point.z));
            Vector3 destination = transform.position + delta;
            if (transform.position != destination) {
                transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity, dampTime);
            }
            //if (HasReachedBounds() || (Mathf.Approximately(transform.position.x, destination.x) && Mathf.Approximately(transform.position.y, destination.y))) {
            //    target = null;
            //}
        }
    }
    private bool startedOnUI = false;
    private bool hasReachedThreshold = false;
    private Vector3 originMousePos;
    private void Dragging() {
        if (startedOnUI) {
            if (!Input.GetMouseButton(2)) {
                ResetDragValues();
            }
            return;
        }
        if (!isDragging) {
            if (Input.GetMouseButtonDown(2)) {
                if (UIManager.Instance.IsMouseOnUI() || InnerMapManager.Instance.currentlyHoveredPoi != null) { //if the dragging started on UI, a tileobject or a character, do not allow drag
                    startedOnUI = true;
                    return;
                }
                //dragOrigin = Input.mousePosition; //on first press of mouse
            } else if (Input.GetMouseButton(2)) {
                currDragTime += Time.deltaTime; //while the left mouse button is pressed
                if (currDragTime >= dragThreshold) {
                    if (!hasReachedThreshold) {
                        dragOrigin = areaMapsCamera.ScreenToWorldPoint(Input.mousePosition);
                        originMousePos = Input.mousePosition;
                        hasReachedThreshold = true;
                    }
                    if (originMousePos !=  Input.mousePosition) { //check if the mouse has moved position from the origin, only then will it be considered dragging
                        CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Drag_Clicked);
                        isDragging = true;
                    }
                }
            }
            
        }

        if (isDragging) {
            Vector3 difference = (areaMapsCamera.ScreenToWorldPoint(Input.mousePosition))- areaMapsCamera.transform.position;
            areaMapsCamera.transform.position = dragOrigin-difference;
            if (Input.GetMouseButtonUp(2)) {
                ResetDragValues();
                CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
            }
        } else {
            if (!Input.GetMouseButton(2)) {
                currDragTime = 0f;
                hasReachedThreshold = false;
            }
        }
    }
    private void ResetDragValues() {
        //CursorManager.Instance.SetCursorTo(CursorManager.Cursor_Type.Default);
        currDragTime = 0f;
        isDragging = false;
        startedOnUI = false;
        hasReachedThreshold = false;
    }
    private void SetCameraBordersForMap(AreaInnerTileMap map) {
        float y = map.transform.localPosition.y;
        //MIN_Y = y;
        //MAX_Y = y;

        MIN_X = map.cameraBounds.x;
        MIN_Y = y + map.cameraBounds.y; //need to offset y values based on position of map, because maps are ordered vertically
        MAX_X = map.cameraBounds.z;
        MAX_Y = y + map.cameraBounds.w;

    }
    private void Edging() {
        if (!allowEdgePanning || isDragging) {
            return;
        }
        bool isEdging = false;
        Vector3 newPos = transform.position;
        if (Input.mousePosition.x > Screen.width) {
            newPos.x += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.x < 0) {
            newPos.x -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }

        if (Input.mousePosition.y > Screen.height) {
            newPos.y += edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (Input.mousePosition.y < 0) {
            newPos.y -= edgingSpeed * Time.deltaTime;
            isEdging = true;
        }
        if (isEdging) {
            target = null; //reset target
        }
        transform.position = newPos;
    }
    public void AllowEdgePanning(bool state) {
        allowEdgePanning = state;
    }
    #endregion

    #region Bounds
    public void CalculateCameraBounds() {
        if (InnerMapManager.Instance.currentlyShowingMap == null) {
            return;
        }
        Vector2 topRightCornerCoordinates = InnerMapManager.Instance.currentlyShowingMap.map
            [InnerMapManager.Instance.currentlyShowingMap.width - 1, InnerMapManager.Instance.currentlyShowingMap.height - 1].localLocation;
        //LocationGridTile leftMostTile = InteriorMapManager.Instance.currentlyShowingMap.map[0, InteriorMapManager.Instance.currentlyShowingMap.height / 2];
        LocationGridTile rightMostTile = InnerMapManager.Instance.currentlyShowingMap.map[InnerMapManager.Instance.currentlyShowingMap.width - 1, InnerMapManager.Instance.currentlyShowingMap.height / 2];
        LocationGridTile topMostTile = InnerMapManager.Instance.currentlyShowingMap.map[InnerMapManager.Instance.currentlyShowingMap.width/2, InnerMapManager.Instance.currentlyShowingMap.height - 1];
        //LocationGridTile botMostTile = InteriorMapManager.Instance.currentlyShowingMap.map[InteriorMapManager.Instance.currentlyShowingMap.width/2, 0];

        //float mapX = Mathf.Floor(topRightCornerCoordinates.x);
        //float mapY = Mathf.Floor(topRightCornerCoordinates.y);

        float vertExtent = areaMapsCamera.orthographicSize;
        float horzExtent = vertExtent * Screen.width / Screen.height;

        Bounds newBounds = new Bounds();
        newBounds.extents = new Vector3(Mathf.Abs(rightMostTile.worldLocation.x), Mathf.Abs(topMostTile.worldLocation.y), 0f);
        SetCameraBounds(newBounds, horzExtent, vertExtent);
    }
    public void ConstrainCameraBounds() {
        float xLowerBound = MIN_X;
        float xUpperBound = MAX_X;
        float yLowerBound = MIN_Y;
        float yUpperBound = MAX_Y;
        if (MAX_X < MIN_X) {
            xLowerBound = MAX_X;
            xUpperBound = MIN_X;
        }
        if (MAX_Y < MIN_Y) {
            yLowerBound = MAX_Y;
            yUpperBound = MIN_Y;
        }
        float xCoord = Mathf.Clamp(transform.position.x, xLowerBound, xUpperBound);
        float yCoord = Mathf.Clamp(transform.position.y, yLowerBound, yUpperBound);
        float zCoord = Mathf.Clamp(transform.position.z, MIN_Z, MAX_Z);
        areaMapsCamera.transform.position = new Vector3(
            xCoord,
            yCoord,
            zCoord);
    }
    private bool HasReachedBounds() {
        if ((Mathf.Approximately(transform.position.x, MAX_X) || Mathf.Approximately(transform.position.x, MIN_X)) &&
                (Mathf.Approximately(transform.position.y, MAX_Y) || Mathf.Approximately(transform.position.y, MIN_Y))) {
            return true;
        }
        return false;
    }
    private bool IsWithinBounds(float value, float lowerBound, float upperBound) {
        if (value >= lowerBound && value <= upperBound) {
            return true;
        }
        return false;
    }
    private void SetCameraBounds(Bounds bounds, float horzExtent, float vertExtent) {
        float halfOfTile = (64f / 2f) / 100f; //1.28
        MIN_X = bounds.min.x + horzExtent - (halfOfTile);
        MAX_X = bounds.max.x - horzExtent + (halfOfTile);
        MIN_Y = bounds.min.y + vertExtent - (halfOfTile);
        MAX_Y = bounds.max.y - vertExtent + (halfOfTile);
    }
    private Vector2[] GetCameraWorldCorners(Camera camera) {
        Vector2[] corners = new Vector2[4]; //4 corners

        // Screens coordinate corner location
        var upperLeftScreen = new Vector2(0, Screen.height);
        var upperRightScreen = new Vector2(Screen.width, Screen.height);
        var lowerLeftScreen = new Vector2(0, 0);
        var lowerRightScreen = new Vector2(Screen.width, 0);

        //Corner locations in world coordinates
        var upperLeft = camera.ScreenToWorldPoint(upperLeftScreen);
        var upperRight = camera.ScreenToWorldPoint(upperRightScreen);
        var lowerRight = camera.ScreenToWorldPoint(lowerRightScreen);
        var lowerLeft = camera.ScreenToWorldPoint(lowerLeftScreen);

        corners[0] = upperLeft;
        corners[1] = upperRight;
        corners[2] = lowerRight;
        corners[3] = lowerLeft;

        return corners;
    }
    public bool CanSee(GameObject go) {
        Vector3 viewPos = areaMapsCamera.WorldToViewportPoint(go.transform.position);
        return viewPos.x >= 0 && viewPos.x <= xSeeLimit && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z >= 0;
    }
    #endregion

    #region Camera Control
    public void SetCameraControlState(bool state) {
        cameraControlEnabled = state;
    }
    #endregion
}
