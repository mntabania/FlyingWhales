using System.Linq;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public abstract class AnimalDen : NaturalStructure {
        
        public LocationStructureObject structureObj {get; private set;}
        public string templateName { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject
        public Vector3 structureObjectWorldPos { get; private set; } //Do not save this since this will be filled up automatically upon loading in SetStructureObject

        #region getters
        public override System.Type serializedData => typeof(SaveDataAnimalDen);
        public override Vector2 selectableSize => structureObj.size;
        #endregion
        
        public AnimalDen(STRUCTURE_TYPE structureType, Region location) : base(structureType, location) { }
        public AnimalDen(Region location, SaveDataNaturalStructure data) : base(location, data) { }

        public override void OnBuiltNewStructure() {
            base.OnBuiltNewStructure();
            if (structureType.IsBeastDen()) {
                LinkThisStructureToVillages();
            }
        }
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
            templateName = structureObj.name;
            structureObjectWorldPos = structureObj.transform.position;
            Vector3 position = structureObj.transform.position;
            position.x -= 0.5f;
            position.y -= 0.5f;
            worldPosition = position;
        }
        #endregion

        protected void LinkThisStructureToVillages() {
            Area sourceArea = occupiedArea;
            for (int i = 0; i < region.villageSpots.Count; i++) {
                VillageSpot spot = region.villageSpots[i];
                Area targetArea = spot.mainSpot;
                if (sourceArea.GetAreaDistanceTo(targetArea) <= 6) {
                    spot.AddLinkedBeastDen(this);
                }
            }
        }
    }
}