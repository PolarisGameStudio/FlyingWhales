﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class LocationDraggable : DraggableItem {

    #region Overrides
    public override void OnBeginDrag(PointerEventData eventData) {
        base.OnBeginDrag(eventData);
        if (!_isDraggable) {
            return;
        }

        AreaEmblem emblem = gameObject.GetComponent<LocationIntelItem>().emblem;
        GameObject clone = (GameObject)Instantiate(emblem.gameObject);
        _draggingObject = clone.GetComponent<RectTransform>();

        //Put _dragging object into the dragging area
        _draggingObject.sizeDelta = emblem.gameObject.GetComponent<RectTransform>().rect.size;
        _draggingObject.SetParent(UIManager.Instance.gameObject.GetComponent<RectTransform>(), true);
        _isDragging = true;
    }
    #endregion
}
