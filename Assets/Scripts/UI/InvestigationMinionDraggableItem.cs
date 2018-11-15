﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InvestigationMinionDraggableItem : DraggableItem {

    [SerializeField] private CharacterPortrait _portrait;
    [SerializeField] private int _index;

    public override void OnBeginDrag(PointerEventData eventData) {
        if (!_isDraggable) {
            return;
        }
        if (!_portrait.gameObject.activeSelf) {
            return;
        }
        GameObject clone = (GameObject) Instantiate(_portrait.gameObject);
        clone.GetComponent<CharacterPortrait>().SetBGState(true);
        _draggingObject = clone.GetComponent<RectTransform>();

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = _portrait.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }

    public override void OnEndDrag(PointerEventData eventData) {
        _isDragging = false;

        if (_draggingObject != null) {
            List<RaycastResult> newRaycastResults = new List<RaycastResult>();
            CustomDropZone customDropzone = null;
            EventSystem.current.RaycastAll(eventData, newRaycastResults);
            for (int i = 0; i < newRaycastResults.Count; i++) {
                customDropzone = newRaycastResults[i].gameObject.GetComponent<CustomDropZone>();
                if (customDropzone != null) {
                    break;
                }
            }
            Destroy(_draggingObject.gameObject);
            if (customDropzone == null) {
                _portrait.gameObject.SetActive(false);
                if(_index == -1) {
                    UIManager.Instance.landmarkInfoUI.AssignMinionToInvestigate(null);
                } else {
                    UIManager.Instance.landmarkInfoUI.AssignPartyMinionToInvestigate(null, _index);
                }
            }
        }
    }
}
