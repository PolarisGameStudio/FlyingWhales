﻿using UnityEngine;
using System.Collections;

public class MildPsytoxin : CharacterTag {
	private int chanceToWorsenCase;

	public MildPsytoxin(ECS.Character character): base(character, CHARACTER_TAG.MILD_PSYTOXIN){
		chanceToWorsenCase = 5;
	}

	#region Overrides
	public override void Initialize (){
		base.Initialize ();
		ScheduleAggravateCheck ();
	}
	#endregion

	private void ScheduleAggravateCheck(){
		GameDate newSched = GameManager.Instance.FirstDayOfTheMonth();
		newSched.AddMonths (1);
		SchedulingManager.Instance.AddEntry (newSched, () => AggravateCheck ());
	}

	private void AggravateCheck(){
		if(_isRemoved){
			return;
		}
		int chance = UnityEngine.Random.Range (0, 100);
		if(chance < chanceToWorsenCase){
			WorsenCase ();
		}else{
			chanceToWorsenCase += 1;
			ScheduleAggravateCheck ();
		}
	}
	private void WorsenCase(){
		_character.AddHistory ("Psytoxin has worsen! It is now moderate!");
		_character.AssignTag (CHARACTER_TAG.MODERATE_PSYTOXIN);
		_character.RemoveCharacterTag (this);
	}

	internal void TriggerWorsenCase(){
		int chance = Utilities.rng.Next (0, 100);
		if(chance < chanceToWorsenCase){
			WorsenCase ();
		}
	}
}