﻿using Inner_Maps;

public class FireBallData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.FIRE_BALL;
    public override string name => "Fire Ball";
    public override string description => "This Spell spawns a floating ball of fire that will move around randomly for a few hours, dealing Fire damage to everything in its path.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public FireBallData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        FireBall fireBall = new FireBall();
        fireBall.SetGridTileLocation(targetTile);
        fireBall.OnPlacePOI();
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