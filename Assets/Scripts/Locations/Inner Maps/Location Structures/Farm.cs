using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Farm : LocationStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Farm(Region location) : base(STRUCTURE_TYPE.FARM, location){
            selectableSize = new Vector2(5f, 5f);
        }
    }
}