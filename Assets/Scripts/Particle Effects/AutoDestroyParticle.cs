﻿using EZObjectPools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDestroyParticle : PooledObject {

    [SerializeField] private ParticleSystem[] particleSystems;

    private void Update() {
        bool allInactive = true;
        for (int i = 0; i < particleSystems.Length; i++) {
            ParticleSystem currPS = particleSystems[i];
            if (currPS.IsAlive()) {
                allInactive = false;
                break;
            }
        }
        if (allInactive) {
            ObjectPoolManager.Instance.DestroyObject(this);
        }
    }

    public override void Reset() {
        base.Reset();
        for (int i = 0; i < particleSystems.Length; i++) {
            particleSystems[i].Clear();
        }
    }

}
