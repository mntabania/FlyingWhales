using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Workshop : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Workshop(Region location) : base(STRUCTURE_TYPE.WORKSHOP, location) {
            SetMaxHPAndReset(8000);
        }
        public Workshop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}