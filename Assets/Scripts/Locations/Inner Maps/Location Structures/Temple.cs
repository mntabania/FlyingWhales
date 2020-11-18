using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Temple : ManMadeStructure{

        public Temple(Region location) : base(STRUCTURE_TYPE.TEMPLE, location) { 
            SetMaxHPAndReset(6000);
        }
        public Temple(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(6000);
        }
    }
}