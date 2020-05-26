using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Inn : ManMadeStructure{
        
        // public override Vector2 selectableSize { get; }
        // public override Vector3 worldPosition => structureObj.transform.position;

        public Inn(Region location) : base(STRUCTURE_TYPE.INN, location) { }
        public Inn(Region location, SaveDataLocationStructure data) : base(location, data) { }
    }
}