﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using Traits;
using Inner_Maps;

public class CombatManager : MonoBehaviour {
    public static CombatManager Instance = null;

    public const int pursueDuration = 10;

    private void Awake() {
        Instance = this;
    }

    public void ApplyElementalDamage(ELEMENTAL_TYPE elementalType, ITraitable target, Character responsibleCharacter = null) { //, bool shouldSetBurningSource = true
        ElementalDamageData elementalDamage = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
        if (!string.IsNullOrEmpty(elementalDamage.addedTraitName)) {
            //Trait trait = null;
            target.traitContainer.AddTrait(target, elementalDamage.addedTraitName, responsibleCharacter); //, out trait
            //if (shouldSetBurningSource && elementalDamage.addedTraitName == "Burning" && trait != null) {
            //    if(target.gridTileLocation != null) {
            //        Burning burning = trait as Burning;
            //        if(burning.sourceOfBurning == null) {
            //            burning.SetSourceOfBurning(new BurningSource(target.gridTileLocation.structure.location), target);
            //        }
            //    }
            //}
        }
    }
    public void CreateHitEffectAt(IDamageable poi, ELEMENTAL_TYPE elementalType) {
        ElementalDamageData elementalData = ScriptableObjectsManager.Instance.GetElementalDamageData(elementalType);
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(elementalData.hitEffectPrefab.name, Vector3.zero, Quaternion.identity, poi.gridTileLocation.parentMap.objectsParent);
        go.transform.position = poi.projectileReceiver.transform.position;
        go.SetActive(true);

    }
    #region Explosion
    public void PoisonExplosion(IPointOfInterest target, int stacks) {
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = target.gridTileLocation.GetTilesInRadius(2);
        float damagePercentage = 0.1f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
            GameManager.Instance.CreateExplodeEffectAt(traitable.gridTileLocation);
            traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Fire);
            Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>();
            if (burningTrait != null && burningTrait.sourceOfBurning == null) {
                if (bs == null) {
                    bs = new BurningSource(InnerMapManager.Instance.currentlyShowingLocation);
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
            }
        }

        Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Poison Explosion", "effect");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        log.AddLogToInvolvedObjects();
    }
    public void FrozenExplosion(IPointOfInterest target, int stacks) {
        List<ITraitable> traitables = new List<ITraitable>();
        List<LocationGridTile> affectedTiles = target.gridTileLocation.GetTilesInRadius(2);
        float damagePercentage = 0.2f * stacks;
        if (damagePercentage > 1) {
            damagePercentage = 1;
        }
        for (int i = 0; i < affectedTiles.Count; i++) {
            LocationGridTile tile = affectedTiles[i];
            traitables.AddRange(tile.GetTraitablesOnTile());
        }
        // flammables = flammables.Where(x => !x.traitContainer.HasTrait("Burning", "Burnt", "Wet", "Fireproof")).ToList();
        BurningSource bs = null;
        for (int i = 0; i < traitables.Count; i++) {
            ITraitable traitable = traitables[i];
            int damage = Mathf.RoundToInt(traitable.maxHP * damagePercentage);
            GameManager.Instance.CreateExplodeEffectAt(traitable.gridTileLocation);
            traitable.AdjustHP(-damage, ELEMENTAL_TYPE.Water);
            Burning burningTrait = traitable.traitContainer.GetNormalTrait<Burning>();
            if (burningTrait != null && burningTrait.sourceOfBurning == null) {
                if (bs == null) {
                    bs = new BurningSource(InnerMapManager.Instance.currentlyShowingLocation);
                }
                burningTrait.SetSourceOfBurning(bs, traitable);
            }
        }

        Log log = new Log(GameManager.Instance.Today(), "Interrupt", "Frozen Explosion", "effect");
        log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
        PlayerManager.Instance.player.ShowNotificationFromPlayer(log);
        log.AddLogToInvolvedObjects();
    }
    #endregion
}