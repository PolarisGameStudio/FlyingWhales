﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Ruinarch;
using Traits;
using UnityEngine;
using Inner_Maps.Location_Structures;
using UnityEngine.Assertions;

public class PlayerSpell {

    //public PlayerJobData parentData { get; protected set; }
    //public Minion minion { get; protected set; }
    public SPELL_TYPE spellType { get; protected set; }
    public SPELL_CATEGORY spellCategory { get; protected set; }
    public string name { get { return PlayerSkillManager.Instance.allSpellsData[spellType].name; } }
    public string description { get { return PlayerSkillManager.Instance.allSpellsData[spellType].description; } }
    public int tier { get; protected set; }
    public int abilityRadius { get; protected set; } //0 means single target
    public virtual string dynamicDescription { get { return description; } }
    public int cooldown { get; protected set; } //cooldown in ticks
    //public Character assignedCharacter { get; protected set; }
    public SPELL_TARGET[] targetTypes { get; protected set; } //what sort of object does this action target
    public bool isActive { get; protected set; }
    public int ticksInCooldown { get; private set; } //how many ticks has this action been in cooldown?
    public int level { get; protected set; }
    //public List<ABILITY_TAG> abilityTags { get; protected set; }
    //public bool hasSecondPhase { get; protected set; }
    //public bool isInSecondPhase { get; protected set; }

    #region getters/setters
    public string worldObjectName {
        get { return name; }
    }
    public bool isInCooldown {
        get { return ticksInCooldown != cooldown; } //check if the ticks this action has been in cooldown is the same as cooldown
    }
    #endregion



    //public void SetParentData(PlayerJobData data) {
    //    parentData = data;
    //}
    //public void SetMinion(Minion minion) {
    //    this.minion = minion;
    //}

    public PlayerSpell(SPELL_TYPE spellType) {
        this.spellType = spellType;
        //this.name = Utilities.NormalizeStringUpperCaseFirstLetters(this.abilityType.ToString());
        //abilityTags = new List<ABILITY_TAG>();
        this.level = 1;
        this.tier = PlayerManager.Instance.GetSpellTier(spellType);
        this.abilityRadius = 0;
        //hasSecondPhase = false;
        OnLevelUp();
    }

    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        OnLevelUp();
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        OnLevelUp();
    }

    #region Virtuals
    public virtual void ActivateAction() { //this is called when the actions button is pressed
        if (this.isActive) { //if this action is still active, deactivate it first
            DeactivateAction();
        }
        //this.assignedCharacter = assignedCharacter;
        isActive = true;
        //parentData.SetActiveAction(this);
        //ActivateCooldown();
        //Messenger.AddListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.AddListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
        PlayerManager.Instance.player.ConsumeAbility(this);
    }
    public virtual void ActivateAction(IPointOfInterest targetPOI) { //this is called when the actions button is pressed
        ActivateAction();
    }
    public virtual void ActivateAction(NPCSettlement targetNpcSettlement) { //this is called when the actions button is pressed
        ActivateAction();
    }
    public virtual void ActivateAction(LocationGridTile targetTile) { 
        ActivateAction();
    }
    public virtual void ActivateAction(List<IPointOfInterest> targetPOIs) {
        ActivateAction();
    }
    public virtual void DeactivateAction() { //this is typically called when the character is assigned to another action or the assigned character dies
        isActive = false;
        //parentData.SetActiveAction(null);
        //Messenger.RemoveListener<Character>(Signals.CHARACTER_DEATH, OnCharacterDied);
        //Messenger.RemoveListener<JOB, Character>(Signals.CHARACTER_UNASSIGNED_FROM_JOB, OnCharacterUnassignedFromJob);
    }
    protected virtual void OnCharacterDied(Character characterThatDied) {
        //if (assignedCharacter != null && characterThatDied.id == assignedCharacter.id) {
        //    DeactivateAction();
        //    //ResetCooldown(); //only reset cooldown if the assigned character dies
        //}
    }
    /// <summary>
    /// Can this action currently be performed.
    /// </summary>
    /// <returns>True or False</returns>
    public virtual bool CanPerformAction() {
        if (isInCooldown) {
            return false;
        }
        return true;
    }
    protected virtual bool CanPerformActionTowards(Character targetCharacter) {
        if (targetCharacter.traitContainer.HasTrait("Blessed")) {
            return false;
        }
        //Quick fix only, remove this later
        if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
            return false;
        }
        return CanPerformAction();
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="poi">The target poi</param>
    /// <returns>true or false</returns>
    public virtual bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi.traitContainer.HasTrait("Blessed")) {
            hoverText = "Blessed characters cannot be targetted.";
            return false;
        }
        //Quick fix only, remove this later
        if (poi is Character) {
            Character targetCharacter = poi as Character;
            if(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
                return false;
            }
        }
        hoverText = string.Empty;
        return true;
    }
    protected virtual void OnLevelUp() { }
    public virtual void SecondPhase() { }
    /// <summary>
    /// If the ability has a range, override this to show that range. <see cref="InputManager.Update"/>
    /// </summary>
    public virtual void ShowRange(LocationGridTile tile) { }
    public virtual void HideRange(LocationGridTile tile) { }
    #endregion

    #region Cooldown
    protected void SetDefaultCooldownTime(int cooldown) {
        this.cooldown = cooldown;
        ticksInCooldown = cooldown;
    }
    private void ActivateCooldown() {
        ticksInCooldown = 0;
        //parentData.SetLockedState(true);
        //Messenger.AddListener(Signals.TICK_ENDED, CheckForCooldown); //IMPORTANT NOTE: Cooldown will start but will not actually finish because this line of code is removed. This is removed this so that the ability can only be used once. Upon every enter of the npcSettlement map, all cooldowns of intervention abilities must be reset
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_ACTIVATED, this);
    }
    private void CheckForCooldown() {
        if (ticksInCooldown == cooldown) {
            //cooldown has been reached!
            OnCooldownDone();
        } else {
            ticksInCooldown++;
        }
    }
    public void InstantCooldown() {
        ticksInCooldown = cooldown;
        OnCooldownDone();
    }
    private void OnCooldownDone() {
        //parentData.SetLockedState(false);
        //Messenger.RemoveListener(Signals.TICK_ENDED, CheckForCooldown);
        Messenger.Broadcast(Signals.JOB_ACTION_COOLDOWN_DONE, this);
    }
    private void ResetCooldown() {
        ticksInCooldown = cooldown;
        //parentData.SetLockedState(false);
    }
    #endregion
}

