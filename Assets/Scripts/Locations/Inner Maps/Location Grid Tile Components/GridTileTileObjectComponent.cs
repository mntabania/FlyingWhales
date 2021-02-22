using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps {
    public class GridTileTileObjectComponent : LocationGridTileComponent {

        public GridTileTileObjectComponent() {
        }
        public GridTileTileObjectComponent(SaveDataGridTileTileObjectComponent data) {

        }

        #region Utilities
        public bool HasWalls() {
            return owner.walls.Count > 0 || owner.objHere is BlockWall;
        }
        public TileObject GetFirstWall() {
            if(owner.objHere is BlockWall) {
                return owner.objHere;
            } else if (owner.walls.Count > 0) {
                return owner.walls[0];
            }
            return null;
        }
        #endregion

        #region Loading
        public void LoadSecondWave() {
        }
        #endregion
    }
    public class SaveDataGridTileTileObjectComponent : SaveData<GridTileTileObjectComponent> {
        public override void Save(GridTileTileObjectComponent data) {
            base.Save(data);

        }
        public override GridTileTileObjectComponent Load() {
            GridTileTileObjectComponent component = new GridTileTileObjectComponent(this);
            return component;
        }
    }
}