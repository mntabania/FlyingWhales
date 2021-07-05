using System.Collections;
using System.Collections.Generic;
using Ruinarch;
using UnityEngine;

namespace Inner_Maps {
    public class GridTileMouseEventsComponent : LocationGridTileComponent {
        public bool hasMouseEvents { get; private set; }
        public bool isHovering { get; private set; }
        private LocationGridTileMouseEvents _mouseEvents;

        public GridTileMouseEventsComponent() { }
        public GridTileMouseEventsComponent(SaveDataGridTileMouseEventsComponent data) {
            hasMouseEvents = data.hasMouseEvents;
        }

        private void SubscribeToShiftKeyListeners() {
            Messenger.AddListener(ControlsSignals.LEFT_SHIFT_DOWN, OnShiftDown);
            Messenger.AddListener(ControlsSignals.LEFT_SHIFT_UP, OnShiftUp);
            if (InputManager.Instance.isShiftDown) {
                OnShiftDown();
            }
        }
        private void UnsubscribeToShiftKeyListeners() {
            Messenger.AddListener(ControlsSignals.LEFT_SHIFT_DOWN, OnShiftDown);
            Messenger.AddListener(ControlsSignals.LEFT_SHIFT_UP, OnShiftUp);
            OnShiftUp();
        }
        