public class SpellData : IPlayerSkill {
    public virtual SPELL_TYPE type => SPELL_TYPE.NONE;
    public virtual string name { get { return string.Empty; } }
    public virtual string description { get { return string.Empty; } }
    public virtual SPELL_CATEGORY category { get { return SPELL_CATEGORY.NONE; } }
    //public virtual INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.NONE;
    public SPELL_TARGET[] targetTypes { get; protected set; }
    public int radius { get; protected set; }

    public int maxCharges { get; private set; }
    public int charges { get; private set; }
    public int manaCost { get; private set; }
    public int cooldown { get; private set; }
    public int threat { get; private set; }
    public int threatPerHour { get; private set; }
    public bool isInUse { get; private set; }

    public int currentCooldownTick { get; private set; }
    public bool hasCharges => maxCharges != -1;
    public bool hasCooldown => cooldown != -1;
    public bool hasManaCost => manaCost != -1;
    public virtual bool isInCooldown => hasCooldown && currentCooldownTick < cooldown;
    
    protected SpellData() {
        charges = -1;
        manaCost = -1;
        cooldown = -1;
        maxCharges = -1;
        threat = 0;
        threatPerHour = 0;
        currentCooldownTick = cooldown;
        isInUse = false;
    }

    #region Virtuals
    public virtual void ActivateAbility(IPointOfInterest targetPOI) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(LocationGridTile targetTile, ref Character spawnedCharacter) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(HexTile targetHex) {
        //if(targetHex.settlementOnTile != null) {
        //    if(targetHex.settlementOnTile.HasResidentInsideSettlement()){
        //        PlayerManager.Instance.player.threatComponent.AdjustThreat(20);
        //    }
        //}
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(LocationStructure targetStructure) {
        OnExecuteSpellActionAffliction();
    }
    public virtual void ActivateAbility(StructureRoom room) {
        OnExecuteSpellActionAffliction();
    }
    public virtual string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter) { return null; }
    public virtual bool CanPerformAbilityTowards(Character targetCharacter) {
        //(targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES)
        if(targetCharacter.traitContainer.HasTrait("Blessed")) {
            return false;
        }
        return CanPerformAbility();
    }
    public virtual bool CanPerformAbilityTowards(TileObject tileObject) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationGridTile targetTile) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(HexTile targetHex) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(LocationStructure targetStructure) { return CanPerformAbility(); }
    public virtual bool CanPerformAbilityTowards(StructureRoom room) { return CanPerformAbility(); }
    /// <summary>
    /// Highlight the affected area of this spell given a tile.
    /// </summary>
    /// <param name="tile">The tile to take into consideration.</param>
    public virtual void HighlightAffectedTiles(LocationGridTile tile) { }
    public virtual void UnhighlightAffectedTiles() {
        TileHighlighter.Instance.HideHighlight();
    }
    /// <summary>
    /// Show an invalid highlight.
    /// </summary>
    /// <param name="tile"></param>
    /// <returns>True or false (Whether or not this spell showed an invalid highlight)</returns>
    public virtual bool InvalidHighlight(LocationGridTile tile) { return false; }
    #endregion

    #region General
    public bool CanPerformAbilityTowards(IPointOfInterest poi) {
        if(poi.poiType == POINT_OF_INTEREST_TYPE.CHARACTER) {
            return CanPerformAbilityTowards(poi as Character);
        } else if (poi.poiType == POINT_OF_INTEREST_TYPE.TILE_OBJECT) {
            return CanPerformAbilityTowards(poi as TileObject);
        }
        return CanPerformAbility();
    }
    public bool CanPerformAbility() {
        return (!hasCharges || charges > 0) && (!hasManaCost || PlayerManager.Instance.player.mana >= manaCost) && (!hasCooldown || currentCooldownTick >= cooldown);
    }
    /// <summary>
    /// Function that determines whether this action can target the given character or not.
    /// Regardless of cooldown state.
    /// </summary>
    /// <param name="poi">The target poi</param>
    /// <returns>true or false</returns>
    public bool CanTarget(IPointOfInterest poi, ref string hoverText) {
        if (poi.traitContainer.HasTrait("Blessed")) {
            hoverText = "Blessed characters cannot be targetted.";
            return false;
        }
        //Quick fix only, remove this later
        if (poi is Character) {
            Character targetCharacter = poi as Character;
            if (targetCharacter.race != RACE.HUMANS && targetCharacter.race != RACE.ELVES) {
                return false;
            }
        }
        hoverText = string.Empty;
        return CanPerformAbilityTowards(poi);
    }
    public bool CanTarget(LocationGridTile tile) {
        return CanPerformAbilityTowards(tile);
    }
    public bool CanTarget(HexTile hex) {
        return CanPerformAbilityTowards(hex);
    }
    protected void IncreaseThreatForEveryCharacterThatSeesPOI(IPointOfInterest poi, int amount) {
        Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_POI, poi, amount);
    }
    //protected void IncreaseThreatThatSeesTile(LocationGridTile targetTile, int amount) {
    //    Messenger.Broadcast(Signals.INCREASE_THREAT_THAT_SEES_TILE, targetTile, amount);
    //}
    public void OnExecuteSpellActionAffliction() {
        if (PlayerSkillManager.Instance.unlimitedCast == false) {
            if(hasCharges && charges > 0) {
                charges--;
                Messenger.Broadcast(Signals.CHARGES_ADJUSTED, this);
            }
            if (hasManaCost) {
                PlayerManager.Instance.player.AdjustMana(-manaCost);
            }
            if (hasCooldown && charges <= 0) {
                currentCooldownTick = 0;
                Messenger.Broadcast(Signals.SPELL_COOLDOWN_STARTED, this);
                Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
            }
        }
        // PlayerManager.Instance.player.threatComponent.AdjustThreatPerHour(threatPerHour);
        PlayerManager.Instance.player.threatComponent.AdjustThreat(threat);
        //PlayerManager.Instance.player.threatComponent.AdjustThreat(20);

        if (category == SPELL_CATEGORY.PLAYER_ACTION) {
            Messenger.Broadcast(Signals.ON_EXECUTE_PLAYER_ACTION, this as PlayerAction);
        } else if (category == SPELL_CATEGORY.AFFLICTION) {
            Messenger.Broadcast(Signals.ON_EXECUTE_AFFLICTION, this);
        } else if (category == SPELL_CATEGORY.SPELL || category == SPELL_CATEGORY.MINION || category == SPELL_CATEGORY.SUMMON) {
            Messenger.Broadcast(Signals.ON_EXECUTE_SPELL, this);
        }
    }
    private void PerTickCooldown() {
        currentCooldownTick++;
        Assert.IsFalse(currentCooldownTick > cooldown, $"Cooldown tick became higher than cooldown in {name}. Cooldown is {cooldown.ToString()}. Cooldown Tick is {currentCooldownTick.ToString()}");
        if(currentCooldownTick == cooldown) {
            SetCharges(maxCharges);
            Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
            Messenger.Broadcast(Signals.SPELL_COOLDOWN_FINISHED, this);
            Messenger.Broadcast(Signals.FORCE_RELOAD_PLAYER_ACTIONS);
        }
    }
    public string GetManaCostChargesCooldownStr() {
        string str = "Mana Cost: " + manaCost;
        str += "\nCharges: " + charges;
        str += "\nCooldown: " + cooldown;
        return str;
    }
    #endregion

    #region Attributes
    public void SetMaxCharges(int amount) {
        maxCharges = amount;
    }
    public void SetCharges(int amount) {
        charges = amount;
    }
    public void AdjustCharges(int amount) {
        charges += amount;
        Messenger.Broadcast(Signals.CHARGES_ADJUSTED, this);
    }
    public void SetManaCost(int amount) {
        manaCost = amount;
    }
    public void SetCooldown(int amount) {
        cooldown = amount;
        currentCooldownTick = cooldown;
    }
    public void SetCurrentCooldownTick(int amount) {
        currentCooldownTick = amount;
    }
    public void SetThreat(int amount) {
        threat = amount;
    }
    public void SetThreatPerHour(int amount) {
        threatPerHour = amount;
    }
    public void SetIsInUse(bool state) {
        isInUse = state;
    }
    #endregion
}

