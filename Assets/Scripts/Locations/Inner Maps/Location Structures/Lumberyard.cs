﻿using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {
        public override Vector3 worldPosition => structureObj.transform.position;
        public Lumberyard(Region location) : base(STRUCTURE_TYPE.LUMBERYARD, location){
            SetMaxHPAndReset(8000);
        }
        public Lumberyard(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}