        private void OnShiftUp() {
            if (owner.tileObjectComponent.objHere != null) {
                if (owner.tileObjectComponent.objHere.mapObjectVisual is TileObjectGameObject tileObjectGameObject) {
                    tileObjectGameObject.MakeObjectClickable();
                }    
            }
            
        }
        private void OnShiftDown() {
            if (owner.tileObjectComponent.objHere != null) {
                if (owner.tileObjectComponent.objHere.mapObjectVisual is TileObjectGameObject tileObjectGameObject) {
                    tileObjectGameObject.MakeObjectUnClickable();
                }
            }
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
            if (owner.structure.structureType != STRUCTURE_TYPE.WILDERNESS) {
                shouldHaveMouseEvents = false;
            }
            // if (owner.tileObjectComponent.objHere != null) {
            //     shouldHaveMouseEvents = false;
            // }
            SetHasMouseEvents(shouldHaveMouseEvents);
        }
        public void SetHasMouseEvents(bool state) {
            if(hasMouseEvents != state) {
                hasMouseEvents = state;
                // Debug.Log($"Set has mouse events of {owner.ToString()} as {hasMouseEvents.ToString()}");
                if (hasMouseEvents) {
                    InitiateMouseEventsGO();
                    SubscribeToShiftKeyListeners();
                } else {
                    DestroyMouseEventsGO();
                    UnsubscribeToShiftKeyListeners();
                }
            }
        }
        private void InitiateMouseEventsGO() {
            GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(InnerMapManager.Instance.gridTileMouseEventsPrefab.name, Vector3.zero, Quaternion.identity);
            go.name = $"{owner.ToString()} MouseEvents";
            _mouseEvents = go.GetComponent<LocationGridTileMouseEvents>();
            _mouseEvents.SetOwner(owner);
            go.transform.SetParent(owner.parentMap.structureTilemap.transform);
            // go.transform.SetAsFirstSibling();
            go.transform.position = owner.centeredWorldLocation;
            // Debug.Log($"Mouse events of {owner}");
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
            if (InputManager.Instance.isShiftDown || !UIManager.Instance.IsMouseOnUIOrMapObject()) { // mouseOnUIOrMapObjectValue == -1
                if (owner.corruptionComponent.CanCorruptTile()) {
                    Cost corruptCost = EditableValuesManager.Instance.GetCorruptTileCost();
                    if (PlayerManager.Instance.player.CanAfford(corruptCost)) {
                        PlayerManager.Instance.player.ReduceCurrency(corruptCost);
                        owner.corruptionComponent.StartCorruption(true);    
                    } else {
                        InnerMapManager.Instance.ShowAreaMapTextPopup($"Not enough {UtilityScripts.Utilities.NotNormalizedConversionEnumToString(corruptCost.currency.ToString())}",
                            owner.centeredWorldLocation, Color.white);
                        AudioManager.Instance.OnErrorSoundPlay();
                    }
                } else if (owner.corruptionComponent.CanDisruptCorruptionOfTile()) {
                    //return cost on disrupt corruption
                    Cost corruptCost = EditableValuesManager.Instance.GetCorruptTileCost();
                    if (corruptCost.currency == CURRENCY.Chaotic_Energy) {
                        //This is so that refunding will not affect spirit energy
                        PlayerManager.Instance.player.plagueComponent.AdjustPlaguePointsWithoutAffectingSpiritEnergy(corruptCost.processedAmount);    
                    } else {
                        PlayerManager.Instance.player.AddCurrency(corruptCost);
                    }
                    owner.corruptionComponent.DisruptCorruption();
                } else if (owner.corruptionComponent.CanBuildDemonicWall()) {
                    Cost buildWallCost = EditableValuesManager.Instance.GetBuildWallCost();
                    if (PlayerManager.Instance.player.CanAfford(buildWallCost)) {
                        PlayerManager.Instance.player.ReduceCurrency(buildWallCost);
                        owner.corruptionComponent.StartBuildDemonicWall();
                    } else {
                        InnerMapManager.Instance.ShowAreaMapTextPopup($"Not enough {UtilityScripts.Utilities.NotNormalizedConversionEnumToString(buildWallCost.currency.ToString())}",
                            owner.centeredWorldLocation, Color.white);
                        AudioManager.Instance.OnErrorSoundPlay();
                    }
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
                bool isMouseOnUIOrMapObject = UIManager.Instance.IsMouseOnUIOrMapObject();
                if (InputManager.Instance.isShiftDown || !isMouseOnUIOrMapObject) { //mouseOnUIOrMapObjectValue == -1
                    isHovering = true;
                    if (owner.corruptionComponent.CanCorruptTile()) {
                        OnHoverEnterTileAdjacentToCorruption();
                    } else if (owner.corruptionComponent.CanDisruptCorruptionOfTile()) {
                        OnHoverEnterBeingCorrupted();
                    } else if (owner.corruptionComponent.CanBuildDemonicWall()) {
                        OnHoverEnterCorrupted();
                    } 
                    // else {
                    //     UIManager.Instance.ShowSmallInfo($"{owner} hovered part 1. is Shift down {InputManager.Instance.isShiftDown.ToString()}. Is Mouse On UI: {isMouseOnUIOrMapObject.ToString()}");    
                    // }
                }
                // else {
                //     UIManager.Instance.ShowSmallInfo($"{owner} hovered part 2. is Shift down {InputManager.Instance.isShiftDown.ToString()}. Is Mouse On UI: {isMouseOnUIOrMapObject.ToString()}");
                // }
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
            Cost corruptCost = EditableValuesManager.Instance.GetCorruptTileCost();
            UIManager.Instance.ShowSmallInfo($"Right click to corrupt tile. Cost: {corruptCost.GetCostStringWithIcon()}");
        }
        private void OnHoverEnterBeingCorrupted() {
            UIManager.Instance.ShowSmallInfo("Right click to undo corruption");
        }
        private void OnHoverEnterCorrupted() {
            Cost buildWallCost = EditableValuesManager.Instance.GetBuildWallCost();
            UIManager.Instance.ShowSmallInfo($"Right click to build wall. Cost: {buildWallCost.GetCostStringWithIcon()}");
        }
        private void OnHoverEnterDemonicWall() {
            UIManager.Instance.ShowSmallInfo("Right click to destroy wall");
        }
        #endregion

        #region Loading
        public void LoadSecondWave() {
            if (hasMouseEvents) {
                InitiateMouseEventsGO();
                SubscribeToShiftKeyListeners();
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