﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColliderUtility : MonoBehaviour {

    private new Collider2D collider;

    private void Awake() {
        collider = this.GetComponent<Collider2D>();
    }

    [ContextMenu("Get Contact Points")]
    public void GetContactPoints() {
        Collider2D[] allCollisions = new Collider2D[100];
        int collisionsCount = collider.GetContacts(allCollisions);
        for (int i = 0; i < collisionsCount; i++) {
            Debug.Log(allCollisions[i].name);
        }
    }

    [ContextMenu("Get Collisions")]
    public void GetCollisions() {
        Collider2D[] collisions = new Collider2D[100];
        ContactFilter2D contactFilter = new ContactFilter2D();
        //contactFilter.SetLayerMask(LayerMask.GetMask("Hextiles"));
        int numOfCollisions = collider.OverlapCollider(contactFilter.NoFilter(), collisions);
        for (int i = 0; i < numOfCollisions; i++) {
            Debug.Log(collisions[i].name);
        }
    }
}
