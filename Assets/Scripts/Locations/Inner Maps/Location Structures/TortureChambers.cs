using System.Collections.Generic;
using System.Linq;
using Traits;
using UnityEngine;
using UnityEngine.Assertions;
namespace Inner_Maps.Location_Structures {
    public class TortureChambers : DemonicStructure {
        public override Vector2 selectableSize { get; }
        private TortureChamberStructureObject _tortureChamberStructureObject;
        public LocationGridTile entrance => _tortureChamberStructureObject.entrance;
        public override string nameplateName => "Prison";
        public TortureChambers(Region location) : base(STRUCTURE_TYPE.TORTURE_CHAMBERS, location){
            selectableSize = new Vector2(10f, 10f);
            nameWithoutID = "Prison";
        }
        public TortureChambers(Region location, SaveDataLocationStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
        }

        public override void OnCharacterUnSeizedHere(Character character) {
            if (character.isNormalCharacter) {
                character.traitContainer.AddTrait(character, "Restrained");
                if (character.partyComponent.hasParty) {
                    character.partyComponent.currentParty.RemoveMember(character);
                }
                if (character.gridTileLocation != null && !character.gridTileLocation.charactersHere.Contains(character)) {
                    character.gridTileLocation.AddCharacterHere(character);
                }
            }
        }
        
        #region Listeners
        protected override void SubscribeListeners() {
            base.SubscribeListeners();
            Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        }
        protected override void UnsubscribeListeners() {
            base.UnsubscribeListeners();
            Messenger.RemoveListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
        }
        #endregion
        
        private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
            if (structure == this && character.isNormalCharacter && IsTilePartOfARoom(character.gridTileLocation, out var room) && room is PrisonCell prisonCell && prisonCell.skeleton == null) {
                DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                door?.Close();
            }
        }

        #region Structure Object
        public override void SetStructureObject(LocationStructureObject structureObj) {
            base.SetStructureObject(structureObj);
            _tortureChamberStructureObject = structureObj as TortureChamberStructureObject;
        }
        public override void OnBuiltNewStructure() {
            _tortureChamberStructureObject.SetEntrance(location.innerMap);
        }
        public override void OnDoneLoadStructure() {
            _tortureChamberStructureObject.SetEntrance(location.innerMap);
        }
        #endregion

        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new PrisonCell(tilesInRoom);
        }
        #endregion
    }
}