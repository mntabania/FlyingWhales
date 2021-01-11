using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class CraftingStructure : ManMadeStructure {
        public override Vector2 selectableSize { get; }
        public override Vector3 worldPosition => structureObj.transform.position;
        public CraftingStructure(Region location) : base(STRUCTURE_TYPE.CRAFTING, location) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHPAndReset(8000);
        }
        public CraftingStructure(Region location, SaveDataManMadeStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 7f);
            SetMaxHP(8000);
        }
    }
}