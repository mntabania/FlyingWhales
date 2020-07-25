using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Temple : ManMadeStructure{

        public Temple(Region location) : base(STRUCTURE_TYPE.TEMPLE, location) { }
        public Temple(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}