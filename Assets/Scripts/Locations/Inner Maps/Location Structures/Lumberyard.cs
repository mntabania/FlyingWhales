using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Lumberyard : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public Lumberyard(Region location) : base(STRUCTURE_TYPE.LUMBERYARD, location){
            selectableSize = new Vector2(10f, 7f);
            SetMaxHPAndReset(8000);
        }
        public Lumberyard(Region location, SaveDataLocationStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHP(8000);
        }
    }
}