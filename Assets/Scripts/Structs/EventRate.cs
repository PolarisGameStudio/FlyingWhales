﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct EventRate {
	public EVENT_TYPES eventType;
	public int rate;
	public int interval;
	public KINGDOM_RELATIONSHIP_STATUS[] relationshipTargets;
	public KINGDOM_TYPE[] kingdomTypes;
	public MILITARY_STRENGTH[] militaryStrength;

	public EventRate(EVENT_TYPES eventType, int rate, KINGDOM_RELATIONSHIP_STATUS[] relationshipTargets, KINGDOM_TYPE[] kingdomTypes, MILITARY_STRENGTH[] militaryStrength) {
		this.eventType = eventType;
		this.rate = rate;
		this.relationshipTargets = relationshipTargets;
		this.kingdomTypes = kingdomTypes;
		this.militaryStrength = militaryStrength;
		this.interval = this.rate;
//		this.interval = this.rate;
	}

	internal void DefaultValues(){
		this.eventType = EVENT_TYPES.NONE;
		this.rate = 0;
		this.relationshipTargets = null;
		this.kingdomTypes = null;
		this.militaryStrength = null;
//		this.multiplier = 1;
	}

	internal void ResetRateAndMultiplier(){
		this.rate = 0;
	}
}