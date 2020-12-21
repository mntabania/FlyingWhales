using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class CityCenter : ManMadeStructure {
        
        public CityCenter(Region location) : base(STRUCTURE_TYPE.CITY_CENTER, location) {
            wallsAreMadeOf = RESOURCE.WOOD;
        }

        public CityCenter(Region location, SaveDataManMadeStructure data) : base(location, data) {
            wallsAreMadeOf = RESOURCE.WOOD;
        }
        public FACILITY_TYPE GetMostNeededValidFacility() {
            return FACILITY_TYPE.NONE;
        }

        public List<LocationGridTile> GetUnoccupiedFurnitureSpotsThatCanProvide(FACILITY_TYPE type) {
            return null;
        }
        public bool HasFacilityDeficit() {
            return false;
        }
        public LocationStructure GetLocationStructure() {
            return this;
        }

        #region IPlayerActionTarget
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME);
        }
        #endregion
    }
}