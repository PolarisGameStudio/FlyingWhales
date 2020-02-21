﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Traits;
using Inner_Maps;

public class MeteorVisual : MonoBehaviour {
    public ParticleSystem meteorParticle;
    public ParticleSystem[] meteorExplosionParticles;

    private LocationGridTile targetTile;
    private int radius;
    public void MeteorStrike(LocationGridTile targetTile, int radius) {
        this.targetTile = targetTile;
        this.radius = radius;
        meteorParticle.Play();
    }
    public void OnMeteorFell() {
        for (int i = 0; i < meteorExplosionParticles.Length; i++) {
            meteorExplosionParticles[i].Play();
        }
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(radius, 0, true);
        for (int i = 0; i < tiles.Count; i++) {
            LocationGridTile tile = tiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            if (traitable is TileObject obj) {
                GameManager.Instance.CreateFireEffectAt(obj.gridTileLocation);
                if (obj.tileObjectType != TILE_OBJECT_TYPE.GENERIC_TILE_OBJECT) {
                    obj.AdjustHP(-obj.currentHP, ELEMENTAL_TYPE.Fire);
                    if (obj.gridTileLocation == null) {
                        continue; //object was destroyed, do not add burning trait
                    }
                } else {
                    obj.AdjustHP(0, ELEMENTAL_TYPE.Fire);
                }
            } else if (traitable is Character character) {
                GameManager.Instance.CreateFireEffectAt(character.gridTileLocation);
                character.AdjustHP(-(int)(character.maxHP * 0.4f), ELEMENTAL_TYPE.Fire, true);
            } else {
                traitable.AdjustHP(-traitable.currentHP, ELEMENTAL_TYPE.Fire);
            }
            Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>();
            if(burningTrait != null && burningTrait.sourceOfBurning == null) {
                if(bs == null) {
                    bs = new BurningSource(traitable.gridTileLocation.parentMap.location);
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
            }
            //if (traitable.currentHP > 0 && Random.Range(0, 100) < 60) {
            //    if (traitable.traitContainer.HasTrait("Flammable") &&
            //        !traitable.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")) {
            //        Burning burning = new Burning();
            //        burning.SetSourceOfBurning(bs, traitable);
            //        traitable.traitContainer.AddTrait(traitable, burning);
            //    }
            //}
        }
        InnerMapCameraMove.Instance.ShakeCamera();
        GameManager.Instance.StartCoroutine(ExpireCoroutine(gameObject));
    }
    private IEnumerator ExpireCoroutine(GameObject go) {
        yield return new WaitForSeconds(2f);
        ObjectPoolManager.Instance.DestroyObject(go);
    }
}
