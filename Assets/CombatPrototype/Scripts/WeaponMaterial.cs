﻿using UnityEngine;
using System.Collections;

namespace ECS{
	[System.Serializable]
	public class WeaponMaterial {
		//The stats here is for NORMAL quality
		public MATERIAL material;
		public int power;
		public int durability;
		public int cost;
	}
}

