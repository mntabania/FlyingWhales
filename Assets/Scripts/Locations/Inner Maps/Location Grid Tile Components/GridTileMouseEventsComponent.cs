using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inner_Maps {
    public class GridTileMouseEventsComponent : LocationGridTileComponent {
        public bool hasMouseEvents { get; private set; }
        public bool isHovering { get; private set; }
        private LocationGridTileMouseEvents _mouseEvents;

        public GridTileMouseEventsComponent() {
        }
        public GridTileMouseEventsComponent(SaveDataGridTileMouseEventsComponent data) {
            hasMouseEvents = data.hasMouseEvents;
        }

        #region Utilities
        public void SetMouseEventsForAllNeighbours(bool state) {
            for (int i = 0; i < owner.neighbourList.Count; i++) {
                owner.neighbourList[i].mouseEventsComponent.SetHasMouseEvents(state);
            }
        }
        public void UpdateHasMouseEventsForAllNeighbours() {
            for (int i = 0; i < owner.neighbourList.Count; i++) {
                owner.neighbourList[i].mouseEventsComponent.UpdateHasMouseEvents();
            }
        }
        public void UpdateHasMouseEventsForSelfAndAllNeighbours() {
            owner.mouseEventsComponent.UpdateHasMouseEvents();
            for (int i = 0; i < owner.neighbourList.Count; i++) {
                owner.neighbourList[i].mouseEventsComponent.UpdateHasMouseEvents();
            }
        }
        public void UpdateHasMouseEvents() {
            bool shouldHaveMouseEvents = true;
            if (!owner.corruptionComponent.isCorrupted) {
                if (!owner.corruptionComponent.IsTileAdjacentToACorruption()) {
                    shouldHaveMouseEvents = false;
                }
            }
            if (owner.tileObjectComponent.objHere != null) {
                shouldHaveMouseEvents = false;
            }
            SetHasMouseEvents(shouldHaveMouseEvents);
        }
        public void SetHasMouseEvents(bool state) {
            if(hasMouseEvents != state) {
                hasMouseEvents = state;
                // Debug.Log($"Set has mouse events of {owner.ToString()} as {hasMouseEvents.ToString()}");
                if (hasMouseEvents) {
                    InitiateMouseEventsGO();
                } else {
                    DestroyMouseEventsGO();
                }
            }
        }
        private void InitiateMouseEventsGO() {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(InnerMapManager.Instance.gridTileMouseEventsPrefab.name, Vector3.zero, Quaternion.identity);
            go.name = $"{owner.ToString()} MouseEvents";
            _mouseEvents = go.GetComponent<LocationGridTileMouseEvents>();
            _mouseEvents.SetOwner(owner);
            go.transform.SetParent(owner.parentMap.structureParent);
            go.transform.position = owner.centeredWorldLocation;
        }
        private void DestroyMouseEventsGO() {
            ObjectPoolManager.Instance.DestroyObject(_mouseEvents.gameObject);
            _mouseEvents.SetOwner(null);
            _mouseEvents = null;
        }
        #endregion

        #region Mouse Events
        public void OnRightClick() {
            if (!GameManager.Instance.gameHasStarted) {
                return;
            }
            //int mouseOnUIOrMapObjectValue = UIManager.Instance.GetMouseOnUIOrMapObjectValue();
            if (!UIManager.Instance.IsMouseOnUIOrMapObject()) { // mouseOnUIOrMapObjectValue == -1
                if (owner.corruptionComponent.CanCorruptTile()) {
                    owner.corruptionComponent.StartCorruption(true);
                } else if (owner.corruptionComponent.CanDisruptCorruptionOfTile()) {
                    owner.corruptionComponent.DisruptCorruption();
                } else if (owner.corruptionComponent.CanBuildDemonicWall()) {
                    owner.corruptionComponent.StartBuildDemonicWall();
                }
            } 
            //else if (mouseOnUIOrMapObjectValue == 2) {
            //    if (owner.corruptionComponent.CanDestroyDemonicWall()) {
            //        owner.corruptionComponent.StartDestroyDemonicWall();
            //    }
            //}
        }
        public void OnHoverEnter() {
            if (!GameManager.Instance.gameHasStarted) {
                return;
            }
            //int mouseOnUIOrMapObjectValue = UIManager.Instance.GetMouseOnUIOrMapObjectValue();
            if (!isHovering) {
                if (!UIManager.Instance.IsMouseOnUIOrMapObject()) { //mouseOnUIOrMapObjectValue == -1
                    isHovering = true;
                    if (owner.corruptionComponent.CanCorruptTile()) {
                        OnHoverEnterTileAdjacentToCorruption();
                    } else if (owner.corruptionComponent.CanDisruptCorruptionOfTile()) {
                        OnHoverEnterBeingCorrupted();
                    } else if (owner.corruptionComponent.CanBuildDemonicWall()) {
                        OnHoverEnterCorrupted();
                    }
                }
            }
            //else if (mouseOnUIOrMapObjectValue == 2) {
            //    isHovering = true;
            //    if (owner.corruptionComponent.CanDestroyDemonicWall()) {
            //        OnHoverEnterDemonicWall();
            //    }
            //}
        }
        public void OnHoverExit() {
            if (isHovering) {
                isHovering = false;
                UIManager.Instance.HideSmallInfo();
            }
            //if (owner.corruptionComponent.IsTileAdjacentToACorruption() && !owner.corruptionComponent.isCorrupted && !owner.corruptionComponent.isCurrentlyBeingCorrupted) {
            //    OnHoverExitTileAdjacentToCorruption();
            //} else if (owner.corruptionComponent.isCurrentlyBeingCorrupted) {
            //    OnHoverExitBeingCorrupted();
            //}
        }
        #endregion

        #region Corruption Mouse Events
        private void OnHoverEnterTileAdjacentToCorruption() {
            UIManager.Instance.ShowSmallInfo("Right click to corrupt tile");
        }
        private void OnHoverEnterBeingCorrupted() {
            UIManager.Instance.ShowSmallInfo("Right click to undo corruption");
        }
        private void OnHoverEnterCorrupted() {
            UIManager.Instance.ShowSmallInfo("Right click to build wall");
        }
        private void OnHoverEnterDemonicWall() {
            UIManager.Instance.ShowSmallInfo("Right click to destroy wall");
        }
        #endregion

        #region Loading
        public void LoadSecondWave() {
            if (hasMouseEvents) {
                InitiateMouseEventsGO();
            }
        }
        #endregion
    }
    public class SaveDataGridTileMouseEventsComponent : SaveData<GridTileMouseEventsComponent>
    {
        public bool hasMouseEvents;

        public override void Save(GridTileMouseEventsComponent data) {
            base.Save(data);
            hasMouseEvents = data.hasMouseEvents;
        }
        public override GridTileMouseEventsComponent Load() {
            GridTileMouseEventsComponent component = new GridTileMouseEventsComponent(this);
            return component;
        }
    }
}