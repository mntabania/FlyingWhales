using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Defiler : DemonicStructure {
        public override Vector2 selectableSize { get; }
        
        public Defiler(Region location) : base(STRUCTURE_TYPE.DEFILER, location) {
            selectableSize = new Vector2(10f, 10f);
        }
        public Defiler(Region location, SaveDataLocationStructure data) : base(location, data) {
            selectableSize = new Vector2(10f, 10f);
        }
        
        public override void OnCharacterUnSeizedHere(Character character) {
            base.OnCharacterUnSeizedHere(character);
            if (character.gridTileLocation != null && IsTilePartOfARoom(character.gridTileLocation, out var room)) {
                DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                door?.Close();
                if (character.partyComponent.hasParty) {
                    character.partyComponent.currentParty.RemoveMember(character);
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
            if (structure == this && character.isNormalCharacter && IsTilePartOfARoom(character.gridTileLocation, out var room) && room is DefilerRoom) {
                DoorTileObject door = room.GetTileObjectInRoom<DoorTileObject>(); //close door in room
                door?.Close();
            }
        }


        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new DefilerRoom(tilesInRoom);
        }
        #endregion
    }
}