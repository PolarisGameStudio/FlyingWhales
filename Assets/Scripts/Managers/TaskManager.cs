﻿using UnityEngine;
using System.Collections;

public enum PILLAGE_ACTION { OBTAIN_ITEM, END, CIVILIAN_DIES, NOTHING }
public enum HUNT_ACTION { EAT, END, NOTHING }

public class TaskManager : MonoBehaviour {
	public static TaskManager Instance;

	private WeightedDictionary<PILLAGE_ACTION> _pillageActions;
	private WeightedDictionary<HUNT_ACTION> _huntActions;
	private WeightedDictionary<string> _vampiricEmbraceActions;
	private WeightedDictionary<string> _drinkBloodActions;
	private WeightedDictionary<string> _hypnotizeActions;

	#region getters/setters
	public WeightedDictionary<PILLAGE_ACTION> pillageActions{
		get { return _pillageActions; }
	}
	public WeightedDictionary<HUNT_ACTION> huntActions{
		get { return _huntActions; }
	}
	public WeightedDictionary<string> vampiricEmbraceActions{
		get { return _vampiricEmbraceActions; }
	}
	public WeightedDictionary<string> drinkBloodActions{
		get { return _drinkBloodActions; }
	}
	public WeightedDictionary<string> hypnotizeActions{
		get { return _hypnotizeActions; }
	}
	#endregion
	void Awake(){
		Instance = this;
	}
		
	internal void Initialize(){
		ConstructData ();
	}

	private void ConstructData(){
		_pillageActions = new WeightedDictionary<PILLAGE_ACTION>();
		_pillageActions.AddElement(PILLAGE_ACTION.OBTAIN_ITEM, 20);
		_pillageActions.AddElement(PILLAGE_ACTION.END, 15);
		_pillageActions.AddElement(PILLAGE_ACTION.CIVILIAN_DIES, 15);
		_pillageActions.AddElement(PILLAGE_ACTION.NOTHING, 70);

		_huntActions = new WeightedDictionary<HUNT_ACTION>();
		_huntActions.AddElement(HUNT_ACTION.EAT, 15);
		_huntActions.AddElement(HUNT_ACTION.END, 15);
		_huntActions.AddElement(HUNT_ACTION.NOTHING, 70);

		_vampiricEmbraceActions = new WeightedDictionary<string>();
		_vampiricEmbraceActions.AddElement ("turn", 15);
		_vampiricEmbraceActions.AddElement ("caught", 15);
		_vampiricEmbraceActions.AddElement("nothing", 50);

		_drinkBloodActions = new WeightedDictionary<string>();
		_drinkBloodActions.AddElement ("drink", 15);
		_drinkBloodActions.AddElement ("caught", 10);
		_drinkBloodActions.AddElement("nothing", 50);

		_hypnotizeActions = new WeightedDictionary<string>();
		_hypnotizeActions.AddElement ("hypnotize", 15);
		_hypnotizeActions.AddElement("nothing", 50);
	}
}