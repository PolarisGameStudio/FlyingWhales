﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapVisualFactory {
    private static readonly string Tile_Object_Prefab_Name = "TileObjectGameObject";
    
    public GameObject CreateNewMeteorObject() {
        GameObject obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("MeteorVisualObject", Vector3.zero, Quaternion.identity);
        return obj;
    }

    public GameObject CreateNewTileObjectMapVisual(TILE_OBJECT_TYPE objType) {
        GameObject obj;
        switch (objType) {
            case TILE_OBJECT_TYPE.TORNADO:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TornadoVisualObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.RAVENOUS_SPIRIT:
            case TILE_OBJECT_TYPE.FEEBLE_SPIRIT:
            case TILE_OBJECT_TYPE.FORLORN_SPIRIT:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("SpiritGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.POISON_CLOUD:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("PoisonCloudMapObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.LOCUST_SWARM:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("LocustSwarmMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.TORCH:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("TorchGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BED:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BedGameObject", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.BALL_LIGHTNING:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("BallLightningMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.FROSTY_FOG:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("FrostyFogMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.VAPOR:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("VaporMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            case TILE_OBJECT_TYPE.FIRE_BALL:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool("FireBallMapObjectVisual", Vector3.zero, Quaternion.identity);
                break;
            default:
                obj = ObjectPoolManager.Instance.InstantiateObjectFromPool(Tile_Object_Prefab_Name, Vector3.zero, Quaternion.identity);
                break;
        }
        return obj;
    }
}
