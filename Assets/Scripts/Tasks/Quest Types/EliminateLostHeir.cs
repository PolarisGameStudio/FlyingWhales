﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ECS;

public class EliminateLostHeir : Quest {

	private Character _chieftain, _falseHeir, _lostHeir;

	public EliminateLostHeir(TaskCreator createdBy, Character chieftain, Character falseHeir, Character lostHeir) : base(createdBy, QUEST_TYPE.ELIMINATE_LOST_HEIR) {
		_alignment = new List<ACTION_ALIGNMENT>() {
			ACTION_ALIGNMENT.UNLAWFUL,
			ACTION_ALIGNMENT.VILLAINOUS
		};
		_chieftain = chieftain;
		_falseHeir = falseHeir;
		_lostHeir = lostHeir;
		_filters = new TaskFilter[] {
			new MustBeFaction((createdBy as Character).faction),
			new MustNotBeCharacter(falseHeir)
		};

		QuestPhase phase1 = new QuestPhase(this, "Search for Heirloom Necklace");
		phase1.AddTask(new Search(createdBy, 5, "Heirloom Necklace", null, this));

		QuestPhase phase2 = new QuestPhase(this, "Attack " + _lostHeir.name + "!");
		phase2.AddTask(new Attack(createdBy, _lostHeir, 5, this));

		MoveTo moveTo = new MoveTo (createdBy, -1, this);
		moveTo.SetForGameOnly (true);
		phase2.AddTask(moveTo);

		QuestPhase phase3 = new QuestPhase(this, "Report to the Successor");
		phase3.AddTask (new Report (createdBy, _falseHeir, this));

		_phases.Add(phase1);
		_phases.Add(phase2);
		_phases.Add(phase3);
	}
}