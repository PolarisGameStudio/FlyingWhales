﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UIHoverHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

    private bool isHovering;

    [SerializeField] private UnityEvent onHoverOverAction;
    [SerializeField] private UnityEvent onHoverExitAction;

    public void OnPointerEnter(PointerEventData eventData) {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovering = false;
        if (onHoverExitAction != null) {
            onHoverExitAction.Invoke();
        }
    }

    private void Update() {
        if (isHovering) {
            if (onHoverOverAction != null) {
                onHoverOverAction.Invoke();
            }
        }
    }
}
