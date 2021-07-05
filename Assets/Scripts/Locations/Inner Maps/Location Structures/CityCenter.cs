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
        protected override void AfterStructureDestruction(Character p_responsibleCharacter = null) {
            base.AfterStructureDestruction(p_responsibleCharacter);
            Messenger.RemoveListener(Signals.DAY_STARTED, OnDayStarted);
        }

        public override void OnTileDamaged(LocationGridTile tile, int amount, bool isPlayerSource) {
            //emptied out on tile damaged function since city centers cannot be damaged
        }
        public override void OnTileRepaired(LocationGridTile tile, int amount) {
            //emptied out on tile repaired function since city centers cannot be repaired
        }

        #region IPlayerActionTarget
        public override void ConstructDefaultActions() {
            base.ConstructDefaultActions();
            AddPlayerAction(PLAYER_SKILL_TYPE.INDUCE_MIGRATION);
            AddPlayerAction(PLAYER_SKILL_TYPE.STIFLE_MIGRATION);
            //AddPlayerAction(PLAYER_SKILL_TYPE.SCHEME);
        }
        #endregion

        private void OnDayStarted() {
            Area hex = occupiedArea;
            LocationGridTile tile = hex.gridTileComponent.GetRandomTileThatIsPassableAndHasNoObjectAndIsInWilderness();
            if(tile != null) {
                int numberOfHerbPlants = hex.tileObjectComponent.GetNumberOfTileObjectsInHexTile(TILE_OBJECT_TYPE.HERB_PLANT);
                if(numberOfHerbPlants < 4) {
                    tile.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.HERB_PLANT), tile);
                }
            }
        }
    }
}