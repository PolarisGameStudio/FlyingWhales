﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class SummonPlayerSkill : SpellData {
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SUMMON; } }
    public RACE race { get; protected set; }
    public string className { get; protected set; }
    public SUMMON_TYPE summonType { get; protected set; }
    public virtual string bredBehaviour => CharacterManager.Instance.GetCharacterClass(className).traitNameOnTamedByPlayer;

    public SummonPlayerSkill() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, homeRegion: targetTile.parentMap.region as Region, className: className);
        summon.OnSummonAsPlayerMonster();
        CharacterManager.Instance.PlaceSummon(summon, targetTile);
        if (targetTile.structure?.settlementLocation != null && 
            targetTile.structure.settlementLocation.locationType != LOCATION_TYPE.SETTLEMENT) {
            summon.MigrateHomeStructureTo(targetTile.structure);	
        } else {
            summon.AddTerritory(targetTile.collectionOwner.partOfHextile.hexTileOwner, false);    
        }
        summon.jobQueue.CancelAllJobs();
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summon);
        // Messenger.Broadcast(Signals.PLAYER_GAINED_SUMMON, summon);
        base.ActivateAbility(targetTile);
    }
    public override void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        Summon summon = CharacterManager.Instance.CreateNewSummon(summonType, PlayerManager.Instance.player.playerFaction, homeRegion: targetTile.parentMap.region as Region, className: className);
        CharacterManager.Instance.PlaceSummon(summon, targetTile);
        //summon.behaviourComponent.AddBehaviourComponent(typeof(DefaultMinion));
        spawnedCharacter = summon;
        Messenger.Broadcast(Signals.PLAYER_PLACED_SUMMON, summon);
        base.ActivateAbility(targetTile, ref spawnedCharacter);
    }
    public override void HighlightAffectedTiles(LocationGridTile tile) {
        TileHighlighter.Instance.PositionHighlight(0, tile);
    }
    public override bool CanPerformAbilityTowards(LocationGridTile targetTile) {
        bool canPerform = base.CanPerformAbilityTowards(targetTile);
        if (canPerform) {
            if (targetTile.structure is Kennel) {
                return false;
            }
            if (targetTile.structure.IsTilePartOfARoom(targetTile, out var structureRoom)) {
                if (structureRoom is DefilerRoom) {
                    return false;
                } else if (structureRoom is PrisonCell) {
                    return false;
                }
            }
            if (!targetTile.collectionOwner.isPartOfParentRegionMap) {
                //only allow summoning on linked tiles
                return false;
            }
            if (bredBehaviour == "Defender") {
                //if minion is defender then do not allow it to be spawned on villages.
                return !targetTile.IsPartOfActiveHumanElvenSettlement();
            }
            return true;
        }
        return false;
    }
}