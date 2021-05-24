using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class ButchersShop : ManMadeStructure {
        public override Vector3 worldPosition => structureObj.transform.position;
        
        public ButchersShop(Region location) : base(STRUCTURE_TYPE.BUTCHERS_SHOP, location) {
            SetMaxHPAndReset(4000);
        }
        public ButchersShop(Region location, SaveDataManMadeStructure data) : base(location, data) {
            SetMaxHP(4000);
        }
    }
}