using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class FishingShack : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Ocean connectedOcean { get; private set; } //TODO:
        
        public FishingShack(Region location) : base(STRUCTURE_TYPE.FISHING_SHACK, location) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHPAndReset(8000);
        }
        public FishingShack(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHP(8000);
        }
    }
}