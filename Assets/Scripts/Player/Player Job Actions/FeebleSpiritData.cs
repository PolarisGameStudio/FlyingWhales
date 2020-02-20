﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class FeebleSpiritData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.FEEBLE_SPIRIT;
    public override string name { get { return "Feeble Spirit"; } }
    public override string description { get { return "Feeble Spirit"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public FeebleSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        FeebleSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<FeebleSpirit>(TILE_OBJECT_TYPE.FEEBLE_SPIRIT);
        spirit.SetGridTileLocation(targetTile);
        spirit.OnPlacePOI();
        // targetTile.structure.AddPOI(spirit, targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null && targetTile.objHere == null;
    }
}
