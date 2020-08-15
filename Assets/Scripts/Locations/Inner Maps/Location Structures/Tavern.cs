using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Tavern : ManMadeStructure{
        
        // public override Vector2 selectableSize { get; }
        // public override Vector3 worldPosition => structureObj.transform.position;

        public Tavern(Region location) : base(STRUCTURE_TYPE.TAVERN, location) {
            SetMaxHPAndReset(8000);
        }
        public Tavern(Region location, SaveDataLocationStructure data) : base(location, data) {
            SetMaxHP(8000);
        }
    }
}