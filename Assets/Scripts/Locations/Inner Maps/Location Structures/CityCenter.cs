using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

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

        public override void Initialize() {
            base.Initialize();
            Messenger.AddListener(Signals.DAY_STARTED, OnDayStarted);
        }
        protected override void AfterStructureDestruction() {
            base.AfterStructureDestruction();
            Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        }

        #region IPlayerActionTarget
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME);
        }
        #endregion

        private void OnDayStarted() {
            if (GameUtilities.RollChance(50)) {
                Area hex = occupiedArea;
                LocationGridTile tile = hex.gridTileComponent.GetRandomTileThatMeetCriteria(t => t.tileObjectComponent.objHere == null && t.structure != this && t.IsPassable());
                if(tile != null) {
                    int numberOfHerbPlants = hex.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.HERB_PLANT);
                    if(numberOfHerbPlants < 4) {
                        tile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HERB_PLANT), tile);
                    }
                }
            }
        }
    }
}