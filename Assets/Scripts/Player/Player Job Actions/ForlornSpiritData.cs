﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;

public class ForlornSpiritData : SpellData {
    public override SPELL_TYPE ability => SPELL_TYPE.FORLORN_SPIRIT;
    public override string name { get { return "Forlorn Spirit"; } }
    public override string description { get { return "Forlorn Spirit"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SABOTAGE; } }
    public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public override int abilityRadius => 1;

    public ForlornSpiritData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        ForlornSpirit spirit = InnerMapManager.Instance.CreateNewTileObject<ForlornSpirit>(TILE_OBJECT_TYPE.FORLORN_SPIRIT);
        spirit.SetGridTileLocation(targetTile);
        spirit.OnPlacePOI();
        // targetTile.structure.AddPOI(spirit, targetTile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        return targetTile.structure != null && targetTile.objHere == null;
    }
}
