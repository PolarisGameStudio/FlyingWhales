﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterTag {
	protected ECS.Character _character;
	protected string _name;
	protected CHARACTER_TAG _attribute;
	protected StatsModifierPercentage _statsModifierPercentage;
	protected bool _isRemoved;
    protected List<ACTION_TYPE> _grantedActionTypes;

	#region getters/setters
	public string name {
		get { return _name; }
	}
	public CHARACTER_TAG attribute {
		get { return _attribute; }
	}
	public ECS.Character character{
		get { return _character; }
	}
	//public List<CharacterTask> tagTasks {
	//	get { return _tagTasks; }
	//}
	public StatsModifierPercentage statsModifierPercentage {
		get { return _statsModifierPercentage; }
	}
	public bool isRemoved {
		get { return _isRemoved; }
	}
	#endregion

	public CharacterTag(ECS.Character character, CHARACTER_TAG attribute) {
		_character = character;
        _attribute = attribute;
        _name = Utilities.NormalizeStringUpperCaseFirstLetters (_attribute.ToString ());
		_statsModifierPercentage = new StatsModifierPercentage ();
		_isRemoved = false;
        _grantedActionTypes = new List<ACTION_TYPE>();

    }

	#region Virtuals
	public virtual void Initialize(){}
    /*
     What should happen when a tag is removed
         */
    public virtual void OnRemoveTag() {
		_isRemoved = true;
	}
    public virtual void PerformDailyAction() {}
	#endregion
}
