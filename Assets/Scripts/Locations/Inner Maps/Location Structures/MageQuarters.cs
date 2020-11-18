﻿using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class MageQuarters : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public MageQuarters(Region location) : base(STRUCTURE_TYPE.MAGE_QUARTERS, location){
            selectableSize = new Vector2(10f, 9f);
            SetMaxHPAndReset(8000);
        }
        public MageQuarters(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 9f);
            SetMaxHP(8000);
        }
    }
}