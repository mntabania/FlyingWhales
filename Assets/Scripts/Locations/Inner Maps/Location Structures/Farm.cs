using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Farm : LocationStructure {
        public override Vector2 selectableSize { get; }
        public Farm(ILocation location) : base(STRUCTURE_TYPE.FARM, location){
            selectableSize = new Vector2(5f, 5f);
        }
        
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            Vector3 position = structureObj.transform.position;
            worldPosition = position;
        }
    }
}