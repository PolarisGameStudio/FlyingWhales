﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BezierCurve : MonoBehaviour {
    public LineRenderer bgLineRenderer;
    public LineRenderer progressMeter;

    private int _progressAmount;
    private Vector3[] _positions;

    #region getters/setters
    public bool isDone {
        get { return progressMeter.positionCount >= _positions.Length; }
    }
    public bool isNoMorePositions {
        get { return progressMeter.positionCount <= 1; }
    }
    #endregion

    private void Start() {
        progressMeter.positionCount = 0;
    }

    public void SetPositions(Vector3[] positions) {
        _positions = positions;
        bgLineRenderer.positionCount = _positions.Length;
        bgLineRenderer.SetPositions(_positions);
    }
    public void SetProgressAmount(int amount) {
        _progressAmount = amount;
    }
    //Returns true if progress is complete
    public IEnumerator AddProgress() {
        Vector3 prevPos = _positions[0];
        for (int i = 0; i < _progressAmount; i++) {
            if(progressMeter.positionCount > 0) {
                prevPos = _positions[progressMeter.positionCount - 1];
            }
            progressMeter.positionCount++;
            float secs = GameManager.Instance.progressionSpeed / (float) _progressAmount;
            progressMeter.SetPosition(progressMeter.positionCount - 1, prevPos);
            iTween.ValueTo(gameObject, iTween.Hash("from", prevPos, "to", _positions[progressMeter.positionCount - 1], "time", secs, "onupdate", "TraverseLineRenderer"));
            yield return new WaitForSeconds(secs);
        }
    }
    private void TraverseLineRenderer(Vector3 vector3) {
        progressMeter.SetPosition(progressMeter.positionCount - 1, vector3);
    }
    //Returns true when progress reaches 0
    public IEnumerator ReduceProgress() {
        for (int i = 0; i < _progressAmount; i++) {
            //if (progressMeter.positionCount > 0) {
            //    prevPos = _positions[progressMeter.positionCount - 1];
            //}
            if (progressMeter.positionCount <= 1) {
                break;
            } else {
                Vector3 currentPos = progressMeter.GetPosition(progressMeter.positionCount - 1);
                progressMeter.positionCount--;
                float secs = GameManager.Instance.progressionSpeed / (float) _progressAmount;
                progressMeter.SetPosition(progressMeter.positionCount - 1, currentPos);
                iTween.ValueTo(gameObject, iTween.Hash("from", currentPos, "to", _positions[progressMeter.positionCount - 1], "time", secs, "onupdate", "TraverseLineRenderer"));
                yield return new WaitForSeconds(secs);
            }

        }
        //progressMeter.positionCount -= _progressAmount;
        //if(progressMeter.positionCount <= 0) {
        //    return true;
        //}
        //return false;
    }
}
