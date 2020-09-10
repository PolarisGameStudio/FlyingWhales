﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Traits;
using UnityEngine;
public class PoisonCloudData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.POISON_CLOUD;
    public override string name => "Poison Cloud";
    public override string description => "This Spell spawns a 2x2 Poison Cloud.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public PoisonCloudData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        PoisonCloud poisonCloud = new PoisonCloud();
        poisonCloud.SetExpiryDate(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(2, 6))));
        poisonCloud.SetGridTileLocation(targetTile);
        poisonCloud.OnPlacePOI();
        poisonCloud.SetStacks(5);
        //IncreaseThreatThatSeesTile(targetTile, 10);
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