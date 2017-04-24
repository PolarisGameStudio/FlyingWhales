﻿using UnityEngine;
using System.Collections;

public class King : Role {

	public Kingdom ownedKingdom;

	public King(Citizen citizen): base(citizen){
		this.citizen.isKing = true;
		this.citizen.city.kingdom.king = this.citizen;
		this.SetOwnedKingdom(this.citizen.city.kingdom);
	}

	internal void SetOwnedKingdom(Kingdom ownedKingdom){
		this.ownedKingdom = ownedKingdom;
	}
}
