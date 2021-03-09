using UnityEngine;
using System.Collections.Generic;

namespace Inner_Maps.Location_Structures {
    public class DefensePoint : PartyStructure {
        public DefensePoint(Region location) : base(STRUCTURE_TYPE.DEFENSE_POINT, location) {
            
        }
        public DefensePoint(Region location, SaveDataDemonicStructure data) : base(location, data) {
            
        }

    }
}