using System.Collections.Generic;
using UnityEngine;
namespace Inner_Maps.Location_Structures {
    public class Defiler : DemonicStructure {
        public override Vector2 selectableSize { get; }
        
        public Defiler(Region location) : base(STRUCTURE_TYPE.DEFILER, location) {
            selectableSize = new Vector2(10f, 10f);
        }

        #region Rooms
        protected override StructureRoom CreteNewRoomForStructure(List<LocationGridTile> tilesInRoom) {
            return new DefilerRoom(tilesInRoom);
        }
        #endregion
    }
}