using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Farm : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Farm(Region location) : base(STRUCTURE_TYPE.FARM, location){
            selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public Farm(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(5f, 5f);
            wallsAreMadeOf = RESOURCE.WOOD;
        }
    }
}