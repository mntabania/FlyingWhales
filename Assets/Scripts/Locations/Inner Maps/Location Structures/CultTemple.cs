using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class CultTemple : ManMadeStructure{

        public CultTemple(Region location) : base(STRUCTURE_TYPE.CULT_TEMPLE, location) { 
            SetMaxHPAndReset(6000);
        }
        public CultTemple(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(6000);
        }
    }
}