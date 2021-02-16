using Inner_Maps;
using Locations.Settlements;
using Ruinarch;
namespace Player_Input {
    public class SpellInputModule : PlayerInputModule {
        public override void OnUpdate() {
            if (UIManager.Instance.IsMouseOnUI() || !InnerMapManager.Instance.isAnInnerMapShowing) {
                InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default); 
                PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();
            } else { 
                LocationGridTile hoveredTile = InnerMapManager.Instance.GetTileFromMousePosition();
                bool canTarget = false; 
                IPointOfInterest hoveredPOI = InnerMapManager.Instance.currentlyHoveredPoi; 
                string hoverText = string.Empty; 
                for (int i = 0; i < PlayerManager.Instance.player.currentActivePlayerSpell.targetTypes.Length; i++) {
                    switch (PlayerManager.Instance.player.currentActivePlayerSpell.targetTypes[i]) { 
                        case SPELL_TARGET.CHARACTER: 
                        case SPELL_TARGET.TILE_OBJECT: 
                            if (hoveredPOI != null) { 
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredPOI, ref hoverText); 
                            } 
                            break; 
                        case SPELL_TARGET.TILE: 
                            if (hoveredTile != null) {
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile); 
                            } 
                            break; 
                        case SPELL_TARGET.AREA: 
                            if (hoveredTile != null) { 
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(hoveredTile.area); 
                            } 
                            break;
                        case SPELL_TARGET.SETTLEMENT:
                            BaseSettlement settlement = null;
                            if (hoveredTile != null && hoveredTile.IsPartOfSettlement(out settlement)) {
                                canTarget = PlayerManager.Instance.player.currentActivePlayerSpell.CanTarget(settlement);
                            }
                            break;
                    }
                    InputManager.Instance.SetCursorTo(canTarget ? InputManager.Cursor_Type.Check : InputManager.Cursor_Type.Cross);
                }
                if (canTarget) {
                    PlayerManager.Instance.player.currentActivePlayerSpell.HighlightAffectedTiles(hoveredTile);
                } else {
                    if (hoveredTile == null || PlayerManager.Instance.player.currentActivePlayerSpell.InvalidHighlight(hoveredTile, ref hoverText) == false) {
                        PlayerManager.Instance.player.currentActivePlayerSpell.UnhighlightAffectedTiles();    
                    }
                }
                if(!string.IsNullOrEmpty(hoverText)) {
                    UIManager.Instance.ShowSmallInfo(hoverText);
                } else { 
                    UIManager.Instance.HideSmallInfo(); 
                } 
            }
        }
    }
}