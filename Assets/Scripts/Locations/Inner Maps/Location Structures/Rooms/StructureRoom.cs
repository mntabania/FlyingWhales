using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;
namespace Inner_Maps.Location_Structures {
    public abstract class StructureRoom : IPlayerActionTarget, ISelectable {
        
        public string name { get; }
        public List<LocationGridTile> tilesInRoom { get; }
        public List<PLAYER_SKILL_TYPE> actions { get; }
        public Vector3 worldPosition { get; protected set; }
        public Vector2 selectableSize { get; protected set; }
        //public List<Character> charactersInRoom => GetCharactersInRoom();
        public LocationStructure parentStructure => tilesInRoom?[0].structure;
        
        protected StructureRoom(string name, List<LocationGridTile> tilesInRoom) {
            this.name = name;
            this.tilesInRoom = tilesInRoom;
            actions = new List<PLAYER_SKILL_TYPE>();
            worldPosition = GetCenterTile().centeredWorldLocation;
            int maxX = tilesInRoom.Max(t => t.localPlace.x);
            int minX = tilesInRoom.Min(t => t.localPlace.x);
            int maxY = tilesInRoom.Max(t => t.localPlace.y);
            int minY = tilesInRoom.Min(t => t.localPlace.y);
            selectableSize = new Vector2((maxX - minX) + 1, (maxY - minY) + 1);
            Initialize();
        }
        public StructureRoom(SaveDataStructureRoom data) {
            this.name = data.name;
            this.tilesInRoom = SaveUtilities.ConvertIDListToLocationGridTiles(data.tilesInRoom);
            actions = new List<PLAYER_SKILL_TYPE>();
            worldPosition = GetCenterTile().centeredWorldLocation;
            int maxX = tilesInRoom.Max(t => t.localPlace.x);
            int minX = tilesInRoom.Min(t => t.localPlace.x);
            int maxY = tilesInRoom.Max(t => t.localPlace.y);
            int minY = tilesInRoom.Min(t => t.localPlace.y);
            selectableSize = new Vector2((maxX - minX) + 1, (maxY - minY) + 1);
            Initialize();
        }

        #region Loading
        public virtual void LoadReferences(SaveDataStructureRoom saveDataStructureRoom) { }
        public virtual void LoadAdditionalReferences(SaveDataStructureRoom saveDataStructureRoom) { }
        #endregion
        
        #region Initialization
        private void Initialize() {
            ConstructDefaultActions();
        }
        #endregion

        #region Player Action Target
        public virtual void ConstructDefaultActions() { }
        public void AddPlayerAction(PLAYER_SKILL_TYPE action) {
            if (actions.Contains(action) == false) {
                actions.Add(action);
            }
        }
        public void RemovePlayerAction(PLAYER_SKILL_TYPE action) {
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
        public void RightSelectAction() {
            Vector3 worldPos = InnerMapCameraMove.Instance.camera.ScreenToWorldPoint(Input.mousePosition);
            UIManager.Instance.ShowPlayerActionContextMenu(this, worldPos, false);
        }
        public void MiddleSelectAction() { }
        public virtual bool CanBeSelected() {
            return true;
        }
        #endregion
        
        #region Characters
        //private List<Character> GetCharactersInRoom() {
        //    List<Character> characters = new List<Character>();
        //    for (int i = 0; i < tilesInRoom.Count; i++) {
        //        characters.AddRange(tilesInRoom[i].charactersHere);
        //    }
        //    return characters;
        //}
        public void PopulateCharactersInRoom(List<Character> p_characters) {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                p_characters.AddRange(tilesInRoom[i].charactersHere);
            }
        }
        public Character GetFirstAliveCharacterInRoom() {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (!c.isDead) {
                        return c;
                    }
                }
            }
            return null;
        }
        public bool HasCharacterInRoom() {
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile t = tilesInRoom[i];
                for (int j = 0; j < t.charactersHere.Count; j++) {
                    Character c = t.charactersHere[j];
                    if (!c.isDead) {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Tile Objects
        public T GetTileObjectInRoom<T>() where T : TileObject{
            for (int i = 0; i < tilesInRoom.Count; i++) {
                LocationGridTile tile = tilesInRoom[i];
                if (tile.tileObjectComponent.objHere != null && tile.tileObjectComponent.objHere is T obj) {
                    return obj;
                }
            }
            return null;
        }
        #endregion

        #region Utilities
        public LocationGridTile GetCenterTile() {
            return GameUtilities.GetCenterTile(tilesInRoom, tilesInRoom[0].parentMap.map);
        }
        public bool HasAnyAliveCharacterInRoom() {
            //return charactersInRoom.Any(c => !c.isDead);
            return GetFirstAliveCharacterInRoom() != null;
        }
        #endregion

        #region Seize
        public virtual bool CanUnseizeCharacterInRoom(Character character) {
            return true;
        }
        #endregion

        #region Destruction
        public virtual void OnParentStructureDestroyed() { }
        #endregion
    }
}

#region Save Data
public class SaveDataStructureRoom : SaveData<StructureRoom> {
    public string name;
    public List<string> tilesInRoom;
    public override void Save(StructureRoom data) {
        base.Save(data);
        name = data.name;
        tilesInRoom = SaveUtilities.ConvertSavableListToIDs(data.tilesInRoom);
    }
}
#endregion