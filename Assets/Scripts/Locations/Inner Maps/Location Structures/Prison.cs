using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public class Prison : ManMadeStructure {
        public override Vector3 worldPosition => structureObj.transform.position;
        public Prison(Region location) : base(STRUCTURE_TYPE.PRISON, location){
            SetMaxHPAndReset(8000);
        }
        public Prison(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}