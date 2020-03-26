﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using UnityEngine;

public class Tornado : PlayerSpell {

    private int radius;
    private int durationInTicks;

    public Tornado() : base(SPELL_TYPE.TORNADO) {
        SetDefaultCooldownTime(24);
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
        radius = 1;
        tier = 1;
        durationInTicks = GameManager.Instance.GetTicksBasedOnHour(2);
    }

    #region Overrides
    public override void ActivateAction(LocationGridTile targetTile) {
        base.ActivateAction(targetTile);
        TornadoTileObject tornadoTileObject = new TornadoTileObject();
        tornadoTileObject.SetRadius(radius);
        tornadoTileObject.SetDuration(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4)));
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
        //targetTile.structure.AddPOI(tornadoTileObject, targetTile);
        //GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool("TornadoObject", Vector3.zero, Quaternion.identity, targetTile.parentAreaMap.objectsParent);
        //TornadoVisual obj = go.GetComponent<TornadoVisual>();
        //obj.Initialize(targetTile, radius * 2, durationInTicks);
       
    }
    protected override void OnLevelUp() {
        base.OnLevelUp();
        if (level == 1) {
            durationInTicks = GameManager.Instance.GetTicksBasedOnHour(2);
        } else if (level == 2) {
            durationInTicks = GameManager.Instance.GetTicksBasedOnHour(4);
        } else {
            durationInTicks = GameManager.Instance.GetTicksBasedOnHour(6);
        }
    }
    public override void ShowRange(LocationGridTile targetTile) {
        base.ShowRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(radius, 0, true);
        InnerMapManager.Instance.HighlightTiles(tiles);
    }
    public override void HideRange(LocationGridTile targetTile) {
        base.HideRange(targetTile);
        List<LocationGridTile> tiles = targetTile.GetTilesInRadius(radius, 0, true);
        InnerMapManager.Instance.UnhighlightTiles(tiles);
    }
    public virtual bool CanTarget(LocationGridTile tile) {
        return tile.structure != null;
    }
    protected virtual bool CanPerformActionTowards(LocationGridTile tile) {
        return tile.structure != null;
    }
    #endregion
}

public class TornadoData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.TORNADO;
    public override string name => "Tornado";
    public override string description => "A destructive cyclone that deals heavy Normal damage to everything in its path.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public TornadoData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        TornadoTileObject tornadoTileObject = new TornadoTileObject();
        tornadoTileObject.SetRadius(1);
        tornadoTileObject.SetDuration(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4)));
        tornadoTileObject.SetGridTileLocation(targetTile);
        tornadoTileObject.OnPlacePOI();
        IncreaseThreatThatSeesTile(targetTile, 10);
        base.ActivateAbility(targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            return targetTile.structure != null;
        }
        return canPerform;
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(1, tile);
    }
}
