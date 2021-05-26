using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public abstract class AnimalDen : NaturalStructure {
        
        public LocationStructureObject structureObj {get; private set;}

        #region getters
        public override System.Type serializedData => typeof(SaveDataAnimalDen);
        #endregion
        
        public AnimalDen(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) { }
        public AnimalDen(Region location, SaveDataNaturalStructure data) : base(location, data) { }
        
        public override void CenterOnStructure() {
            if (InnerMapManager.Instance.isAnInnerMapShowing && InnerMapManager.Instance.currentlyShowingMap != region.innerMap) {
                InnerMapManager.Instance.HideAreaMap();
            }
            if (region.innerMap.isShowing == false) {
                InnerMapManager.Instance.ShowInnerMap(region);
            }
            if (structureObj != null) {
                InnerMapCameraMove.Instance.CenterCameraOn(structureObj.gameObject);
            } 
        }
        public override void ShowSelectorOnStructure() {
            Selector.Instance.Select(this);
        }
        
        #region Structure Object
        public virtual void SetStructureObject(LocationStructureObject structureObj) {
            this.structureObj = structureObj;
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion
    }
}