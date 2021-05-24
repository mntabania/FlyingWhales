using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;
using UnityEngine.Assertions;
using UtilityScripts;
namespace Player_Input {
    public class PickPortalInputModule : PlayerInputModule {

        private System.Action _onPortalPlacedAction;
        private LocationStructureObject _portalPrefab;
        private LocationGridTile _lastHoveredTile;
        private bool _canPlacePortalOnCurrentTile;
        private string cannotPerformReason;

        public PickPortalInputModule() {
            _portalPrefab = InnerMapManager.Instance.GetStructurePrefabsForStructure(STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE).First().GetComponent<LocationStructureObject>();
            Assert.IsNotNull(_portalPrefab);
        }

        public void AddOnPortalPlacedAction(System.Action p_action) {
            _onPortalPlacedAction += p_action;
        }
        public void RemoveOnPortalPlacedAction(System.Action p_action) {
            _onPortalPlacedAction -= p_action;
        }
        
        public override void OnUpdate() {
            LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
            if (hoveredTile != null && !UIManager.Instance.IsMouseOnUI()) {
                if (_lastHoveredTile != hoveredTile) {
                    _lastHoveredTile = hoveredTile;
                    Area area = hoveredTile.area;
                    bool hasEnoughSpace = _portalPrefab.HasEnoughSpaceIfPlacedOn(hoveredTile, out var noSpaceReason);
                    bool canBuildDemonicStructureOnArea = area.structureComponent.CanBuildDemonicStructureHere(STRUCTURE_TYPE.THE_PORTAL, out var notValidAreaReason);
                    _canPlacePortalOnCurrentTile = hasEnoughSpace && canBuildDemonicStructureOnArea;
                    if (!_canPlacePortalOnCurrentTile) {
                        if (!string.IsNullOrEmpty(noSpaceReason)) {
                            cannotPerformReason = noSpaceReason;    
                        } else if (!string.IsNullOrEmpty(notValidAreaReason)) {
                            cannotPerformReason = notValidAreaReason;    
                        } else {
                            cannotPerformReason = string.Empty;    
                        }
                    } else {
                        cannotPerformReason = string.Empty;
                    }
                    var color = _canPlacePortalOnCurrentTile ? GameUtilities.GetValidTileHighlightColor() : GameUtilities.GetInvalidTileHighlightColor();
                    InputManager.Instance.SetCursorTo(_canPlacePortalOnCurrentTile ? InputManager.Cursor_Type.Check : InputManager.Cursor_Type.Cross);
                    PlayerManager.Instance.SetStructurePlacementVisualHighlightColor(color);
                }
                if (Input.GetMouseButtonDown(0)) {
                    if (_canPlacePortalOnCurrentTile ) {
                        AskForPlacePortalConfirmation(hoveredTile);    
                    } else {
                        if (!string.IsNullOrEmpty(cannotPerformReason)) {
                            InnerMapManager.Instance.ShowAreaMapTextPopup(cannotPerformReason, hoveredTile.centeredWorldLocation, Color.white);
                        }    
                    }
                }
            } else {
                _lastHoveredTile = null;
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                TileHighlighter.Instance.HideHighlight();
            }
        }

        private void AskForPlacePortalConfirmation(LocationGridTile p_tile) {
            PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(false);
            UIManager.Instance.ShowYesNoConfirmation("Build Portal", "Are you sure you want to build your portal here?", 
                () => PlacePortal(p_tile), OnClickNo, showCover: true, layer: 50, onClickCloseAction: OnClickClose, onHideUIAction: OnHideUI);
        }
        private void OnClickNo() {
            PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(true);
        }
        private void OnClickClose() {
            PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(true);
        }
        private void OnHideUI() {
            PlayerManager.Instance.SetStructurePlacementVisualFollowMouseState(true);
        }
        
        private void PlacePortal(LocationGridTile p_tile) {
            PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(p_tile.area);
            playerSettlement.SetName("Demonic Intrusion");
            PlayerManager.Instance.InitializePlayer(p_tile.area);
        
            // p_tile.area.gridTileComponent.StartCorruption(p_tile.area);
            p_tile.InstantPlaceDemonicStructure(new StructureSetting(STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE));
            _onPortalPlacedAction?.Invoke();
        }
    }
}