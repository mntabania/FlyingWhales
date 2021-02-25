using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class DefensePoint : DemonicStructure {
        public DefensePoint(Region location) : base(STRUCTURE_TYPE.DEFENSE_POINT, location) { }
        public DefensePoint(Region location, SaveDataDemonicStructure data) : base(location, data) { }
    }
}