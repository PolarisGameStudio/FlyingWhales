﻿using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Prison : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Prison(Region location) : base(STRUCTURE_TYPE.PRISON, location){
            selectableSize = new Vector2(13f, 10f);
            SetMaxHPAndReset(8000);
        }
    }
}