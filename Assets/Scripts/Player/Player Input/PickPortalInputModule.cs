using Inner_Maps;
using Locations.Settlements;
using Ruinarch;
using UnityEngine;
using UtilityScripts;
namespace Player_Input {
    public class PickPortalInputModule : PlayerInputModule {

        private System.Action _onPortalPlacedAction;

        public void AddOnPortalPlacedAction(System.Action p_action) {
            _onPortalPlacedAction += p_action;
        }
        public void RemoveOnPortalPlacedAction(System.Action p_action) {
            _onPortalPlacedAction -= p_action;
        }
        
        public override void OnUpdate() {
            LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
            if (hoveredTile != null && !UIManager.Instance.IsMouseOnUI()) {
                Area area = hoveredTile.area;
                bool canBuildDemonicStructureHere = area.structureComponent.CanBuildDemonicStructureHere(STRUCTURE_TYPE.THE_PORTAL);
                var color = canBuildDemonicStructureHere ? GameUtilities.GetValidTileHighlightColor() : GameUtilities.GetInvalidTileHighlightColor();
                InputManager.Instance.SetCursorTo(canBuildDemonicStructureHere ? InputManager.Cursor_Type.Check : InputManager.Cursor_Type.Cross);
                TileHighlighter.Instance.PositionHighlight(area, color);
                if (canBuildDemonicStructureHere && Input.GetMouseButtonDown(0)) {
                    AskForPlacePortalConfirmation(area);
                }
            } else {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
                TileHighlighter.Instance.HideHighlight();
            }
        }

        private void AskForPlacePortalConfirmation(Area hexTile) {
            UIManager.Instance.ShowYesNoConfirmation("Build Portal", "Are you sure you want to build your portal here?", () => PlacePortal(hexTile), showCover: true, layer: 50);
        }
        
        private void PlacePortal(Area p_tile) {
            p_tile.SetElevation(ELEVATION.PLAIN);
            PlayerSettlement playerSettlement = LandmarkManager.Instance.CreateNewPlayerSettlement(p_tile);
            playerSettlement.SetName("Demonic Intrusion");
            WorldConfigManager.Instance.mapGenerationData.portal = p_tile;
            PlayerManager.Instance.InitializePlayer(p_tile);
        
            p_tile.gridTileComponent.StartCorruption(p_tile);
            LandmarkManager.Instance.PlaceBuiltStructureForSettlement(p_tile.settlementOnArea, p_tile.region.innerMap, p_tile, STRUCTURE_TYPE.THE_PORTAL, RESOURCE.NONE);
            _onPortalPlacedAction?.Invoke();
        }
    }
}