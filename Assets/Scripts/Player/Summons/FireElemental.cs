﻿using Inner_Maps;
using Traits;

public class FireElemental : Summon {

    public const string ClassName = "Fire Elemental";
    
    public override string raceClassName => $"Fire Elemental";
    
    public FireElemental() : base(SUMMON_TYPE.Fire_Elemental, ClassName, RACE.ELEMENTAL, UtilityScripts.Utilities.GetRandomGender()) { }
    public FireElemental(string className) : base(SUMMON_TYPE.Fire_Elemental, className, RACE.ELEMENTAL, UtilityScripts.Utilities.GetRandomGender()) { }
    public FireElemental(SaveDataSummon data) : base(data) { }

    public override void Initialize() {
        base.Initialize();
        traitContainer.AddTrait(this, "Fireproof");
    }
    public override void SubscribeToSignals() {
        base.SubscribeToSignals();
        Messenger.AddListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    public override void UnsubscribeSignals() {
        base.UnsubscribeSignals();
        Messenger.RemoveListener<ActualGoapNode>(Signals.CHARACTER_FINISHED_ACTION, OnCharacterFinishedAction);
    }
    private void OnCharacterFinishedAction(ActualGoapNode goapNode) {
        if (goapNode.actor == this && goapNode.action.goapType == INTERACTION_TYPE.STAND) {
            Burning burning = new Burning();
            burning.InitializeInstancedTrait();
            burning.SetSourceOfBurning(new BurningSource(), gridTileLocation.genericTileObject);
            gridTileLocation.genericTileObject.traitContainer.AddTrait(gridTileLocation.genericTileObject, burning, this, bypassElementalChance: true);
        }
    }
}

