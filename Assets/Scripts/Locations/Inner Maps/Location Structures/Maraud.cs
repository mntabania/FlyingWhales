using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class Maraud : DemonicStructure {
        public Maraud(Region location) : base(STRUCTURE_TYPE.MARAUD, location) { }
        public Maraud(Region location, SaveDataDemonicStructure data) : base(location, data) { }
    }
}