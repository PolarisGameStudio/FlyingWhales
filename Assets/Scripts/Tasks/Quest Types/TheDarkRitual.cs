﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class TheDarkRitual : Quest {

	public TheDarkRitual(TaskCreator createdBy) : base(createdBy, QUEST_TYPE.THE_DARK_RITUAL) {
		_alignment = new List<ACTION_ALIGNMENT>() {
			ACTION_ALIGNMENT.VILLAINOUS,
		};

		QuestPhase phase1 = new QuestPhase(this, "Search for an Inimical Incantation Book");
		phase1.AddTask(new Search(createdBy, 5, "Book of Inimical Incantations", null, this));

		QuestPhase phase2 = new QuestPhase(this, "Perform Ritual");
		phase2.AddTask(new DoRitual(createdBy, 5, this));

		_phases.Add(phase1);
		_phases.Add(phase2);
	}

	#region Overrides
	public override bool CanAcceptQuest (Character character){
		if(base.CanAcceptQuest (character)){
			if(character.HasTag(CHARACTER_TAG.RITUALIST)){
				return true;
			}
		}
		return false;
	}
	#endregion
}