public class BallLightningData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.BALL_LIGHTNING;
    public override string name => "Ball Lightning";
    public override string description => "This Spell spawns a floating ball of electricity that will move around randomly for a few hours, dealing Electric damage to everything in its path.";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public BallLightningData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        BallLightningTileObject ballLightning = new BallLightningTileObject();
        ballLightning.SetGridTileLocation(targetTile);
        ballLightning.OnPlacePOI();
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

public class FrostyFogData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.FROSTY_FOG;
    public override string name { get { return "Frosty Fog"; } }
    public override string description { get { return "Frosty Fog"; } }
    public override SPELL_CATEGORY category { get { return SPELL_CATEGORY.SPELL; } }
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;
    public virtual int abilityRadius => 1;

    public FrostyFogData() : base() {
        targetTypes = new[] { SPELL_TARGET.TILE };
    }
    public override void ActivateAbility(LocationGridTile targetTile) {
        FrostyFogTileObject frostyFog = new FrostyFogTileObject();
        frostyFog.SetGridTileLocation(targetTile);
        frostyFog.OnPlacePOI();
        frostyFog.SetStacks(EditableValuesManager.Instance.frostyFogStacks);
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

public class VaporData : SpellData {
    public override SPELL_TYPE type => SPELL_TYPE.VAPOR;
    public override string name => "Vapor";
    public override string description => "Vapor";
    public override SPELL_CATEGORY category => SPELL_CATEGORY.SPELL;
    //public override INTERVENTION_ABILITY_TYPE type => INTERVENTION_ABILITY_TYPE.SPELL;

    public VaporData() : base() {
        targetTypes = new SPELL_TARGET[] { SPELL_TARGET.TILE };
    }

    public override void ActivateAbility(LocationGridTile targetTile) {
        VaporTileObject vaporTileObject = new VaporTileObject();
        vaporTileObject.SetGridTileLocation(targetTile);
        vaporTileObject.OnPlacePOI();
        vaporTileObject.SetStacks(EditableValuesManager.Instance.vaporStacks);
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
        FireBallTileObject fireBallTileObject = new FireBallTileObject();
        fireBallTileObject.SetGridTileLocation(targetTile);
        fireBallTileObject.OnPlacePOI();
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

public class PlayerJobActionSlot {
    public int level;
    public PlayerSpell ability;

    public PlayerJobActionSlot() {
        level = 1;
        ability = null;
    }

    public void SetAbility(PlayerSpell ability) {
        this.ability = ability;
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
    }

    public void LevelUp() {
        level++;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_INTERVENE_LEVEL, this);
    }
    public void SetLevel(int amount) {
        level = amount;
        level = Mathf.Clamp(level, 1, PlayerDB.MAX_LEVEL_INTERVENTION_ABILITY);
        if (this.ability != null) {
            this.ability.SetLevel(level);
        }
        Messenger.Broadcast(Signals.PLAYER_GAINED_INTERVENE_LEVEL, this);
    }
}