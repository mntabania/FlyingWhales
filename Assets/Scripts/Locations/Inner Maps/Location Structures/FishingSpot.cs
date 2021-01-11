using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class FishingSpot : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public FishingSpot(Region location) : base(STRUCTURE_TYPE.FISHING_SPOT, location) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHPAndReset(8000);
        }
        public FishingSpot(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHP(8000);
        }
    }
}