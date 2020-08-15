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
            }
        }

        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new DefilerRoom(tilesInRoom);
        }
        #endregion
    }
}