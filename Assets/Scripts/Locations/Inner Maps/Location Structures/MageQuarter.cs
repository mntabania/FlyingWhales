using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class MageQuarter : LocationStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public MageQuarter(ILocation location) : base(STRUCTURE_TYPE.MAGE_QUARTERS, location){
            selectableSize = new Vector2(10f, 9f);
        }
    }
}