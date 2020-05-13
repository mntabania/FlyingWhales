using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public abstract class StructureRoom : IPlayerActionTarget, ISelectable {
        
        public string name { get; }
        public List<LocationGridTile> tilesInRoom { get; }
        public List<SPELL_TYPE> actions { get; }
        public Vector3 worldPosition { get; }
        public Vector2 selectableSize { get; }
        public List<Character> charactersInRoom => GetCharactersInRoom();
        
        protected StructureRoom(string name, List<LocationGridTile> tilesInRoom) {
            this.name = name;
            this.tilesInRoom = tilesInRoom;
            actions = new List<SPELL_TYPE>();
            worldPosition = GetCenterTile().centeredWorldLocation;
            int maxX = tilesInRoom.Max(t => t.localPlace.x);
            int minX = tilesInRoom.Min(t => t.localPlace.x);
            int maxY = tilesInRoom.Max(t => t.localPlace.y);
            int minY = tilesInRoom.Min(t => t.localPlace.y);
            selectableSize = new Vector2((maxX - minX) + 1, (maxY - minY) + 1);
            Initialize();
        }

        #region Initialization
        public void Initialize() {
            ConstructDefaultActions();
        }
        #endregion

        #region Player Action Target
        public virtual void ConstructDefaultActions() { }
        public void AddPlayerAction(SPELL_TYPE action) {
            if (actions.Contains(action) == false) {
                actions.Add(action);
            }
        }
        public void RemovePlayerAction(SPELL_TYPE action) {
            actions.Remove(action);
        }
        public void ClearPlayerActions() {
            actions.Clear();
        }
        #endregion

        #region Selectable
        public bool IsCurrentlySelected() {
            return UIManager.Instance.structureRoomInfoUI.activeRoom == this;
        }
        public void LeftSelectAction() {
            UIManager.Instance.ShowStructureRoomInfo(this);
        }
        public void RightSelectAction() { }
        public bool CanBeSelected() {
            return true;
        }
        #endregion
        
        #region Characters
        private List<Character> GetCharactersInRoom() {
            List<Character> characters = new List<Character>();
            for (int i = 0; i < tilesInRoom.Count; i++) {
                characters.AddRange(tilesInRoom[i].charactersHere);
            }
            return characters;
        } 
        #endregion

        #region Tile Objects
        public T GetTileObjectInRoom<T>() where T : TileObject{
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile tile = tilesInRoom[i];
                if (tile.objHere != null && tile.objHere is T obj) {
                    return obj;
                }
            }
            return null;
        }
        #endregion

        #region Utilities
        protected LocationGridTile GetCenterTile() {
            return GameUtilities.GetCenterTile(tilesInRoom, tilesInRoom[0].parentMap.map);
        }
        #endregion
    }
}