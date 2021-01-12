using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Tannery : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Tannery(Region location) : base(STRUCTURE_TYPE.TANNERY, location) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHPAndReset(8000);
        }
        public Tannery(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHP(8000);
        }
    }
}