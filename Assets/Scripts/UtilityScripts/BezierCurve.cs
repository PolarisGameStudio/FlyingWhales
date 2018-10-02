﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class BezierCurve : MonoBehaviour {

    public int numOfInterpolations;
    public LineRenderer lineRenderer;
    public Transform startPoint;
    public Transform endPoint;
    public Transform controlPoint1;
    public Transform controlPoint2;

    private void Update() {
        DrawCubicCurve(startPoint.position, endPoint.position, numOfInterpolations);
    }

    public void DrawCubicCurve(Vector3 startPoint, Vector3 endPoint, int numOfInterpolations) {
        Vector3 dir = endPoint - startPoint;

        if (dir == Vector3.zero) {
            return;
        }
        lineRenderer.positionCount = numOfInterpolations;

        Vector3 normal = Vector3.Cross(Vector3.up, dir);
        Vector3 normalUp = Vector3.Cross(dir, normal);

        normalUp = normalUp.normalized;
        float multiplier =  1f - (Vector3.Distance(startPoint, endPoint) * 0.1f);
        multiplier = Mathf.Clamp(multiplier, 0.5f, 1f);
        //normalUp *= dir.magnitude * 1f;
       
        //Vector3 controlPoint1 = new Vector3(normalUp.x + 0.5f, normalUp.y, normalUp.z);
        //Vector3 controlPoint2 = new Vector3(normalUp.x - 0.5f, normalUp.y, normalUp.z);
        Vector3 curveSharpness = normalUp * dir.magnitude * multiplier;

        Vector3 p1c = startPoint + curveSharpness;
        Vector3 p2c = endPoint + curveSharpness;
        controlPoint1.position = p1c;
        controlPoint2.position = p2c;


        float timeDivisor = (float) numOfInterpolations;
        float t = 0;
        for (int i = 1; i <= numOfInterpolations; i++) {
            t = i / timeDivisor;
            Vector3 curvePoint = AstarSplines.CubicBezier(startPoint, p1c, p2c, endPoint, t);
            lineRenderer.SetPosition(i - 1, curvePoint);
        }
    }
    public void DrawCubicCurve(Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint1, Vector3 controlPoint2, int numOfInterpolations) {
        lineRenderer.positionCount = numOfInterpolations;

        float timeDivisor = (float) numOfInterpolations;
        float t = 0;
        for (int i = 1; i <= numOfInterpolations; i++) {
            t = i / timeDivisor;
            Vector3 curvePoint = AstarSplines.CubicBezier(startPoint, controlPoint1, controlPoint2, endPoint, t);
            lineRenderer.SetPosition(i - 1, curvePoint);
        }
    }

    [ContextMenu("Get Variables")]
    public void GetVariables() {
        Vector3 dir = endPoint.position - startPoint.position;

        Vector3 normal = Vector3.Cross(Vector3.up, dir);
        Vector3 normalUp = Vector3.Cross(dir, normal);

        normalUp = normalUp.normalized;
        normalUp *= dir.magnitude * 0.1f;


        Debug.LogWarning("Distance: " + Vector3.Distance(startPoint.position, endPoint.position));
        Debug.LogWarning("Dir: " + dir);
        Debug.LogWarning("Up: " + Vector3.up);
        Debug.LogWarning("Normal: " + normal);
        Debug.LogWarning("NormalUp: " + normalUp);
    }
}
