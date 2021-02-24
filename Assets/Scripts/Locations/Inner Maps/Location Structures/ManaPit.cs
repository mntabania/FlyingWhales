using UnityEngine;

namespace Inner_Maps.Location_Structures {
    public class ManaPit : DemonicStructure {
        public ManaPit(Region location) : base(STRUCTURE_TYPE.MANA_PIT, location) { }
        public ManaPit(Region location, SaveDataDemonicStructure data) : base(location, data) { }
    }
}